using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string gameSavePath = Application.persistentDataPath + "/save.json";
    private static string settingsSavePath = Application.persistentDataPath + "/settings.json";
    public static string GameSavePath => gameSavePath;
    public static string SettingsSavePath => settingsSavePath;

    public static void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(gameSavePath, json);
        Debug.Log("Game saved to " + gameSavePath);
    }

    public static void SaveSettings(GameSettings settings)
    {
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(settingsSavePath, json);
        Debug.Log("Game saved to " + settingsSavePath);
    }

    public static GameData LoadGame()
    {
        if (File.Exists(gameSavePath))
        {
            string json = File.ReadAllText(gameSavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("Game loaded from " + gameSavePath);
            return data;
        }

        Debug.Log("No save found, creating new GameData.");
        return new GameData();
    }

    public static GameSettings LoadSettings()
    {
        if (File.Exists(settingsSavePath))
        {
            string json = File.ReadAllText(settingsSavePath);
            GameSettings settings = JsonUtility.FromJson<GameSettings>(json);
            Debug.Log("Settings loaded from " + settingsSavePath);
            return settings;
        }

        Debug.Log("No settings found, creating new GameSettings.");
        return new GameSettings();
    }

    public static void DeleteSave()
    {
        if (File.Exists(gameSavePath))
        {
            File.Delete(gameSavePath);
            Debug.Log("SaveGame deleted.");
        }
    }
}