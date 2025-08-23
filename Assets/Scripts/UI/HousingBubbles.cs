using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class HousingBubbles : MonoBehaviour
{
    public GameObject houseInfoBubblePrefab;
    public float infoBubbleShift = 0.75f;

    private SaveFileManager saveFileManager;
    private Dictionary<Vector3Int, GameObject> houseBubbles;

    private void Start()
    {
        houseBubbles = new Dictionary<Vector3Int, GameObject>();
        saveFileManager = SaveFileManager.instance;

        foreach (var house in saveFileManager.housingData)
        {
            ShowHouseBubble(house.Key, house.Value);
        }
    }

    public bool ShowHouseBubble(Vector3Int cellPosition, House house)
    {
        if (houseBubbles.ContainsKey(cellPosition))
        {
            return false;
        }

        var worldCellPosition = saveFileManager.housingTilemap.CellToWorld(cellPosition);
        var bubble = Instantiate(houseInfoBubblePrefab,
                                 worldCellPosition + Vector3.up * infoBubbleShift,
                                 Quaternion.identity,
                                 transform);

        houseBubbles.Add(cellPosition, bubble);

        var color = house.owned ? Color.blue : Color.white;
        color.a = 0.75f;
        bubble.GetComponent<Image>().color = color;

        var priceTextField = bubble.GetComponentInChildren<TextMeshProUGUI>();
        priceTextField.text = house.currentPrice.ToDollars();

        return true;
    }

    public bool UpdateHouseBubbleColor(Vector3Int cellPosition, Color color)
    {
        if (!houseBubbles.ContainsKey(cellPosition))
        {
            return false;
        }

        houseBubbles[cellPosition].GetComponent<Image>().color = color;
        return true;
    }


    public void DestroyHouseBubble(Vector3Int cellPosition)
    {
        if (!houseBubbles.ContainsKey(cellPosition))
        {
            return;
        }

        Destroy(houseBubbles[cellPosition]);
        houseBubbles.Remove(cellPosition);
    }

    /*
    public bool ShowHouseBubble(Vector3Int cellPosition)
    {
        if (!saveFileManager.housingData.ContainsKey(selectedCellPosition))
        {
            return false;
        }

        var house = saveFileManager.housingData[selectedCellPosition];
        ShowHouseBubble(cellPosition, house);

        return true;
    }
    */
}
