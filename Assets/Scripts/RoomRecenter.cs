using UnityEngine;

public class RoomRecenter : MonoBehaviour
{
    public Transform room;        // root of the room (not a child of the target)
    public Transform spawnPoint;  // child of room: where the player stands, facing forward

    public void Recenter()
    {
        Transform cam = Camera.main.transform;

        // 1. Match heading: rotate the room around the spawn point (yaw only)
        float yawDelta = cam.eulerAngles.y - spawnPoint.eulerAngles.y;
        room.RotateAround(spawnPoint.position, Vector3.up, yawDelta);

        // 2. Match position on the horizontal plane (keep the floor height)
        Vector3 delta = cam.position - spawnPoint.position;
        delta.y = 0;
        room.position += delta;
    }
}