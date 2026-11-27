using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class StepWriter : MonoBehaviour {

    public float charsPerSecond = 12f;
    public float randomness = 0.4f;      // 0 = perfectly even, 1 = very uneven
    public bool playOnEnable = true;

    public string writingSound;

    TMP_Text tmp;
    Coroutine routine;

    void Awake() {
        tmp = GetComponent<TMP_Text>();
    }

    void OnEnable() {
        if (playOnEnable) Write(tmp.text);
    }

    public void Write(string text) {
        if (routine != null) StopCoroutine(routine);
        tmp.text = text;
        routine = StartCoroutine(WriteRoutine());
    }

    // Instantly show everything
    public void Skip() {
        if (routine != null) StopCoroutine(routine);
        tmp.maxVisibleCharacters = int.MaxValue;
    }

    IEnumerator WriteRoutine() {
        AudioSource writingSource = null;
        if(writingSound != null)
            writingSource = AudioManager.Instance.PlayEffectSource(writingSound, false);
        tmp.maxVisibleCharacters = 0;
        tmp.ForceMeshUpdate();              // makes sure characterCount is up to date
        int total = tmp.textInfo.characterCount;

        for (int i = 1; i <= total; i++) {
            tmp.maxVisibleCharacters = i;

            // spaces and line breaks cost no time, so the writing doesn't pause on blanks
            char c = tmp.textInfo.characterInfo[i - 1].character;
            if (char.IsWhiteSpace(c)) continue;

            float delay = 1f / charsPerSecond;
            delay *= 1f + Random.Range(-randomness, randomness);
            yield return new WaitForSeconds(delay);
        }

        if(writingSource != null)
            AudioManager.Instance.StopDestroySource(writingSource);

        routine = null;
    }
}