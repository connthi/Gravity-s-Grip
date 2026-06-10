using UnityEngine;

/// <summary>
/// Slides between a closed and open transform.
/// Stops updating once fully open to avoid wasted per-frame work.
/// SetTargets() is called by LevelBuilder at construction time.
/// </summary>
public class PuzzleDoor : MonoBehaviour
{
    [SerializeField] private Transform closedPosition;
    [SerializeField] private Transform openPosition;
    [SerializeField] private float     openSpeed = 2.5f;
    [SerializeField] private bool      startOpen = false;

    private enum DoorState { Closed, Opening, Open }
    private DoorState _state;

    private void Start()
    {
        _state = startOpen ? DoorState.Open : DoorState.Closed;
        if (closedPosition != null && !startOpen)
        {
            transform.position = closedPosition.position;
            transform.rotation = closedPosition.rotation;
        }
    }

    private void Update()
    {
        if (_state != DoorState.Opening || openPosition == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position, openPosition.position, openSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, openPosition.rotation, openSpeed * 90f * Time.deltaTime);

        if (Vector3.Distance(transform.position, openPosition.position) < 0.01f)
        {
            transform.position = openPosition.position;
            transform.rotation = openPosition.rotation;
            _state = DoorState.Open;
        }
    }

    // -- Public API -----------------------------------------------------------

    public void Open()  { if (_state != DoorState.Open) _state = DoorState.Opening; }
    public void Close() { _state = DoorState.Closed; }

    /// Called by LevelBuilder after instantiation.
    public void SetTargets(Transform closed, Transform open)
    {
        closedPosition = closed;
        openPosition   = open;
    }
}