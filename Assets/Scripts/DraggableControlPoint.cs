using UnityEngine;

// Contrôle clavier d'un point de contrôle Bézier.
// Sélectionne le point au clic, puis déplace-le avec O/K/L/M.
// O = haut (avant), K = gauche, L = bas (arrière), M = droite
// Maj = accélérer. PageUp/PageDown = monter/descendre si lockY = false.
public class DraggableControlPoint : MonoBehaviour
{
    [Header("Déplacement")]
    public float moveSpeed = 2f;
    public float fastMultiplier = 3f;

    [Header("Axe vertical")]
    public bool lockY = true;   // garde Y constant
    public float yPlane = 0f;   // Y verrouillé quand lockY = true

    [Header("Sélection")]
    public bool alwaysActive = false;           // si true, ce point répond toujours aux touches
    public KeyCode toggleKey = KeyCode.Return;  // touche pour (dé)sélectionner sans souris

    private static DraggableControlPoint active;

    void Start()
    {
        if (lockY) yPlane = transform.position.y;
    }

    void OnDisable()
    {
        if (active == this) active = null;
    }

    void OnMouseDown()
    {
        active = this; // sélection par clic
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            active = (active == this) ? null : this;

        bool canControl = alwaysActive || active == this;
        if (!canControl) return;

        float mult = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? fastMultiplier : 1f;
        float step = moveSpeed * mult * Time.deltaTime;

        Vector3 delta = Vector3.zero;

        // Mapping demandé : O/K/L/M
        if (Input.GetKey(KeyCode.O)) delta += Vector3.forward * step; // haut
        if (Input.GetKey(KeyCode.L)) delta += Vector3.back * step; // bas
        if (Input.GetKey(KeyCode.K)) delta += Vector3.left * step; // gauche
        if (Input.GetKey(KeyCode.M)) delta += Vector3.right * step; // droite

        // Optionnel : contrôle vertical si lockY = false
        if (!lockY)
        {
            if (Input.GetKey(KeyCode.PageUp)) delta += Vector3.up * step;
            if (Input.GetKey(KeyCode.PageDown)) delta += Vector3.down * step;
        }

        if (delta.sqrMagnitude > 0f)
        {
            transform.position += delta;
            if (lockY) transform.position = new Vector3(transform.position.x, yPlane, transform.position.z);
        }
    }

    void OnDrawGizmos()
    {
        Color c = (active == this) ? Color.green : Color.yellow;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
