using UnityEngine;

public class GlobalManager : Singleton<GlobalManager>
{
    public static string CURRENT_MAP_LEVEL = "CURRENT_MAP_LEVEL";

    private int currentMap = 0;
    public void SetKeyInt(string key, int v)
    {
        PlayerPrefs.SetInt(key, v);
    }
    public bool CheckKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }
    public int GetKeyInt(string key, int defaultValue)
    {
        if (CheckKey(key))
        {
            return PlayerPrefs.GetInt(key);
        }
        return defaultValue;
    }
    public int GetCurrentMap()
    {
        return currentMap;
    }
    public void SetCurrentMap(int index)
    {
        currentMap = index;
    }
}
