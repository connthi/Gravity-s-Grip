using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorSwitch : MonoBehaviour
{
    public PuzzleDoor targetDoor;
    public string requiredTag = "Torch";

    private void Reset()
    {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(requiredTag) && targetDoor != null)
        {
            targetDoor.OpenDoor();
        }
    }
}
