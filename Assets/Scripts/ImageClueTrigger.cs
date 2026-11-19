using UnityEngine;
using Vuforia;

// Va en cada ImageTarget, junto a su Clue.
[RequireComponent(typeof(ObserverBehaviour))]
public class ImageClueTrigger : MonoBehaviour {

    [SerializeField] private Clue clue;
    private ObserverBehaviour observer;

    private void Awake() {
        observer = GetComponent<ObserverBehaviour>();
        if (clue == null) { clue = GetComponent<Clue>(); }
    }

    private void OnEnable() {
        observer.OnTargetStatusChanged += OnStatusChanged;
    }

    private void OnDisable() {
        observer.OnTargetStatusChanged -= OnStatusChanged;
    }

    private void OnStatusChanged(ObserverBehaviour obs, TargetStatus status) {
        bool tracked = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;

        if (tracked && ClueSequence.Instance != null) {
            ClueSequence.Instance.Submit(clue);
        }
    }
}