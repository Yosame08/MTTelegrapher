using UnityEngine;

public class SceneChanging : MonoBehaviour {
    public string sceneName;
    public void GoToScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
