using UnityEngine;
using UnityEngine.UI;

// Mirar una pista con el retículo del centro durante scanTime segundos.
// Las pistas necesitan un Collider.
public class GazeClueScanner : MonoBehaviour {

    [SerializeField] private float scanTime = 1.5f;
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private Image progressImage;   // Image tipo Filled en el centro

    private Camera cam;
    private Clue lastSeen;
    private float timer;
    private bool done;

    private void Start() {
        cam = Camera.main;
    }

    private void Update() {
        Clue seen = GetClueAtCenter();

        if (seen != lastSeen) {
            lastSeen = seen;
            timer = 0f;
            done = false;
        }

        if (seen == null || done) {
            SetProgress(0f);
            return;
        }

        timer += Time.deltaTime;
        SetProgress(timer / scanTime);

        if (timer >= scanTime) {
            done = true;
            ClueSequence.Instance.Submit(seen);
        }
    }

    private Clue GetClueAtCenter() {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance)) {
            return hit.collider.GetComponentInParent<Clue>();
        }
        return null;
    }

    private void SetProgress(float value) {
        if (progressImage != null) { progressImage.fillAmount = Mathf.Clamp01(value); }
    }
}