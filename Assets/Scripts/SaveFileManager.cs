using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct House
{
    public string houseScriptableObjectName;
    public int currentPrice, currentRent;
    public bool morgaged, owned;
}

[System.Serializable]
public struct UserData
{
    public string name;
    public int balance;
}


public class SaveFileManager : MonoBehaviour
{
    [HideInInspector]
    public UserData userData;
    public Dictionary<Vector3Int, House> housingData;
    public BuildSettingsScriptableObject buildSettings;
    public HouseScriptableObject[] houseScriptableObjectsRaw;
    public Tilemap housingTilemap;

    private string _saveFileName;
    private float priceFluctuationFactor = 8;

    private const string saveFileHousingDataExtension = "_housingData";
    private const string saveFileUserDataExtension = "_userData";

    public UnityEvent onHousingDataSaveFileUpdated, onUserDataSaveFileUpdated;

    public Dictionary<string, HouseScriptableObject> houseScriptableObjects { get; private set; }
    public bool userDataInitialized { get; private set; }
    private string saveFileHousingDataPath => Path.Join(Application.persistentDataPath, _saveFileName + saveFileHousingDataExtension);
    private string saveFileUserDataPath => Path.Join(Application.persistentDataPath, _saveFileName + saveFileUserDataExtension);

    public static SaveFileManager instance { get; private set; }

    private void Awake()
    {
        // prevent more than one singleton instance
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // initialize events
        if (onHousingDataSaveFileUpdated == null)
        {
            onHousingDataSaveFileUpdated = new UnityEvent();
            onUserDataSaveFileUpdated = new UnityEvent();
        }

        houseScriptableObjects = new Dictionary<string, HouseScriptableObject>();
        housingData = new Dictionary<Vector3Int, House>();
        _saveFileName = buildSettings.saveFileName;

        foreach (var houseScriptableObject in houseScriptableObjectsRaw)
        {
            houseScriptableObject.description = houseScriptableObject.description.Replace("\\n", "\n");
            houseScriptableObjects.Add(houseScriptableObject.tile.name, houseScriptableObject);
        }

        if (TryGetTilemap())
        {
            LoadUserDataSaveFile();
            if (!LoadHousingDataSaveFile())
            {
                CreateHousingDataSaveFile();
            }
        }
    }

    private int FluctuatePrice(int price, int roundingFactor)
    {
        float priceFluctuation = price / priceFluctuationFactor;
        float newPrice = price + Random.Range(-priceFluctuation, priceFluctuation);
        return Mathf.RoundToInt(newPrice / roundingFactor) * roundingFactor;
    }

    private Vector3Int ParseVector(string rawVector)
    {
        var vectorComponents = rawVector.Split(',');
        return new Vector3Int(int.Parse(vectorComponents[0]),
                              int.Parse(vectorComponents[1]),
                              int.Parse(vectorComponents[2]));
    }

    /// <summary>
    /// generate and serialize a new save
    /// </summary>
    private void CreateHousingDataSaveFile()
    {
        if (!TryGetTilemap())
        {
            return;
        }

        string saveFile = "";
        foreach (var cellPosition in housingTilemap.cellBounds.allPositionsWithin)
        {
            var tile = housingTilemap.GetTile(cellPosition);
            if (tile == null) continue;

            var houseScriptableObject = houseScriptableObjects[tile.name];
            var house = new House()
            {
                houseScriptableObjectName = tile.name,
                currentPrice = FluctuatePrice(houseScriptableObject.price, 1000),
                currentRent = FluctuatePrice(houseScriptableObject.startingRent, 100),
                morgaged = false
            };

            housingData.Add(cellPosition, house);
            saveFile += $"{cellPosition.x},{cellPosition.y},{cellPosition.z}:\t{JsonUtility.ToJson(house)}\n";
        }

        File.WriteAllText(saveFileHousingDataPath, saveFile);
        Debug.Log($"Sucessfully created Housing Data Save File\nLocation: {saveFileHousingDataPath}");
        onHousingDataSaveFileUpdated.Invoke();
    }

