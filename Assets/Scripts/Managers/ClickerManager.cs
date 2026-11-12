using UnityEngine;
using UnityEngine.InputSystem;

public class ClickerManager : MonoBehaviour
{

    [Header("Input Settings")]
    private PlayerActions clickerInputs;

    private void Awake() {
        clickerInputs = new PlayerActions();
    }

    private void OnEnable() {
        clickerInputs.Enable();
    }

    private void OnDisable() {
        clickerInputs.Disable();
    }

    private void OnDestroy() {
        clickerInputs.Dispose();
    }

    bool beingPressed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (clickerInputs.Clicker.Click.IsPressed()) {
            if(beingPressed) { return; }
            beingPressed = true;
            AudioManager.Instance.PlayEffectPlayer("Click");
        } else {
            beingPressed = false;
        }
    }
}
