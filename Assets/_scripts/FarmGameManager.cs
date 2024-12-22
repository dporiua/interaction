using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
public class FarmGameManager : MonoBehaviour
{
    [Header("Game Elements")] //I LOVE HEADERS SO MUCH I WILL USE THEM IN EVERY GAME NOW
    public TMP_Text cowCountText;
    public TMP_Text chickenCountText;
    public TMP_Text moneyText;
    public TMP_Text dayCountdownText;
    public TMP_Text achievementPopupText;

    [Header("Prices")]
    public int cowPrice = 100;
    public int chickenPrice = 20;

    [Header("Happiness Meters")]
    public float cowHappiness = 200f;
    public float chickenHappiness = 200f;

    [Header("Daily Clock")]
    public float dayDurationInSeconds = 60f;
    private float currentDayTime = 0f;

    [Header("Animal Feeding Costs")]
    public int cowFoodCost = 10;
    public int chickenFoodCost = 5;

    [Header("Bulk Buying")]
    public GameObject bulkButtonsRow;
    public float buttonPressWindow = 5f;
    public int buttonPressThreshold = 20;

    [Header("Notification System")]
    public GameObject notificationPrefab;
    public Transform notificationParent;
    public float notificationDuration = 3f;

    [Header("Popup Event")]
    public GameObject popupEventObject;
    public float popupTriggerTime = 48f; 
    private bool popupTriggered = false;

    public TMP_Text[] textGroup1; 
    public TMP_Text[] textGroup2;

    private int cows;
    private int chickens;
    private int money;

    private int buttonPressCount = 0;
    private float buttonPressTimer = 0f;
    private bool bulkButtonsUnlocked = false;

    private float cowReproductionTimer = 0f;
    private float chickenReproductionTimer = 0f;

    //private float achievementPopupTimer = 0f;  NOT IN USE, DO NOT TRY TO USE
    //private bool isAchievementPopupVisible = false;

    private float totalGameTime = 0f;
    public GameObject oldObject; 
    public GameObject newObject;
    public int Cows => cows;
    public int Chickens => chickens;
    public int Money => money;
    public string googleScriptURL;


    void Start()
    {
        Debug.Log($"Initializing Game: Username={StartScreenManager.PlayerUsername}, Cows={StartScreenManager.InitialCows}, Chickens={StartScreenManager.InitialChickens}, Money={StartScreenManager.InitialMoney}");
        bulkButtonsRow.SetActive(false);
        achievementPopupText.gameObject.SetActive(false);
        popupEventObject.SetActive(false);
        currentDayTime = dayDurationInSeconds;
        InitializeGame(StartScreenManager.InitialCows, StartScreenManager.InitialChickens, StartScreenManager.InitialMoney);
        UpdateUI();
    }

