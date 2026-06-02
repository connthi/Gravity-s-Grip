using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BreakableProp : MonoBehaviour
{
    public float breakForceThreshold = 8f;
    public GameObject[] debrisPrefabs;
    public bool destroyOnBreak = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > breakForceThreshold)
        {
            Break();
        }
    }

    private void Break()
    {
        foreach (GameObject debris in debrisPrefabs)
        {
            if (debris == null)
                continue;

            GameObject instance = Instantiate(debris, transform.position, transform.rotation);
            Rigidbody debrisRb = instance.GetComponent<Rigidbody>();
            if (debrisRb != null)
            {
                debrisRb.AddExplosionForce(150f, transform.position, 1.5f);
            }
        }

        if (destroyOnBreak)
        {
            Destroy(gameObject);
        }
    }
}
