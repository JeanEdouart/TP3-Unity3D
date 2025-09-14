using UnityEngine;

public class HumanScript : MonoBehaviour
{
    [SerializeField]
    PlayerBehaviour playerBehaviour;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PerformJump()
    {
        playerBehaviour.GetComponent<Rigidbody>().AddForce(Vector3.up * playerBehaviour.jumpForce, ForceMode.Impulse);
    }
}
