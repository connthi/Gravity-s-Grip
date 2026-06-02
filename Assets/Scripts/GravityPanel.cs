using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GravityPanel : MonoBehaviour
{
    public Vector3 gravityDirection = Vector3.down;
    public bool affectPlayer = true;
    public bool affectPhysicsObjects = true;

    private void Reset()
    {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (affectPlayer)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetGravity(gravityDirection.normalized);
            }
        }

        if (affectPhysicsObjects)
        {
            GravityAffectedObject gravityObject = other.GetComponent<GravityAffectedObject>();
            if (gravityObject != null)
            {
                gravityObject.SetGravity(gravityDirection.normalized);
            }
        }
    }
}