    void Update()
    {
        HandleDailyClock();
        HandleReproduction();
        HandleButtonPressTimer();
        //HandleAchievementPopup();
        HandlePopupEventTrigger();
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGameOnline();
        }
    }
    public void SaveGameOnline()
    {
        StartCoroutine(SavePlayerData());
    }
    //this will only get called by the external script
    public void InitializeGame(int initialCows, int initialChickens, int initialMoney)
    {
        cows = initialCows;
        chickens = initialChickens;
        money = initialMoney;
        UpdateUI();
    }
    private IEnumerator SavePlayerData()
    {
        string username = StartScreenManager.PlayerUsername; 

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("Username is empty. Cannot save game.");
            yield break;
        }

        string url = $"{googleScriptURL}?action=save" +
                     $"&username={username.Trim()}" +
                     $"&cows={cows}" +
                     $"&chickens={chickens}" +
                     $"&money={money}";

        Debug.Log($"Saving Game Data: Username={username}, Cows={cows}, Chickens={chickens}, Money={money}");

        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Game data saved successfully!");
        }
        else
        {
            Debug.LogError("Error saving game data: " + request.error);
        }
    }

    public void SetGameState(int newCows, int newChickens, int newMoney)
    {
        cows = newCows;
        chickens = newChickens;
        money = newMoney;
    }
    private void HandleDailyClock()
    {
        if (currentDayTime > 0)
        {
            currentDayTime -= Time.deltaTime;
            totalGameTime += Time.deltaTime / dayDurationInSeconds * 24;
            UpdateDayCountdownUI();
        }
        else
        {
            EndDay();
            currentDayTime = dayDurationInSeconds;
        }
    }

    private void HandlePopupEventTrigger()
    {
        if (!popupTriggered && totalGameTime >= popupTriggerTime)
        {
            popupTriggered = true;
            popupEventObject.SetActive(true);
        }
    }

    public void OnYesButtonPressed()
    {
        if (money >= 350)
        {
            cowHappiness -= 100;
            cowFoodCost += 2;
            money -= 350;
            ShowNotification("Cow happiness increased by 100!", Color.green);
            oldObject.SetActive(false);
            newObject.SetActive(true);
            
            popupEventObject.SetActive(false);

            foreach (TMP_Text text in textGroup1)
                text.gameObject.SetActive(false);
            foreach (TMP_Text text in textGroup2)
                text.gameObject.SetActive(true);

            UpdateUI();
        }
        else
        {
            ShowNotification("Not enough money!", Color.red);
        }
    }

    public void OnNoButtonPressed()
    {
        popupEventObject.SetActive(false);
    }

    private void UpdateDayCountdownUI()
    {
        int hours = Mathf.FloorToInt((currentDayTime / dayDurationInSeconds) * 24);
        dayCountdownText.text = $"{hours:00}:00";

        if (hours > 12) dayCountdownText.color = Color.green;
        else if (hours > 6) dayCountdownText.color = Color.yellow;
        else dayCountdownText.color = Color.red;
    }

    private void EndDay()
    {
        int totalFoodCost = (cows * cowFoodCost) + (chickens * chickenFoodCost);

        if (money >= totalFoodCost)
        {
            money -= totalFoodCost;
        }
        else
        {
            int cowsLost = Mathf.Min(cows, 1);
            int chickensLost = Mathf.Min(chickens, 1);
            cows -= cowsLost;
            chickens -= chickensLost;

            if (cowsLost > 0) ShowNotification($"- {cowsLost} Cow(s)", Color.red);
            if (chickensLost > 0) ShowNotification($"- {chickensLost} Chicken(s)", Color.red);
        }

        UpdateUI();
    }

    private void HandleReproduction()
    {
        if (cows > 0)
        {
            cowReproductionTimer += Time.deltaTime;
            if (cowReproductionTimer >= cowHappiness / cows)
            {
                cows++;
                cowReproductionTimer = 0f;
                ShowNotification("+ 1 Cow", Color.green);
                UpdateUI();
            }
        }

        if (chickens > 0)
        {
            chickenReproductionTimer += Time.deltaTime;
            if (chickenReproductionTimer >= chickenHappiness / chickens)
            {
                chickens++;
                chickenReproductionTimer = 0f;
                ShowNotification("+ 1 Chicken", Color.green);
                UpdateUI();
            }
        }
    }

    private void ShowNotification(string message, Color color)
    {
        GameObject notification = Instantiate(notificationPrefab, notificationParent);
        TMP_Text text = notification.GetComponentInChildren<TMP_Text>();
        text.text = message;
        text.color = color;

        Destroy(notification, notificationDuration);
    }

    public void BuyCow() { ExecuteBuySell(1, ref cows, -cowPrice, "+ 1 Cow", Color.green); }
    public void SellCow() { ExecuteBuySell(-1, ref cows, cowPrice, "- 1 Cow", Color.red); }
    public void BuyChicken() { ExecuteBuySell(1, ref chickens, -chickenPrice, "+ 1 Chicken", Color.green); }
    public void SellChicken() { ExecuteBuySell(-1, ref chickens, chickenPrice, "- 1 Chicken", Color.red); }
    public void BuyCowsInBulk() { ExecuteBuySell(10, ref cows, -cowPrice * 10, "+ 10 Cows", Color.green); }
    public void SellCowsInBulk() { ExecuteBuySell(-10, ref cows, cowPrice * 10, "- 10 Cows", Color.red); }
    public void BuyChickensInBulk() { ExecuteBuySell(10, ref chickens, -chickenPrice * 10, "+ 10 Chickens", Color.green); }
    public void SellChickensInBulk() { ExecuteBuySell(-10, ref chickens, chickenPrice * 10, "- 10 Chickens", Color.red); }

    private void ExecuteBuySell(int amount, ref int animalCount, int cost, string notificationMessage, Color notificationColor)
    {
        if (animalCount + amount >= 0 && money + cost >= 0)
        {
            animalCount += amount;
            money += cost;
            ShowNotification(notificationMessage, notificationColor);
            UpdateUI();
            ButtonPress();
        }
    }

    private void HandleButtonPressTimer()
    {
        if (buttonPressCount > 0)
        {
            buttonPressTimer += Time.deltaTime;
            if (buttonPressTimer >= buttonPressWindow)
            {
                buttonPressCount = 0;
                buttonPressTimer = 0f;
            }
        }
    }

    private void ButtonPress()
    {
        buttonPressCount++;
        if (buttonPressCount >= buttonPressThreshold && !bulkButtonsUnlocked)
        {
            UnlockBulkButtons();
        }
    }

    private void UnlockBulkButtons()
    {
        bulkButtonsRow.SetActive(true);
        ShowNotification("Achievement Unlocked: Bulk Buying!", Color.green);
    }

    private void UpdateUI()
    {
        cowCountText.text = $"{cows}";
        chickenCountText.text = $"{chickens}";
        moneyText.text = $"${money}";
    }
}
