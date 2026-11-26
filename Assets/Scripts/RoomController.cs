using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoomController : MonoBehaviour {

    [Header("References")]
    public Transform room;          // room root, not parented to anything, kept upright
    public Transform startPoint;    // first hotspot (inside the room, at floor level)

    [Header("Settings")]
    public float eyeHeight = 1.5f;
    public bool lockHeight = true;  // ignore the phone's up/down movement
    public float moveSpeed = 4f;    // meters per second when travelling between points
    public string roomLayer = "Room";
    public float jumpThreshold = 35f;   // degrees per frame treated as a tracking jump

    static readonly RaycastHit[] hits = new RaycastHit[32];

    PlayerActions actions;
    Camera cam;
    Transform camT;
    Transform currentPoint;
    Hotspot[] spots;

    int mask;
    Vector3 pivotLocal;             // current floor point, in room-local space
    float lockedY;
    float lastYaw;

    bool ready, spawned, scanner, haveYaw;

    // ---------------------------------------------------------------- Lifecycle

    void Awake() {
        actions = new PlayerActions();
        actions.Clicker.Click.performed += OnClick;
    }

    void OnEnable() { actions.Clicker.Enable(); }

    void OnDisable() { actions.Clicker.Disable(); }

    void OnDestroy() {
        actions.Clicker.Click.performed -= OnClick;
        actions.Dispose();
    }

    void Start() {
        Init();
    }

    bool Init() {
        if (ready) return true;

        cam = Camera.main;
        if (cam == null) {
            Debug.LogError("No camera tagged MainCamera. Tag the ARCamera.");
            return false;
        }
        camT = cam.transform;

        if (room == null || startPoint == null) {
            Debug.LogError("Assign 'room' and 'startPoint' in the RoomController inspector.");
            return false;
        }

        int layer = LayerMask.NameToLayer(roomLayer);
        if (layer < 0) {
            Debug.LogError("Create a layer named " + roomLayer);
            return false;
        }

        mask = 1 << layer;
        SetLayer(room.gameObject, layer);
        room.gameObject.SetActive(false);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        ready = true;
        return true;
    }

    // ---------------------------------------------------------------- Public API

    public void Spawn() {
        if (spawned || !Init()) return;
        spawned = true;
        room.gameObject.SetActive(true);

        spots = room.GetComponentsInChildren<Hotspot>(true);
        SetCurrent(startPoint);
        pivotLocal = room.InverseTransformPoint(startPoint.position);
        lockedY = camT.position.y;

        // Make the start point face the way the camera is facing
        float pointYawInRoom = (Quaternion.Inverse(room.rotation) * startPoint.rotation).eulerAngles.y;
        room.rotation = Quaternion.Euler(0, CamYaw() - pointYawInRoom, 0);
        Follow();

        lastYaw = CamYaw();
        haveYaw = true;
    }

    public void Recenter() {
        if (!spawned || !ready) return;

        float delta = CamYaw() - currentPoint.eulerAngles.y;
        room.rotation = Quaternion.Euler(0, delta, 0) * room.rotation;
        Follow();

        lastYaw = CamYaw();
        haveYaw = true;
    }

    // Scanner mode: hide the room layer so only the camera feed shows
    public void SetScanner(bool on) {
        if (!Init()) return;

        scanner = on;
        if (on) cam.cullingMask &= ~mask;
        else cam.cullingMask |= mask;
    }

    public void ToggleScanner() {
        SetScanner(!scanner);
    }

    // ---------------------------------------------------------------- Input

    void OnClick(InputAction.CallbackContext ctx) {
        if (!spawned || !ready || scanner) return;
        if (Pointer.current == null) return;

        Vector2 pos = Pointer.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(pos);

        // Must pass the layer MASK (1 << layer), not the layer index
        int n = Physics.RaycastNonAlloc(ray, hits, 500f, mask);

        Hotspot best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < n; i++) {
            var spot = hits[i].collider.GetComponentInParent<Hotspot>();
            if (spot && spot.gameObject.activeInHierarchy && hits[i].distance < bestDist) {
                best = spot;
                bestDist = hits[i].distance;
            }
        }

        Debug.Log($"[Room] tap at {pos}: {n} hits, hotspot: {(best ? best.name : "none")}");

        if (best) SetCurrent(best.transform);
    }

    // ---------------------------------------------------------------- Movement

    void LateUpdate() {
        if (!spawned || !ready) return;

        AbsorbYawJumps();

        // Glide toward the selected hotspot
        Vector3 target = room.InverseTransformPoint(currentPoint.position);
        pivotLocal = Vector3.MoveTowards(pivotLocal, target, moveSpeed * Time.deltaTime);

        Follow();
    }

    void Follow() {
        float y = lockHeight ? lockedY : camT.position.y;
        Vector3 want = new Vector3(camT.position.x, y - eyeHeight, camT.position.z);
        room.position = want - room.rotation * pivotLocal;
    }

    void SetCurrent(Transform t) {
        currentPoint = t;
        foreach (var s in spots) s.gameObject.SetActive(s.transform != t);
    }

    // ---------------------------------------------------------------- Yaw handling

    float CamYaw() {
        Vector3 f = camT.forward;
        f.y = 0;

        if (f.sqrMagnitude < 0.05f) {   // looking almost straight up/down
            Vector3 u = camT.up;
            u.y = 0;
            f = camT.forward.y < 0 ? u : -u;
        }

        return Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
    }

    // Counteracts sudden AR tracking rotation jumps so the room doesn't spin
    void AbsorbYawJumps() {
        float yaw = CamYaw();

        if (haveYaw) {
            float d = Mathf.DeltaAngle(lastYaw, yaw);
            if (Mathf.Abs(d) > jumpThreshold)
                room.rotation = Quaternion.Euler(0, d, 0) * room.rotation;
        }

        lastYaw = yaw;
        haveYaw = true;
    }

    // ---------------------------------------------------------------- Helpers

    static void SetLayer(GameObject go, int layer) {
        go.layer = layer;
        foreach (Transform t in go.transform) {
            if (t.GetComponent<RectTransform>()) continue;  // leave UI objects alone
            SetLayer(t.gameObject, layer);
        }
    }
}