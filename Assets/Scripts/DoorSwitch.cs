using UnityEngine;

/// <summary>
/// Opens a PuzzleDoor when a lit torch enters this trigger zone.
/// Decoupled: checks TorchPickup on the collider OR on the PlayerController.
/// SetDoor() and SetObjective() are called by LevelBuilder at construction time.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DoorSwitch : MonoBehaviour
{
    [SerializeField] private PuzzleDoor      targetDoor;
    [SerializeField] private PuzzleObjective objective;
    [SerializeField] private bool            requireLitTorch = true;

    private bool _triggered;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered || targetDoor == null) return;

        TorchPickup torch = FindTorch(other);
        if (torch == null) return;
        if (requireLitTorch && !torch.IsLit) return;

        _triggered = true;
        targetDoor.Open();
        objective?.Complete();
    }

    // -- Shims for LevelBuilder -----------------------------------------------

    public void SetDoor(PuzzleDoor door)           => targetDoor      = door;
    public void SetObjective(PuzzleObjective obj)  => objective       = obj;
    public void SetRequireLitTorch(bool val)       => requireLitTorch = val;

    // -- Private ---------------------------------------------------------------

    private static TorchPickup FindTorch(Collider other)
    {
        TorchPickup direct = other.GetComponentInParent<TorchPickup>();
        if (direct != null) return direct;

        PlayerController player = other.GetComponent<PlayerController>();
        return player?.CarriedTorch;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _triggered
            ? new Color(0f, 1f, 0f, 0.3f)
            : new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }
#endif
}