using UnityEngine;
using UnityEngine.Events;

// Datos y contenido de una pista. Va en el mismo objeto que su disparador.
public class Clue : MonoBehaviour {

    [Header("Orden (0 = la primera)")]
    public int order;

    [Header("Contenido")]
    public string title;
    [TextArea(3, 10)] public string body;
    public GameObject[] contents;       // Todo lo que debe encenderse (incluye los padres apagados)
    public UnityEvent onShown;          // Extras: VideoPlayer.Play, sonidos...
    [TextArea] public string hintForNext;

    private void Start() {
        SetContents(false);

        if (ClueSequence.Instance != null) {
            ClueSequence.Instance.Register(this);
        }
    }

    private void OnDestroy() {
        if (ClueSequence.Instance != null) {
            ClueSequence.Instance.Unregister(this);
        }
    }

    public void SetContents(bool active) {
        if (contents == null) { return; }

        foreach (GameObject go in contents) {
            if (go != null) { go.SetActive(active); }
        }
    }
}