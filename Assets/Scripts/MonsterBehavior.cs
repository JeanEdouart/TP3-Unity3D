using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Health))]
public class MonsterBehavior : MonoBehaviour
{
    public Animator animator;
    public Transform player;
    public bool debugLogs = true;

    [Header("Combat")]
    public int attackDamage = 10;
    public float attackInterval = 1.0f; // secondes entre attaques
    public float attackRange = 2.0f;

    [Header("Déplacement")]
    public bool matchPlayerSpeed = true;  // utilise la vitesse du joueur
    public float moveSpeed = 3.5f;        // vitesse de fallback si pas de PlayerBehaviour
    public float rotationSpeed = 8f;      // vitesse de rotation vers le joueur
    public float acceleration = 12f;      // lissage de la vitesse

    bool playerInZone = false;
    Coroutine attackRoutine;

    Health myHealth;
    Health playerHealth;
    PlayerBehaviour playerBehaviour;
    Rigidbody rb;
    float currentSpeed;
    public float leashDistance = 15f;  // distance pour perdre l'aggro
    public float stopBuffer = 0.9f;    // stop à 90% de la portée (anti-jitter)

    bool chasing = false;              // suit tant que pas “perdu”
    float desiredSpeed = 0f;           // vitesse demandée (lue par FixedUpdate)
    public bool disableRootMotion = true; // évite que l'anim pousse/recul le modèle
    public float attackStickExtra = 0.75f; // marge pour sortir de la zone d’attaque (hysteresis)
    bool inAttackZone = false;             // vrai une fois en portée, jusqu'à dépasser la marge


    void Awake()
    {
        myHealth = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();

    }

    void Start()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player)
        {
            playerHealth = player.GetComponent<Health>();
            playerBehaviour = player.GetComponent<PlayerBehaviour>();
            if (matchPlayerSpeed && playerBehaviour) moveSpeed = playerBehaviour.speed;
        }

        if (myHealth) myHealth.onDeath.AddListener(Die);

        if (debugLogs)
        {
            Debug.Log($"[Monster] animator={(animator ? animator.name : "NULL")}", this);
            Debug.Log($"[Monster] player={(player ? player.name : "NULL")}, playerHealth={(playerHealth ? "OK" : "NULL")}", this);
            Debug.Log($"[Monster] moveSpeed={moveSpeed} (matchPlayerSpeed={matchPlayerSpeed})", this);
        }
        if (animator && disableRootMotion) animator.applyRootMotion = false;

    }

    public void SetPlayerInZone(bool inZone)
    {
        if (debugLogs) Debug.Log($"[Monster] SetPlayerInZone({inZone})", this);

        playerInZone = inZone;

        if (inZone)
        {
            // Démarre/maintient la poursuite
            if (!chasing)
            {
                chasing = true;
                if (attackRoutine == null) attackRoutine = StartCoroutine(AttackLoop());
            }
        }
        else
        {
            // Ne PAS stopper tout de suite : le leash s'en occupe dans Update()
            // On peut juste baisser l'anim d'attaque tant qu'on n'est pas à portée
            animator?.SetBool("IsAttacking", false);
        }
    }


    // Déplacement + orientation (chaque frame)
    void Update()
    {
        if (debugLogs && Input.GetKeyDown(KeyCode.T))
            SetPlayerInZone(!Input.GetKey(KeyCode.LeftShift));

        if (myHealth.currentHealth <= 0 || player == null) return;

        // Suivre la vitesse du joueur en temps réel si demandé
        if (matchPlayerSpeed && playerBehaviour) moveSpeed = playerBehaviour.speed;

        // ...
        if (!chasing) return;

        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;

        // Leash: on ne perd l'aggro que si on est hors trigger ET trop loin
        if (!playerInZone && dist > leashDistance)
        {
            chasing = false;
            animator?.SetBool("IsAttacking", false);
            desiredSpeed = 0f;
            currentSpeed = 0f;
            if (attackRoutine != null) { StopCoroutine(attackRoutine); attackRoutine = null; }
            return;
        }

        // Rotation (yaw only)
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // ---- HYSTERESIS DE PORTÉE ----
        // On "entre" en zone d’attaque à attackRange
        // On n’en "sort" que si on dépasse attackRange + attackStickExtra
        float enterDist = attackRange;
        float exitDist = attackRange + attackStickExtra;

        if (!inAttackZone && dist <= enterDist) inAttackZone = true;
        else if (inAttackZone && dist > exitDist) inAttackZone = false;

        // Mouvement / Anim selon inAttackZone
        if (!inAttackZone)
        {
            // On n’est pas encore à portée : on avance
            currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * Time.deltaTime);
            desiredSpeed = currentSpeed;
            animator?.SetBool("IsAttacking", true);
        }
        else
        {
            // En portée : on ne bouge plus, on attaque
            currentSpeed = 0f;
            desiredSpeed = 0f;
            animator?.SetBool("IsAttacking", false);
        }

        // (option) locomotion anim :
        // animator?.SetFloat("MoveSpeed", currentSpeed);

    }


    void FixedUpdate()
    {
        if (!chasing || desiredSpeed <= 0f) return;

        Vector3 step = transform.forward * desiredSpeed * Time.fixedDeltaTime;
        if (rb != null && rb.isKinematic) rb.MovePosition(rb.position + step);
        else transform.position += step;
    }


    IEnumerator AttackLoop()
    {
        var wait = new WaitForSeconds(attackInterval);

        while (chasing && myHealth.currentHealth > 0 && playerHealth != null)
        {
            if (Vector3.Distance(transform.position, player.position) <= attackRange)
            {
                animator?.SetTrigger("Attack");
                playerHealth.TakeDamage(attackDamage);
            }
            yield return wait;
        }

    }

    public void DealDamage()
    {
        if (playerHealth == null || myHealth.currentHealth <= 0 || !playerInZone) return;
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
            playerHealth.TakeDamage(attackDamage);
    }

    void Die()
    {
        animator?.SetTrigger("Die");
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        this.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
