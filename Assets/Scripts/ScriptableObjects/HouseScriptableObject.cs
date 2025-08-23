using UnityEngine;
using UnityEngine.Tilemaps;

public enum HousingClass
{
    WornDown, Basic, MiddleClass, Luxurious
}

[CreateAssetMenu(fileName = "House_", menuName = "ScriptableObjects/House", order = 0)]
public class HouseScriptableObject : ScriptableObject
{
    public new string name;
    public string description;
    public HousingClass housingClass;
    public TileBase tile;

    public int price, morgage, startingRent, maximumRentUpcharge;
}
