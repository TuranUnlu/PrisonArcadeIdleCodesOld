using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    public static void SaveData(GameData gameData)
    {
        string dataString = JsonUtility.ToJson(gameData);
        PlayerPrefs.SetString("data", dataString);
    }

    public static void SaveData(DataBase dataBase)
    {
        string dataString = JsonUtility.ToJson(dataBase);
        PlayerPrefs.SetString("data2", dataString);
    }
    public static void LoadData(GameData gameData)
    {
        if (!PlayerPrefs.HasKey("data"))
        {
            SaveData(gameData);
            return;
        }

        string dataString = PlayerPrefs.GetString("data");
        JsonUtility.FromJsonOverwrite(dataString, gameData);
    }

    public static void LoadData(DataBase dataBase)
    {
        if (!PlayerPrefs.HasKey("data2"))
        {
            SaveData(dataBase);
            return;
        }

        string dataString = PlayerPrefs.GetString("data2");
        JsonUtility.FromJsonOverwrite(dataString, dataBase);
    }
}
