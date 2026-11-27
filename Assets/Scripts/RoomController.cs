using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoomController : MonoBehaviour {

    [Header("References")]
    public Transform room;          // room root, not parented to anything, kept upright
    public Transform startPoint;    // first hotspot (inside the room, at floor level)

    [SerializeField] GameObject ARButton;
    [SerializeField] GameObject ScannerButton;

    [Header("Settings")]
    public float eyeHeight = 1.5f;
    public bool lockHeight = true;      // ignore the phone's up/down movement
    public float moveSpeed = 4f;        // meters per second when travelling between points
    public string roomLayer = "Room";
    public float jumpThreshold = 35f;   // degrees per frame treated as a tracking jump
    public bool exitScannerOnSpawn = true;

    static readonly RaycastHit[] hits = new RaycastHit[32];

    PlayerActions actions;
    Camera cam;
    Transform camT;
    Transform currentPoint;
    Hotspot[] spots;

    int mask;
    Vector3 pivotLocal;     // current standing point, in room-local space (y = floor)
    float floorLocalY;      // room-local height of the floor
    float lockedY;          // world height of the camera when the room spawned
    float lastYaw;

    bool ready, spawned, scanner, haveYaw;

    // ---------------------------------------------------------------- Lifecycle

    void Awake() {
        actions = new PlayerActions();
        actions.Clicker.Click.performed += OnClick;
        SetScanner(true);
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

        // The floor height comes from the start point, and is used for every hotspot
        floorLocalY = room.InverseTransformPoint(startPoint.position).y;

        SetCurrent(startPoint);
        pivotLocal = PointLocal(currentPoint);
        lockedY = camT.position.y;

        // Make the start point face the way the camera is facing
        float pointYawInRoom = (Quaternion.Inverse(room.rotation) * startPoint.rotation).eulerAngles.y;
        room.rotation = Quaternion.Euler(0, CamYaw() - pointYawInRoom, 0);
        Follow();

        lastYaw = CamYaw();
        haveYaw = true;

        if (exitScannerOnSpawn) SetScanner(false);
    }

    public void RecenterStart() {
        if (!spawned || !ready) return;

        float delta = CamYaw() - startPoint.eulerAngles.y;
        room.rotation = Quaternion.Euler(0, delta, 0) * room.rotation;
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

        UpdateScannerButtons();
    }

    public void ToggleScanner() {
        SetScanner(!scanner);
    }

    public void UpdateScannerButtons() {
        if (ARButton) ARButton.SetActive(scanner);
        if (ScannerButton) ScannerButton.SetActive(!scanner);
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

        // Glide toward the selected hotspot (horizontal only, height stays at eyeHeight)
        Vector3 target = PointLocal(currentPoint);
        float scale = Mathf.Max(0.0001f, room.lossyScale.x);
        float step = moveSpeed * Time.deltaTime / scale;   // moveSpeed is in world meters
        pivotLocal = Vector3.MoveTowards(pivotLocal, target, step);

        Follow();
    }

    // Keeps the room's current standing point exactly under the camera, at eye height.
    // Rotation is never touched here.
    void Follow() {
        float y = lockHeight ? lockedY : camT.position.y;
        Vector3 want = new Vector3(camT.position.x, y - eyeHeight, camT.position.z);
        room.position = want - room.TransformVector(pivotLocal);   // handles rotation AND scale
    }

    // A hotspot's position in room-local space, at floor height
    Vector3 PointLocal(Transform t) {
        Vector3 p = room.InverseTransformPoint(t.position);
        p.y = floorLocalY;
        return p;
    }

    void SetCurrent(Transform t) {
        var h = t.GetComponentInParent<Hotspot>();
        if (h == null) {
            Debug.LogWarning("[Room] '" + t.name + "' has no Hotspot component, so it can't be hidden.");
            currentPoint = t;
            foreach (var s in spots) s.gameObject.SetActive(true);
            return;
        }

        currentPoint = h.transform;
        foreach (var s in spots) s.gameObject.SetActive(s != h);
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
            // leave screen-space UI alone, but include world-space canvases
            var canvas = t.GetComponent<Canvas>();
            if (canvas && canvas.renderMode != RenderMode.WorldSpace) continue;
            SetLayer(t.gameObject, layer);
        }
    }
}