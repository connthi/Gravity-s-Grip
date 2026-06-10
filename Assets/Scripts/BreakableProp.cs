using UnityEngine;

/// <summary>
/// Shatters into debris prefabs when hit hard enough.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BreakableProp : MonoBehaviour
{
    [SerializeField] private float       breakThreshold = 8f;
    [SerializeField] private GameObject[] debrisPrefabs;
    [SerializeField] private bool        destroyOnBreak = true;

    private void OnCollisionEnter(Collision col)
    {
        if (col.relativeVelocity.magnitude > breakThreshold)
            Break();
    }

    private void Break()
    {
        foreach (var prefab in debrisPrefabs)
        {
            if (prefab == null) continue;
            var instance = Instantiate(prefab, transform.position, transform.rotation);
            instance.GetComponent<Rigidbody>()?.AddExplosionForce(150f, transform.position, 1.5f);
        }

        if (destroyOnBreak) Destroy(gameObject);
    }
}