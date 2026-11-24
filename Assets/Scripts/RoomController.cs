using UnityEngine;

public class RoomController : MonoBehaviour
{
    public Transform room;          // root of the room, not parented to anything
    public Transform spawnPoint;    // direct child of room, floor level, yaw-only rotation
    public float eyeHeight = 1.5f;
    public string roomLayer = "Room";

    bool spawned;
    int mask;

    void Start()
    {
        int layer = LayerMask.NameToLayer(roomLayer);
        if (layer < 0) { Debug.LogError("Create a layer named " + roomLayer); return; }
        mask = 1 << layer;
        SetLayer(room.gameObject, layer);
        room.gameObject.SetActive(false);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Hook to FirstTarget's ScanTrigger.onFound
    public void Spawn()
    {
        if (spawned) return;
        spawned = true;
        room.gameObject.SetActive(true);
        PlaceAroundCamera();
    }

    // Hook to the Recenter button
    public void Recenter()
    {
        if (spawned) PlaceAroundCamera();
    }

    void PlaceAroundCamera()
    {
        Transform cam = Camera.main.transform;

        // Where the spawn point must end up: under the camera, at floor height, same heading
        Quaternion wantRot = Quaternion.Euler(0, cam.eulerAngles.y, 0);
        Vector3 wantPos = new Vector3(cam.position.x, cam.position.y - eyeHeight, cam.position.z);

        // Move the whole room so the spawn point lands there
        Quaternion roomRot = wantRot * Quaternion.Inverse(spawnPoint.localRotation);
        Vector3 roomPos = wantPos - roomRot * spawnPoint.localPosition;
        room.SetPositionAndRotation(roomPos, roomRot);
    }

    // Scanner mode: hide the room layer so only the camera feed (and target visuals) show
    public void SetScanner(bool on)
    {
        var cam = Camera.main;
        if (on) cam.cullingMask &= ~mask;
        else    cam.cullingMask |= mask;
    }

    bool scanner;
    public void ToggleScanner() { scanner = !scanner; SetScanner(scanner); }

    static void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform) SetLayer(t.gameObject, layer);
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 40;
        GUI.Label(new Rect(20, 20, 1200, 200), Camera.main.transform.position.ToString("F2"));
    }
}