using UnityEngine;
using UnityEngine.UI;

public class PopupWindow : MonoBehaviour
{
    [HideInInspector]
    public GameObject windowObject;
    [HideInInspector]
    public SaveFileManager saveFileManager;
    private bool isWindowOpened = false;

    private void Start()
    {
        saveFileManager = SaveFileManager.instance;
    }

    public void CreateBaseWindow(GameObject preFab, string closeWindowButtonName = "Close Window Button")
    {
        windowObject = Instantiate(preFab, transform);
        var closeWindowButton = windowObject.transform.Find(closeWindowButtonName).GetComponent<Button>();
        closeWindowButton.onClick.AddListener(CloseBaseWindow);
        isWindowOpened = true;
    }

    public void CloseBaseWindow()
    {
        if (!isWindowOpened)
        {
            return;
        }

        Destroy(windowObject);
        isWindowOpened = false;
    }
}
