using UnityEngine;

public class ThirdPersonOrbitCamera : MonoBehaviour
{
    public Transform player;
    public float rotationSpeed = 100f;
    public float verticalMinLimit = 2f;
    public float verticalMaxLimit = 60f;

    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float zoomSpeed = 2f;

    private float currentX = 10f;
    private float currentY = 20f;

    void Start()
    {
        

        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Rotation caméra seulement si clic gauche ou droit
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, verticalMinLimit, verticalMaxLimit);

            // Si clic droit, faire tourner le joueur en douceur
            if (Input.GetMouseButton(1))
            {
                Vector3 playerEuler = player.eulerAngles;
                Quaternion targetRotation = Quaternion.Euler(playerEuler.x, currentX, playerEuler.z);
                player.rotation = Quaternion.Lerp(player.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }

        // Calcul rotation et position caméra
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 position = player.position - (rotation * Vector3.forward * distance);

        transform.rotation = rotation;
        transform.position = position;
    }
}
