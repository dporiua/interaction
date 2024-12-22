using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject buttonPanel;
    public GameObject usernamePanel; 
    public TMP_InputField usernameInput;
    public TMP_Text errorText; 
    public GameObject loadingText;

    [Header("Google Sheets Integration")]
    public string googleScriptURL;

    private bool isReturningPlayer = false;

    public static string PlayerUsername { get; private set; }
    public static int InitialCows { get; private set; }
    public static int InitialChickens { get; private set; }
    public static int InitialMoney { get; private set; }

    public void ChooseNewPlayer()
    {
        isReturningPlayer = false;
        buttonPanel.SetActive(false);
        usernamePanel.SetActive(true);
        errorText.gameObject.SetActive(false);
    }

    public void ChooseReturningPlayer()
    {
        isReturningPlayer = true;
        buttonPanel.SetActive(false);
        usernamePanel.SetActive(true);
        errorText.gameObject.SetActive(false);
    }

    public void SubmitUsername()
    {
        string enteredUsername = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(enteredUsername))
        {
            ShowError("Username cannot be empty.");
            return;
        }

        PlayerUsername = enteredUsername;

        if (isReturningPlayer)
        {
            StartCoroutine(ValidateReturningPlayer());
        }
        else
        {
            InitializeNewPlayer();
        }
    }

    private void InitializeNewPlayer()
    {
        Debug.Log($"New player created: {PlayerUsername}");
        InitialCows = 0;
        InitialChickens = 0;
        InitialMoney = 500;
        LoadMainGameScene();
    }

    private IEnumerator ValidateReturningPlayer()
    {
        loadingText.SetActive(true);
        string url = $"{googleScriptURL}?action=load&username={PlayerUsername}";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();
        loadingText.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.downloadHandler.text == "NOT_FOUND")
            {
                ShowError("Username not found. Please try again.");
            }
            else
            {
                PlayerData data = JsonUtility.FromJson<PlayerData>(request.downloadHandler.text);
                InitialCows = data.cows;
                InitialChickens = data.chickens;
                InitialMoney = data.money;

                Debug.Log($"Returning player loaded: {PlayerUsername}, Cows: {InitialCows}, Chickens: {InitialChickens}, Money: {InitialMoney}");
                LoadMainGameScene();
            }
        }
        else
        {
            ShowError("Error connecting to server.");
            Debug.LogError("Error loading player data: " + request.error);
        }
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
    }

    private void LoadMainGameScene()
    {
        SceneManager.LoadScene("Main");
    }
}

[System.Serializable]
public class PlayerData
{
    public string username;
    public int cows;
    public int chickens;
    public int money;
}
