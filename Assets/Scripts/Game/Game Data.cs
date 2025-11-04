using UnityEngine.Playables;
using UnityEngine;
using System.IO;

[System.Serializable]
public class GameData
{
    public string nickname;
    
    public int sunsetTime;
    public int sunriseTime;

    public int bestScore = 0;
}


public class GameDataController : Singleton<GameDataController>
{
    public GameData gameData { get; private set; }

    public void LoadGameData()
    {
        if (File.Exists(Application.persistentDataPath + "/gameData.json"))
        {
            string json = File.ReadAllText(Application.persistentDataPath + "/gameData.json");
            gameData = JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            gameData = new GameData();
        }
        Debug.Log(gameData);
    }

    public void SaveGameData()
    {
        string json = JsonUtility.ToJson(gameData);
        File.WriteAllText(Application.persistentDataPath + "/gameData.json", json);
    }
}