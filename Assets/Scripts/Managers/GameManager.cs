using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public enum GameStates {
        LOADING,
        MENU,
        PLAYING,
        CREDITS
    }

    [SerializeField]
    private GameStates currentGameState;

    public GameStates GetCurrentState() {
        return currentGameState;
    }

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject.transform.parent);
        }

        currentGameState = GameStates.LOADING;
    }

    private void Start() {
        StartCoroutine(LoadScene("MainMenu"));
    }

    public IEnumerator LoadScene(string scene, bool isCinematic = false) {
        Debug.Log("Loading: " + scene);
        // Start loading the scene
        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        // Wait until the level finish loading
        while (!asyncLoadLevel.isDone)
            yield return null;

        // Wait a frame so every Awake and Start method is called before we decide
        // the final game state for this scene.
        yield return new WaitForEndOfFrame();

        if (scene == "MainMenu") {
            currentGameState = GameStates.MENU;
        }

        if (scene == "Credits") {
            currentGameState = GameStates.CREDITS;
        }

        if (scene == "Game" || scene == "Game Coru" || scene == "Game Gabri") {
            currentGameState = GameStates.PLAYING;
        }

    }
}
