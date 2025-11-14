using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class SaveGameData
{
    public int score;
    public string timestamp;
    public string sessionId;
    public string playerUsername;
}

public class LocalSaveManager : MonoBehaviour
{
    private static LocalSaveManager instance;
    public static LocalSaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LocalSaveManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("LocalSaveManager");
                    instance = go.AddComponent<LocalSaveManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "gamesave.json");
    }
    
    public void SaveToJSON(int score)
    {
        try
        {
            string playerUsername = PlayerPrefs.GetString("PlayerUsername", "UnknownPlayer");
            string sessionId = PlayerPrefs.GetString("SessionId", System.Guid.NewGuid().ToString());
            
            if (!PlayerPrefs.HasKey("SessionId"))
            {
                PlayerPrefs.SetString("SessionId", sessionId);
            }
            
            SaveGameData saveData = new SaveGameData
            {
                score = score,
                timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                sessionId = sessionId,
                playerUsername = playerUsername
            };
            
            string json = JsonUtility.ToJson(saveData, true);
            string savePath = GetSavePath();
            
            File.WriteAllText(savePath, json);
            Debug.Log($"Game saved to JSON: {savePath}");
            Debug.Log($"Save data: Score={score}, Player={playerUsername}, Time={saveData.timestamp}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game to JSON: {e.Message}");
        }
    }
    
    public SaveGameData LoadFromJSON()
    {
        try
        {
            string savePath = GetSavePath();
            
            if (!File.Exists(savePath))
            {
                Debug.Log("No save file found at: " + savePath);
                return null;
            }
            
            string json = File.ReadAllText(savePath);
            SaveGameData saveData = JsonUtility.FromJson<SaveGameData>(json);
            
            Debug.Log($"Game loaded from JSON: Score={saveData.score}, Player={saveData.playerUsername}, Time={saveData.timestamp}");
            return saveData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game from JSON: {e.Message}");
            return null;
        }
    }
    
    public bool HasSaveFile()
    {
        return File.Exists(GetSavePath());
    }
    
    public void DeleteSaveFile()
    {
        try
        {
            string savePath = GetSavePath();
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Save file deleted: " + savePath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
        }
    }
}
