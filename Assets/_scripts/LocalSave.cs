using UnityEngine;
using System.IO;

public class LocalSave : MonoBehaviour
{
    public int cows;
    public int chickens;
    public int money;
    public FarmGameManager farmGameManager;

    private string filePath;

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
        Debug.Log($"Save File Path: {filePath}");

        if (farmGameManager)
        {
            cows = farmGameManager.Cows;
            chickens = farmGameManager.Chickens;
            money = farmGameManager.Money;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame();
        }
    }

    private void SaveGame()
    {
        if (farmGameManager)
        {
            cows = farmGameManager.Cows;
            chickens = farmGameManager.Chickens;
            money = farmGameManager.Money;
        }

        SaveData saveData = new SaveData { cows = cows, chickens = chickens, money = money };
        File.WriteAllText(filePath, JsonUtility.ToJson(saveData, true));
        Debug.Log("Game saved locally.");
    }

    private void LoadGame()
    {
        if (File.Exists(filePath))
        {
            SaveData loadedData = JsonUtility.FromJson<SaveData>(File.ReadAllText(filePath));
            cows = loadedData.cows;
            chickens = loadedData.chickens;
            money = loadedData.money;

            Debug.Log($"Game loaded locally: Cows={cows}, Chickens={chickens}, Money={money}");

                if (farmGameManager)
            {
                farmGameManager.SetGameState(cows, chickens, money);
            }
        }
        else
        {
            Debug.LogError("Save not found");
        }
    }
}

[System.Serializable]
public class SaveData
{
    public int cows;
    public int chickens;
    public int money;
}
