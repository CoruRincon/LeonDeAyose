using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public Transform room;          // currently a child of FirstTarget
    public RoomRecenter recenter;   // the script from before (with spawnPoint set)
    bool spawned;

    // Call this from the target found event
    public void OnTargetFound()
    {
        if (spawned) return;
        spawned = true;

        room.SetParent(null, true);   // keep world pose, free from the target

        // Remove any pitch/roll inherited from the target
        room.rotation = Quaternion.Euler(0, room.eulerAngles.y, 0);

        room.gameObject.SetActive(true);
        recenter.Recenter();          // put the player at the spawn point
    }
}