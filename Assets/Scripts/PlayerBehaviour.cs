using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerBehaviour : MonoBehaviour
{
    public float speed; // Speed of the player
    public float jumpForce; // Force applied when the player jumps
    private bool jump; // Flag to check if the player can jump

    [SerializeField] private Animator animator;   // <- Animator du CHILD (visuel)
    [SerializeField] private Health health;       // auto-hooké si laissé vide
    private Rigidbody rb;

    public int attackDamage = 15;
    public float attackRange = 0.5f;
    public float attackCooldown = 0.5f;
    public Transform attackOrigin;          // si null -> transform
    public LayerMask monsterMask;           // assigne "Monster" Layer ou Default

    private bool isDead = false;
    float nextTime;

    void Awake()
    {
        // Auto-hook de sécurité (si pas assigné dans l'inspector)
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!health) health = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (!attackOrigin) attackOrigin = transform;

        if (health != null)
            health.onDeath.AddListener(HandleDeath);
        else
            Debug.LogError("[Player] Health manquant sur le Player (parent).");
    }

    void Update()
    {
        // Test rapide (optionnel) : appuie sur K pour te tuer -> vérifie anim/lock
        if (!isDead && Input.GetKeyDown(KeyCode.K))
            health?.TakeDamage(9999);

        if (isDead) return;

        if (Time.time >= nextTime && Input.GetMouseButtonDown(0))
        {
            Vector3 center = attackOrigin.position + transform.forward * 1f;
            var hits = Physics.OverlapSphere(center, attackRange, monsterMask.value == 0 ? ~0 : monsterMask);

            foreach (var h in hits)
            {
                var hp = h.GetComponentInParent<Health>();
                if (hp != null) hp.TakeDamage(attackDamage);
            }

            nextTime = Time.time + attackCooldown;
        }

        if (Input.GetMouseButtonDown(0))
            animator.SetTrigger("Attack");
    }

    void OnDrawGizmosSelected()
    {
        Transform o = attackOrigin ? attackOrigin : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(o.position + o.forward * 1f, attackRange);
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            if (rb) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            return;
        }

        if (Input.GetKey(KeyCode.W)) animator.SetBool("RunForward", true); else animator.SetBool("RunForward", false);
        if (Input.GetKey(KeyCode.S)) animator.SetBool("RunBackward", true); else animator.SetBool("RunBackward", false);
        if (Input.GetKey(KeyCode.D)) animator.SetBool("RunLeft", true); else animator.SetBool("RunLeft", false);
        if (Input.GetKey(KeyCode.A)) animator.SetBool("RunRight", true); else animator.SetBool("RunRight", false);

        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(Horizontal, 0, Vertical);
        move = transform.rotation * move; // Appliquer la rotation du joueur
        transform.position += move * Time.deltaTime * speed;

        if (Input.GetKeyDown(KeyCode.Space) && jump)
        {
            animator.SetTrigger("Jump");
            jump = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            jump = true;
    }

    // --- Mort joueur ---
    public void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        // Stop net des mouvements/physiques
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll; // fige tout
        }

        if (animator)
        {
            bool hasDie = false;
            foreach (var p in animator.parameters)
                if (p.type == AnimatorControllerParameterType.Trigger && p.name == "Die") { hasDie = true; break; }

            if (hasDie) animator.SetTrigger("Die");
            else Debug.LogWarning("[Player] Pas de Trigger 'Die' dans l'Animator. Vérifie le nom/param et la transition Any State -> Death.");
        }
        else
        {
            Debug.LogWarning("[Player] Animator non assigné. Laisse le champ vide pour auto-hook, ou assigne l'Animator du child dans l'Inspector.");
        }
    }
}
