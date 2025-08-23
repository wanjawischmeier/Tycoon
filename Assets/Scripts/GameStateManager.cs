using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    private void Start()
    {
        if (!SaveFileManager.instance.userDataInitialized)
        {
            Debug.Log("User data save file not found. Switching to account creation scene");
            SceneManager.LoadScene("Setup", LoadSceneMode.Single);
        }
    }
}
