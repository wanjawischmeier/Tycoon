using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HouseSettingsWindow : PopupWindow
{
    public GameObject houseSettingsWindowPrefab;
    public HousingBubbles housingBubbles;

    private GameObject purchaseButton;
    private TextMeshProUGUI houseRentText;
    private Vector3Int selectedCellPosition;
    private int sliderRent;

    public bool OpenWindow(Vector3Int cellPosition)
    {
        if (!saveFileManager.TryGetTilemap())
        {
            return false;
        }

        if (!saveFileManager.housingData.ContainsKey(cellPosition))
        {
            Debug.Log($"Failed to show house card\nData for cell position {cellPosition} not found");
            return false;
        }

        var house = saveFileManager.housingData[cellPosition];
        var houseScriptableObject = saveFileManager.houseScriptableObjects[house.houseScriptableObjectName];

        CreateBaseWindow(houseSettingsWindowPrefab);

        var windowTitle = windowObject.transform.Find("Window Title");
        windowTitle.GetComponent<TextMeshProUGUI>().text = houseScriptableObject.name;

        var windowContent = windowObject.transform.FindPath("Scroll View/Viewport/Content");

        string infoText = houseScriptableObject.description;
        infoText += $"\n\n--------\n\n{JsonUtility.ToJson(house)}\n\n{JsonUtility.ToJson(houseScriptableObject)}";
        windowContent.Find("House Description").GetComponent<TextMeshProUGUI>().text = infoText;

        string houseClass = houseScriptableObject.housingClass.ToString();
        windowContent.Find("House Class").GetComponent<TextMeshProUGUI>().text = houseClass;

        string houseValue = house.currentPrice.ToDollars();
        windowContent.Find("House Value").GetComponent<TextMeshProUGUI>().text = houseValue;

        string houseRent = house.currentRent.ToDollars();
        var houseRentTransform = windowContent.Find("House Rent");
        houseRentText = houseRentTransform.Find("Text").GetComponent<TextMeshProUGUI>();
        houseRentText.text = houseRent;

        var sliderTransform = houseRentTransform.Find("Rent Slider");
        var sliderComponent = sliderTransform.GetComponent<Slider>();
        sliderComponent.maxValue = houseScriptableObject.startingRent / 50;
        sliderComponent.value = house.currentRent / 100;
        if (house.owned)
        {
            sliderComponent.interactable = true;
            sliderComponent.onValueChanged.AddListener(OnRentSliderChanged);

            var sliderEventTrigger = sliderTransform.GetComponent<EventTrigger>();
            sliderEventTrigger.triggers[0].callback.AddListener(ProcessRentChange);
        }


        purchaseButton = windowObject.transform.Find("Purchase Button").gameObject;
        var purchaseButtonComponent = purchaseButton.GetComponent<Button>();
        if (house.owned)
        {
            purchaseButton.SetChildTextView("Text (TMP)", "Sell");
            purchaseButtonComponent.onClick.AddListener(ProcessSellTransaction);
        }
        else
        {
            bool affordable = saveFileManager.userData.balance >= house.currentPrice;
            purchaseButtonComponent.onClick.AddListener(ProcessPurchaseTransaction);
            purchaseButtonComponent.interactable = affordable;
        }

        selectedCellPosition = cellPosition;
        return true;
    }

    public void ProcessPurchaseTransaction()
    {
        if (!saveFileManager.housingData.ContainsKey(selectedCellPosition))
        {
            Debug.Log($"Failed to locate house at {selectedCellPosition} in save file");
            return;
        }

        var house = saveFileManager.housingData[selectedCellPosition];
        if (house.owned) return;

        house.owned = true;
        saveFileManager.housingData[selectedCellPosition] = house;
        saveFileManager.userData.balance -= house.currentPrice;
        saveFileManager.TryUpdateExistingSaveFile();

        housingBubbles.UpdateHouseBubbleColor(selectedCellPosition, new Color(0, 0, 1, 0.75f));

        CloseBaseWindow();
    }

    public void ProcessSellTransaction()
    {
        if (!saveFileManager.housingData.ContainsKey(selectedCellPosition))
        {
            Debug.Log($"Failed to locate house at {selectedCellPosition} in save file");
            return;
        }

        var house = saveFileManager.housingData[selectedCellPosition];
        if (!house.owned) return;

        house.owned = false;
        saveFileManager.housingData[selectedCellPosition] = house;
        saveFileManager.userData.balance += house.currentPrice;
        saveFileManager.TryUpdateExistingSaveFile();

        housingBubbles.UpdateHouseBubbleColor(selectedCellPosition, new Color(1, 1, 1, 0.75f));

        CloseBaseWindow();
    }

    private void ProcessRentChange(BaseEventData call)
    {
        if (!saveFileManager.housingData.ContainsKey(selectedCellPosition))
        {
            Debug.Log($"Failed to locate house at {selectedCellPosition} in save file");
            return;
        }

        var house = saveFileManager.housingData[selectedCellPosition];
        if (!house.owned) return;

        house.currentRent = sliderRent;
        saveFileManager.housingData[selectedCellPosition] = house;
        saveFileManager.TryUpdateExistingHousingDataSaveFile();
    }

    private void OnRentSliderChanged(float value)
    {
        sliderRent = Mathf.RoundToInt(value * 100);
        houseRentText.text = sliderRent.ToDollars();
    }
}
