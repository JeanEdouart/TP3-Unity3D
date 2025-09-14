using UnityEngine;

public class AggroZone : MonoBehaviour
{
    public MonsterBehavior owner;
    public bool debugLogs = true;

    void Start()
    {
        if (!owner) owner = GetComponentInParent<MonsterBehavior>();
        if (debugLogs) Debug.Log($"[AggroZone] owner={(owner ? owner.name : "NULL")}", this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (debugLogs) Debug.Log($"[AggroZone] ENTER by {other.name} (tag={other.tag})", other);
        if (other.CompareTag("Player"))
            owner?.SetPlayerInZone(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (debugLogs) Debug.Log($"[AggroZone] EXIT by {other.name} (tag={other.tag})", other);
        if (other.CompareTag("Player"))
            owner?.SetPlayerInZone(false);
    }

    void OnDrawGizmos()
    {
        var sc = GetComponent<SphereCollider>();
        if (!sc) return;
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(sc.center, sc.radius);
    }
}
