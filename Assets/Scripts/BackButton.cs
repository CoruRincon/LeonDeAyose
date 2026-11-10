using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetChild(0).gameObject.SetActive(!SceneManager.GetActiveScene().name.Equals("MainMenu"));
    }

    public void OnBackButton() {
        Debug.Log("Pressed");
        SceneManager.LoadScene("MainMenu");
    }
}
