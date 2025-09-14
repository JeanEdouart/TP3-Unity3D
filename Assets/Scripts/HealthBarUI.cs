using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Health target;         // Le Health du perso
    public Slider slider;         // Le Slider de l'UI
    public Vector3 worldOffset = new Vector3(0, 2f, 0);

    Transform cam;

    void Start()
    {
        if (!target) target = GetComponentInParent<Health>();
        cam = Camera.main ? Camera.main.transform : null;

        if (target)
        {
            slider.maxValue = target.maxHealth;
            slider.value = target.currentHealth;
            target.OnHealthChanged += UpdateBar;
        }
    }
    void LateUpdate()
    {
        if (!target) return;

        // Position au-dessus de la tête
        transform.position = target.transform.position + worldOffset;

        // Toujours vertical : on ignore l'inclinaison en Y de la caméra
        if (cam)
        {
            Vector3 fwd = cam.forward;
            fwd.y = 0f;                       // pas d'inclinaison
            if (fwd.sqrMagnitude < 0.0001f)   // sécurité
                fwd = Vector3.forward;

            transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        }
    }

    void UpdateBar(int current, int max)
    {
        if (slider)
        {
            if (slider.maxValue != max) slider.maxValue = max;
            slider.value = current;
        }
    }
}
