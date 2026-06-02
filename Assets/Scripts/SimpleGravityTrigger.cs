using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleGravityTrigger : MonoBehaviour
{
    public Vector3 gravityDirection = Vector3.down;
    public bool overridePlayerGravity = true;

    private void Reset()
    {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && overridePlayerGravity)
        {
            player.SetGravity(gravityDirection.normalized);
        }
    }
}
