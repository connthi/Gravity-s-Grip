using UnityEngine;

public class PuzzleDoor : MonoBehaviour
{
    public Transform closedPosition;
    public Transform openPosition;
    public float openSpeed = 2f;

    private bool isOpen;

    void Update()
    {
        if (closedPosition == null || openPosition == null)
            return;

        Transform target = isOpen ? openPosition : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, target.position, openSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, openSpeed * 100f * Time.deltaTime);
    }

    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }
}
