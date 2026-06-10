using UnityEngine;

/// <summary>
/// Changes gravity direction for anything that enters (or is already inside)
/// this trigger volume. SetDirection() is called by LevelBuilder.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GravityPanel : MonoBehaviour
{
    [SerializeField] private Vector3 gravityDirection  = Vector3.down;
    [SerializeField] private bool    affectPlayer      = true;
    [SerializeField] private bool    affectRigidbodies = true;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => ApplyGravity(other);
    // Handles panel activating while object is already inside.
    private void OnTriggerStay(Collider other)  => ApplyGravity(other);

    private void ApplyGravity(Collider other)
    {
        var pc = affectPlayer ? other.GetComponentInParent<PlayerController>() : null;
        if (pc != null)
        {
            pc.SetGravityDirection(gravityDirection.normalized);
            // Sync every physics object in the scene to the same gravity as the player.
            if (affectRigidbodies)
                foreach (var gao in FindObjectsByType<GravityAffectedObject>())
                    gao.SetGravity(gravityDirection.normalized);
            return;
        }

        if (affectRigidbodies)
            other.GetComponent<GravityAffectedObject>()?.SetGravity(gravityDirection.normalized);
    }

    public void SetDirection(Vector3 dir) => gravityDirection = dir.normalized;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.35f);
        Gizmos.DrawCube(transform.position, transform.lossyScale);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, gravityDirection.normalized * 1.5f);
    }
#endif
}