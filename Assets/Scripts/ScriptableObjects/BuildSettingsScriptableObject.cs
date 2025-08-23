using UnityEngine;

[CreateAssetMenu(fileName = "BuildSettings", menuName = "ScriptableObjects/BuildSettings", order = 2)]
public class BuildSettingsScriptableObject : ScriptableObject
{
    public string saveFileName;
    public int startingBalance;
}