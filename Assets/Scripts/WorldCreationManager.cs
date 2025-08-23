using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldCreationManager : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public Button createWorldButton;
    public BuildSettingsScriptableObject buildSettings;

    private SaveFileManager saveFileManager;

    private void Start()
    {
        saveFileManager = SaveFileManager.instance;

        if (saveFileManager.userDataInitialized)
        {
            Debug.Log("Already found an existing user data save file, switching to game scene");
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        }
    }

    public void OnNameFieldEdited(string argument)
    {
        createWorldButton.interactable = nameInputField.text.Length != 0;
    }

    public void CreateWorld()
    {
        nameInputField.interactable = false;
        createWorldButton.interactable = false;

        var userData = new UserData()
        {
            balance = buildSettings.startingBalance,
            name = nameInputField.text
        };

        saveFileManager.CreateUserDataSaveFile(userData);
        SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Single);
    }
}
