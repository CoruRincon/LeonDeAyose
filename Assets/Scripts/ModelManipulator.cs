
using UnityEngine;
using UnityEngine.InputSystem;

public class ModelManipulator : MonoBehaviour {

    [Header("Input Settings")]
    private PlayerActions playerInputs;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;

    private void Awake() {
        playerInputs = new PlayerActions();
    }

    private void OnEnable() {
        playerInputs.Enable();
    }

    private void OnDisable() {
        playerInputs.Disable();
    }

    private void OnDestroy() {
        playerInputs.Dispose();
    }

    private void Update() {
        HandleMouse();
        HandleTouch();
    }

    private void HandleMouse() {
        // Rotate only while left mouse button is held
        if (playerInputs._3DViewer.Rotate.IsPressed()) {
            Vector2 delta =
                playerInputs._3DViewer.PointerMovement.ReadValue<Vector2>();

            Rotate(delta);
        }

        // Mouse scroll zoom
        float scroll = playerInputs._3DViewer.Zoom.ReadValue<Vector2>().y;

        if (scroll != 0)
            Zoom(scroll * zoomSpeed * 0.01f);
    }

    private void HandleTouch() {
        if (Touchscreen.current == null)
            return;

        var touches = Touchscreen.current.touches;

        // Single finger: Rotate
        if (touches[0].press.isPressed &&
            !touches[1].press.isPressed) {
            Vector2 delta =
                playerInputs._3DViewer.PointerMovement.ReadValue<Vector2>();

            Rotate(delta);
        }

        // Two fingers: Pinch zoom
        if (touches[0].press.isPressed &&
            touches[1].press.isPressed) {
            Vector2 pos0 = touches[0].position.ReadValue();
            Vector2 pos1 = touches[1].position.ReadValue();

            Vector2 prev0 = pos0 - touches[0].delta.ReadValue();
            Vector2 prev1 = pos1 - touches[1].delta.ReadValue();

            float previousDistance =
                Vector2.Distance(prev0, prev1);

            float currentDistance =
                Vector2.Distance(pos0, pos1);

            Zoom((currentDistance - previousDistance)
                * zoomSpeed * 0.01f);
        }
    }

    private void Rotate(Vector2 delta) {
        transform.Rotate(
            Vector3.up,
            -delta.x * rotationSpeed,
            Space.World
        );

        transform.Rotate(
            Vector3.right,
            delta.y * rotationSpeed,
            Space.World
        );
    }

    private void Zoom(float amount) {
        float scale = Mathf.Clamp(
            transform.localScale.x + amount,
            minScale,
            maxScale
        );

        transform.localScale = Vector3.one * scale;
    }
}