    /// <summary>
    /// try to load and parse an existing save
    /// </summary>
    /// <returns></returns>
    private bool LoadHousingDataSaveFile()
    {
        if (!File.Exists(saveFileHousingDataPath))
        {
            Debug.Log($"Housing Data Save File couldn't be located\nLocation: {saveFileHousingDataPath}");
            return false;
        }

        string[] loadedHouseData = File.ReadAllLines(saveFileHousingDataPath);
        foreach (var houseSaveRaw in loadedHouseData)
        {
            var houseSave = houseSaveRaw.Split(":\t");
            var cellPosition = ParseVector(houseSave[0]);
            var house = JsonUtility.FromJson<House>(houseSave[1]);

            housingData.Add(cellPosition, house);
        }

        Debug.Log($"Successfully loaded Housing Data Save File\nLocation: {saveFileHousingDataPath}");
        onHousingDataSaveFileUpdated.Invoke();
        return true;
    }

    /// <summary>
    /// should be called after updating housingData
    /// </summary>
    /// <returns></returns>
    public bool TryUpdateExistingHousingDataSaveFile()
    {
        if (housingData.Count == 0 || !TryGetTilemap())
        {
            return false;
        }

        string saveFile = "";
        foreach (var cellPosition in housingTilemap.cellBounds.allPositionsWithin)
        {
            bool houseOnTile = housingData.TryGetValue(cellPosition, out House house);
            if (!houseOnTile) continue;

            saveFile += $"{cellPosition.x},{cellPosition.y},{cellPosition.z}:\t{JsonUtility.ToJson(house)}\n";
        }

        File.WriteAllText(saveFileHousingDataPath, saveFile);
        onHousingDataSaveFileUpdated.Invoke();
        return true;
    }

    public void CreateUserDataSaveFile(UserData userData)
    {
        this.userData = userData;
        userDataInitialized = true;

        string saveFile = JsonUtility.ToJson(userData);
        File.WriteAllText(saveFileUserDataPath, saveFile);
        Debug.Log($"Sucessfully created User Data Save File\nLocation: {saveFileHousingDataPath}");
        onUserDataSaveFileUpdated.Invoke();
    }

    private bool LoadUserDataSaveFile()
    {
        if (!File.Exists(saveFileUserDataPath))
        {
            userDataInitialized = false;

            Debug.Log($"User Data Save File couldn't be located\nLocation: {saveFileUserDataPath}");
            return false;
        }

        var userDataRaw = File.ReadAllText(saveFileUserDataPath);
        userData = JsonUtility.FromJson<UserData>(userDataRaw);
        userDataInitialized = true;

        Debug.Log($"Successfully loaded User Data Save File\nLocation: {saveFileUserDataPath}");
        onUserDataSaveFileUpdated.Invoke();
        return true;
    }

    /// <summary>
    /// should be called after updating userData
    /// </summary>
    /// <returns></returns>
    public bool TryUpdateExistingUserDataSaveFile()
    {
        if (housingData.Count == 0)
        {
            return false;
        }

        string saveFile = JsonUtility.ToJson(userData);

        File.WriteAllText(saveFileUserDataPath, saveFile);
        onUserDataSaveFileUpdated.Invoke();
        return true;
    }

    public bool TryGetTilemap()
    {
        var tilemapGameObject = GameObject.FindWithTag("Housing Tilemap");
        if (tilemapGameObject == null)
        {
            Debug.Log("Unable to locate housing tilemap");
            return false;
        }

        housingTilemap = tilemapGameObject.GetComponent<Tilemap>();
        return true;
    }

    public bool TryUpdateExistingSaveFile()
    {
        return TryUpdateExistingHousingDataSaveFile() && TryUpdateExistingUserDataSaveFile();
    }
}
