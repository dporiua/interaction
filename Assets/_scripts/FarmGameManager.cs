using UnityEngine;
using TMPro;

public class FarmGameManager : MonoBehaviour
{
    [Header("Game Elements")]
    public TMP_Text cowCountText;
    public TMP_Text chickenCountText;
    public TMP_Text moneyText;

    [Header("Prices")]
    public int cowPrice = 100;
    public int chickenPrice = 20;

    [Header("Happiness Meters")]
    public float cowHappiness = 200f;
    public float chickenHappiness = 200f;

    [Header("Bulk Buying")]
    public GameObject bulkButtonsRow;
    public float buttonPressWindow = 5f;
    public int buttonPressThreshold = 20;
    public TMP_Text achievementPopupText;

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
    }

    void Update()
    {
        HandleReproduction();
        HandleButtonPressTimer();
        HandleAchievementPopup();
    }

    private void HandleReproduction()
    {
        // Reproduction logic for cows
        if (cows > 0)
        {
            cowReproductionTimer += Time.deltaTime;
            float cowReproductionTime = cowHappiness / cows;

            if (cowReproductionTimer >= cowReproductionTime)
            {
                cows++;
                cowReproductionTimer = 0f;
                UpdateUI();
            }
        }

        // Reproduction logic for chickens
        if (chickens > 0)
        {
            chickenReproductionTimer += Time.deltaTime;
            float chickenReproductionTime = chickenHappiness / chickens;

            if (chickenReproductionTimer >= chickenReproductionTime)
            {
                chickens++;
                chickenReproductionTimer = 0f;
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
            if (achievementPopupTimer >= 3f) 
            {
                achievementPopupText.gameObject.SetActive(false);
                isAchievementPopupVisible = false;
            }
        }
    }

    public void ButtonPress()
    {
        buttonPressCount++;
        buttonPressTimer = 0f;

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

    public void BuyCow() { ExecuteBuySell(1, ref cows, -cowPrice); }
    public void SellCow() { ExecuteBuySell(-1, ref cows, cowPrice); }
    public void BuyChicken() { ExecuteBuySell(1, ref chickens, -chickenPrice); }
    public void SellChicken() { ExecuteBuySell(-1, ref chickens, chickenPrice); }

    public void BuyCowsInBulk() { ExecuteBuySell(10, ref cows, -cowPrice * 10); }
    public void SellCowsInBulk() { ExecuteBuySell(-10, ref cows, cowPrice * 10); }
    public void BuyChickensInBulk() { ExecuteBuySell(10, ref chickens, -chickenPrice * 10); }
    public void SellChickensInBulk() { ExecuteBuySell(-10, ref chickens, chickenPrice * 10); }

    private void ExecuteBuySell(int amount, ref int animalCount, int cost)
    {
        if (animalCount + amount >= 0 && money + cost >= 0)
        {
            animalCount += amount;
            money += cost;
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
