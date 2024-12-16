using UnityEngine;
using TMPro;

public class FarmGameManager : MonoBehaviour
{
    [Header("Game Elements")]
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
    public float buttonPressWindow = 5f; // Time to reach threshold
    public int buttonPressThreshold = 20;

    [Header("Notification System")]
    public GameObject notificationPrefab; // Prefab for notifications
    public Transform notificationParent; // Parent for notifications
    public float notificationDuration = 3f; // How long notifications stay on screen

    private int cows = 0;
    private int chickens = 0;
    private int money = 500;

    private int buttonPressCount = 0;
    private float buttonPressTimer = 0f;
    private bool bulkButtonsUnlocked = false;

    private float cowReproductionTimer = 0f;
    private float chickenReproductionTimer = 0f;

    private float achievementPopupTimer = 0f;
    private bool isAchievementPopupVisible = false;

    void Start()
    {
        UpdateUI();
        bulkButtonsRow.SetActive(false); // Hide the bulk buttons initially
        achievementPopupText.gameObject.SetActive(false); // Hide achievement popup
        currentDayTime = dayDurationInSeconds; // Start the clock
    }

    void Update()
    {
        HandleDailyClock();
        HandleReproduction();
        HandleButtonPressTimer();
        HandleAchievementPopup();
    }

    private void HandleDailyClock()
    {
        if (currentDayTime > 0)
        {
            currentDayTime -= Time.deltaTime;
            UpdateDayCountdownUI();
        }
        else
        {
            EndDay();
            currentDayTime = dayDurationInSeconds; // Reset the clock for a new day
        }
    }

    private void UpdateDayCountdownUI()
    {
        int hours = Mathf.FloorToInt((currentDayTime / dayDurationInSeconds) * 24);
        dayCountdownText.text = $"{hours:00}:00";
        if (hours > 12)
        {
            dayCountdownText.color = Color.green; // More than 12 hours left
        }
        else if (hours > 6)
        {
            dayCountdownText.color = Color.yellow; // 6 to 12 hours left
        }
        else
        {
            dayCountdownText.color = Color.red; // Less than 6 hours left
        }
    }

    private void EndDay()
    {
        int totalFoodCost = (cows * cowFoodCost) + (chickens * chickenFoodCost);

        if (money >= totalFoodCost)
        {
            money -= totalFoodCost; // Deduct food cost
        }
        else
        {
            int remainingMoney = money;

            int initialChickens = chickens;
            int initialCows = cows;

            int chickensThatCanBeFed = remainingMoney / chickenFoodCost;
            remainingMoney -= chickensThatCanBeFed * chickenFoodCost;
            chickens = Mathf.Max(0, chickens - (chickens - chickensThatCanBeFed));

            int cowsThatCanBeFed = remainingMoney / cowFoodCost;
            remainingMoney -= cowsThatCanBeFed * cowFoodCost;
            cows = Mathf.Max(0, cows - (cows - cowsThatCanBeFed));

            money = 0;

            // Trigger death notifications
            if (initialChickens > chickens)
                ShowNotification($"- {initialChickens - chickens} Chickens", Color.red);

            if (initialCows > cows)
                ShowNotification($"- {initialCows - cows} Cows", Color.red);
        }

        UpdateUI();
    }

    private void HandleReproduction()
    {
        if (cows > 0)
        {
            cowReproductionTimer += Time.deltaTime;
            float cowReproductionTime = cowHappiness / cows;

            if (cowReproductionTimer >= cowReproductionTime)
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
            float chickenReproductionTime = chickenHappiness / chickens;

            if (chickenReproductionTimer >= chickenReproductionTime)
            {
                chickens++;
                chickenReproductionTimer = 0f;
                ShowNotification("+ 1 Chicken", Color.green);
                UpdateUI();
            }
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

    private void HandleAchievementPopup()
    {
        if (isAchievementPopupVisible)
        {
            achievementPopupTimer += Time.deltaTime;
            if (achievementPopupTimer >= 3f) // Display popup for 3 seconds
            {
                achievementPopupText.gameObject.SetActive(false);
                isAchievementPopupVisible = false;
            }
        }
    }

    public void ButtonPress()
    {
        buttonPressCount++;
        buttonPressTimer = 0f; // Reset the timer

        if (buttonPressCount >= buttonPressThreshold && !bulkButtonsUnlocked)
        {
            UnlockBulkButtons();
        }
    }

    private void UnlockBulkButtons()
    {
        bulkButtonsRow.SetActive(true);
        bulkButtonsUnlocked = true;

        // Show mini achievement popup
        achievementPopupText.text = "Achievement Unlocked: Bulk Buying!";
        achievementPopupText.gameObject.SetActive(true);
        isAchievementPopupVisible = true;
        achievementPopupTimer = 0f;
    }

    private void ShowNotification(string message, Color color)
    {
        GameObject notification = Instantiate(notificationPrefab, notificationParent);
        TMP_Text text = notification.GetComponentInChildren<TMP_Text>();
        text.text = message;
        text.color = color;

        // Start fading and scrolling animation
        Destroy(notification, notificationDuration);
    }

    public void BuyCow() { ExecuteBuySell(1, ref cows, -cowPrice, "+ 1 Cow", Color.green); }
    public void SellCow() { ExecuteBuySell(-1, ref cows, cowPrice, "- 1 Cow", Color.red); }
    public void BuyChicken() { ExecuteBuySell(1, ref chickens, -chickenPrice, "+ 1 Chicken", Color.green); }
    public void SellChicken() { ExecuteBuySell(-1, ref chickens, chickenPrice, "- 1 Chicken", Color.red); }

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

    private void UpdateUI()
    {
        cowCountText.text = $"{cows}";
        chickenCountText.text = $"{chickens}";
        moneyText.text = $"${money}";
    }
}
