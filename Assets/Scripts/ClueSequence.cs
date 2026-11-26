using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Decide si una pista toca, la muestra y avanza. No sabe cómo se escaneó.
public class ClueSequence : MonoBehaviour {

    public static ClueSequence Instance { get; private set; }

    [Header("Textos (tu 'Information' ahora, la pizarra después)")]
    [SerializeField] private TMP_Text titleLabel;    // Opcional
    [SerializeField] private TMP_Text bodyLabel;
    [SerializeField] private TMP_Text messageLabel;  // Avisos (opcional)

    [Header("Mensajes")]
    [SerializeField, TextArea] private string startMessage = "Escanea la primera pista.";
    [SerializeField, TextArea] private string wrongOrderMessage = "Todavía no. Sigue el orden.";
    [SerializeField, TextArea] private string completedMessage = "¡Has completado todas las pistas!";

    [Header("Eventos")]
    [SerializeField] private UnityEvent onClueUnlocked;
    [SerializeField] private UnityEvent onAllCompleted;

    private readonly List<Clue> clues = new List<Clue>();
    private int currentStep = 0;
    private Clue shown;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SetMessage(startMessage);
    }

    public void Register(Clue clue) {
        if (clues.Contains(clue)) { return; }
        clues.Add(clue);
        clues.Sort((a, b) => a.order.CompareTo(b.order));
    }

    public void Unregister(Clue clue) {
        clues.Remove(clue);
    }

    // Lo llaman los disparadores (imagen, mirada...)
    public void Submit(Clue clue) {
        int index = clues.IndexOf(clue);
        if (index < 0) { return; }

        // Pista futura
        if (index > currentStep) {
            SetMessage(wrongOrderMessage);
            return;
        }

        bool isNew = index == currentStep;

        // Pista ya mostrada y sin cambios: no repetir (evita reiniciar el vídeo)
        if (!isNew && clue == shown) { return; }

        Show(clue);

        if (!isNew) { return; }

        currentStep++;
        onClueUnlocked?.Invoke();

        if (currentStep >= clues.Count) {
            SetMessage(completedMessage);
            onAllCompleted?.Invoke();
        } else if (!string.IsNullOrEmpty(clue.hintForNext)) {
            SetMessage(clue.hintForNext);
        } else {
            SetMessage("Pista " + currentStep + " de " + clues.Count + " desbloqueada.");
        }
    }

    private void Show(Clue clue) {
        if (shown != null && shown != clue) {
            shown.SetContents(false);
        }

        shown = clue;
        clue.SetContents(true);

        if (titleLabel != null) { titleLabel.text = clue.title; }

        if (clue.bodyLabel != null) { clue.bodyLabel.text = clue.body; }
        else if (bodyLabel != null) { bodyLabel.text = clue.body; }

        clue.onShown?.Invoke();
    }

    // Para tu botón "Cerrar"
    public void HideCurrent() {
        if (shown != null) {
            shown.SetContents(false);
            shown = null;
        }
    }

    public void ResetProgress() {
        HideCurrent();
        currentStep = 0;
        SetMessage(startMessage);
    }

    private void SetMessage(string text) {
        if (messageLabel != null) { messageLabel.text = text; }
    }
}