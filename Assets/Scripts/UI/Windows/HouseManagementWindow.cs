using UnityEngine;
using UnityEngine.UI;

public class HouseManagementWindow : PopupWindow
{
    public GameObject houseManagementWindowPrefab, houseInfoScrollviewPanelPrefab;
    public HouseSettingsWindow houseSettingsWindow;
    public Button button;

    public void OpenWindow()
    {
        CreateBaseWindow(houseManagementWindowPrefab);

        var scrollViewContent = windowObject.transform.Find("Viewport").Find("Content");
        foreach (var house in saveFileManager.housingData)
        {
            if (house.Value.owned)
            {
                AddScrollViewPanel(house.Key, house.Value, scrollViewContent);
            }
        }
    }

    private void AddScrollViewPanel(Vector3Int cellPosition, House house, Transform context)
    {
        var panel = Instantiate(houseInfoScrollviewPanelPrefab, context);
        var houseScriptableObject = saveFileManager.houseScriptableObjects[house.houseScriptableObjectName];
        
        panel.SetChildTextView("House Name", houseScriptableObject.name);
        panel.SetChildTextView("House Value", house.currentPrice.ToDollars());
        panel.SetChildTextView("House Rent", house.currentRent.ToDollars());

        var button = panel.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            CloseBaseWindow();
            houseSettingsWindow.OpenWindow(cellPosition);
        });
    }
}
