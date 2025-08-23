using TMPro;
using UnityEngine;

public class TopPanel : MonoBehaviour
{
    public TextMeshProUGUI balanceText, usernameText;

    private SaveFileManager saveFileManager;

    private void Start()
    {
        saveFileManager = SaveFileManager.instance;
        saveFileManager.onUserDataSaveFileUpdated.AddListener(UpdatePanel);
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        var userData = saveFileManager.userData;
        balanceText.text = userData.balance.ToDollars();
        usernameText.text = userData.name;
    }
}
