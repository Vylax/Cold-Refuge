using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using static Utils;
using System.Linq;
using System;

public class BonusManager : MonoBehaviour
{
    private static BonusStatus bonusStatus = BonusStatus.None;
    //private static bool bonusTriggered = false;
    private Queue<System.Action> bonusQueue = new Queue<System.Action>();

    public List<Bonus> playerBonuses = new List<Bonus>();
    public List<Bonus> drawnBonusPool = new List<Bonus>(); // Your available bonuses
    public BonusTier currBonusPoolTier;

    private bool wasCalledFromReroll = false;

    public bool debug = false;
    private static BonusManager instance;

    public static BonusManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BonusManager>();

                if (instance == null)
                {
                    Debug.LogWarning("No BonusManager found in the scene. Creating a new one.");
                    instance = GameManager.Instance.gameObject.AddComponent<BonusManager>();
                }
            }

            // Return the instance
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        InitUI();
    }

    public void TriggerBonus(BonusTier bonusPooltier, bool calledFromReroll = false)
    {
        bonusStatus = BonusStatus.Triggered;

        // Make sure the reroll was accounted for if it is one
        wasCalledFromReroll = calledFromReroll;

        // Update bonus pool tier
        currBonusPoolTier = bonusPooltier;

        // Drawn bonus pool here with the correct tier
        DrawnBonusPool(5, bonusPooltier);

        // Set bonusStatus to Drawn
    }

    private void BonusDone()
    {
        // turn the bool back to false when we're done and process the next bonus if there is one
        //bonusTriggered = false;
        bonusStatus = BonusStatus.None;
        TryTriggerBonus();
    }

    // Call this method to queue another call
    public void QueueBonus(BonusTier bonusPool)
    {
        bonusQueue.Enqueue(() => TriggerBonus(bonusPool));
        TryTriggerBonus();
    }

    // Check the bool condition and process the queue
    private void TryTriggerBonus()
    {
        if (bonusStatus != BonusStatus.None) return;

        while (bonusQueue.Count > 0)
        {
            wasCalledFromReroll = false;
            System.Action methodToCall = bonusQueue.Dequeue();
            methodToCall?.Invoke();
            break;
        }
    }

    private void ApplyBonus(Bonus bonus)
    {
        // TODO: Apply selected buff here
        // if the bonus name contains "speed", increase the player's speedFactor
        // if the bonus name contains "health", increase the player's healthFactor
        // if the bonus name contains "strength", increase the player's strengthFactor
        // if the bonus name contains "range", increase the player's rangeFactor
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        if(bonus.name.Contains("speed"))
        {
            player.speedFactor += 0.1f;
        }
        else if(bonus.name.Contains("health"))
        {
            player.healthFactor += 0.1f;
        }
        else if(bonus.name.Contains("strength"))
        {
            player.strengthFactor += 0.1f;
        }
        else if(bonus.name.Contains("range"))
        {
            player.rangeFactor += 0.1f;
        }

        // Add bonus to the player's owned bonuses
        playerBonuses.Add(bonus);

        // Close the bonus selection UI
        BonusDone();
    }

    private void DrawnBonusPool(int bonusCount, BonusTier bonusPooltier)
    {
        List<int> indexes = GetRandomIndices();

        drawnBonusPool.Clear();
        foreach (int i in indexes)
        {
            drawnBonusPool.Add(bonuses[i]);
        }

        // Update the UI accordingly
        InitUI();
    }

    private List<int> GetRandomIndices()
    {
        // Get a list of indices for elements in 'bonuses' that are not in 'playerBonuses'
        List<int> availableIndices = Enumerable.Range(0, bonuses.Count)
            .Where(i => !playerBonuses.Any(pb => pb.Equals(bonuses[i])))
            .ToList();

        // Ensure that there are at least 5 available indices
        if (availableIndices.Count == 0)
        {
            throw new InvalidOperationException("No index available left.");
        }

        // Shuffle the available indices
        availableIndices = availableIndices.OrderBy(i => Guid.NewGuid()).ToList();

        // Take the first 5 indices or less if there isn't 5
        int numRandomIndices = Math.Min(5, availableIndices.Count);
        List<int> randomIndices = availableIndices.Take(numRandomIndices).ToList();

        return randomIndices;
    }

    void OnGUI()
    {
        // Set up a GUIStyle for centered label and button text
        centeredLabelStyle = GUI.skin.GetStyle("Label");
        centeredLabelStyle.alignment = TextAnchor.UpperCenter;

        buttonTextStyle = new GUIStyle(GUI.skin.button);

        if (debug && DisplayBonusTriggerButton(bonusStatus))
        {
            float squareButtonSize = Screen.width / 4f;

            // Display a square button at the bottom-left corner
            if (GUI.Button(new Rect(10f, Screen.height - squareButtonSize - 10f, squareButtonSize, squareButtonSize), "Bonus"))
            {
                QueueBonus(BonusTier.Normal);
            }
        }

        if (!DisplayBonusSelectionUI(bonusStatus)) return;

        // Display Bonus selection UI
        // Wait until player selects a bonus (or watch an add later on)

        for (int i = 0; i < drawnBonusPool.Count; i++)
        {
            string buttonText = $"{drawnBonusPool[i].name}";

            if (GUI.Button(BonusButtonRect(i, buttonText), GUIContent.none, buttonTextStyle))
            {
                // Handle bonus selection logic here
                ApplyBonus(drawnBonusPool[i]);
            }

            float iconSize = buttonSize - 10f; // Adjust this value to leave some padding
            Rect iconRect = new Rect(BonusButtonRect(i, buttonText).x + (buttonSize - iconSize) / 2f, BonusButtonRect(i, buttonText).y + (buttonSize - iconSize) / 2f, iconSize, iconSize);

            GUI.DrawTexture(iconRect, drawnBonusPool[i].image, ScaleMode.ScaleToFit);
        }

        // Display the reroll option only if it wasn't called before
        if (!wasCalledFromReroll)
        {
            buttonTextStyle.fontSize = CalculateFontSize(adButtonText, adButtonWidth, adButtonHeight);

            if (GUI.Button(new Rect(centerX - adButtonWidth / 2, centerY + (buttonRadius + buttonSize * 3 / 2f) + 25, adButtonWidth, adButtonHeight), adButtonText, buttonTextStyle))
            {
                // Handle ad logic and reroll bonus pool here
                Instance.GetComponent<AdDisplay>().TryShowAd(() => RerollBonusPool());
            }
        }

        for (int i = 0; i < playerBonuses.Count; i++)
        {
            float labelX = labelSpacing + i * (labelWidth + labelSpacing);
            float labelY = Screen.height - labelHeight - labelSpacing;

            // Display GUI Box behind each owned bonus
            GUI.Box(new Rect(labelX - labelSpacing / 2, labelY - 5, labelWidth + labelSpacing, boxHeight), "");
            GUI.Label(new Rect(labelX, labelY, labelWidth, labelHeight), "" + playerBonuses[i].image, centeredLabelStyle);
            // You can also display the image using GUI.DrawTexture if you have the Texture2D
        }
    }

    // Helper method to calculate font size based on text and button dimensions
    int CalculateFontSize(string text, float width, float height)
    {
        int fontSize = 1;
        GUIStyle style = new GUIStyle(GUI.skin.button);

        while (style.CalcSize(new GUIContent(text)).x < width && style.CalcSize(new GUIContent(text)).y < height)
        {
            style.fontSize = fontSize++;
        }

        return fontSize - 2;
    }

    GUIStyle centeredLabelStyle;
    GUIStyle buttonTextStyle;
    float centerX;
    float centerY;
    float buttonRadius;
    float buttonSize;
    float angleIncrement;

    float adButtonWidth;
    float adButtonHeight;

    string adButtonText;

    // Display possessed bonuses from bottom left to bottom right
    float labelWidth;
    float labelHeight;
    float labelSpacing;
    float boxHeight;

    private void InitUI()
    {
        // Display buttons for choosing bonuses in a circular arrangement
        centerX = Screen.width / 2f;
        centerY = Screen.height / 2f;
        buttonRadius = 125f; // Increased button size
        buttonSize = 250f; // Increased button size
        angleIncrement = 360f / drawnBonusPool.Count;

        // Display ad button twice as big as bonus buttons, centered below the circle
        adButtonWidth = 400f;
        adButtonHeight = 100f;

        adButtonText = "Reroll with Ad";

        // Display possessed bonuses from bottom left to bottom right
        labelWidth = 200;
        labelHeight = 25;
        labelSpacing = 10;
        boxHeight = labelHeight + 10;
    }

    private Rect BonusButtonRect(int i, string buttonText)
    {
        float angle = i * angleIncrement - 90; // Place the first bonus at 12 o'clock
        float buttonX = centerX + Mathf.Cos(angle * Mathf.Deg2Rad) * (buttonRadius + buttonSize);
        float buttonY = centerY + Mathf.Sin(angle * Mathf.Deg2Rad) * (buttonRadius + buttonSize);

        buttonTextStyle.fontSize = CalculateFontSize(buttonText, buttonSize, buttonSize);

        return new Rect(buttonX - buttonSize / 2f, buttonY - buttonSize / 2f, buttonSize, buttonSize);
    }

    /// <summary>
    /// Method to be called from other scripts to trigger a bonus
    /// </summary>
    public void PickUpBonus()
    {
        QueueBonus(BonusTier.Normal);
    }

    public void PickUpBonus(BonusTier tier)
    {
        QueueBonus(tier);
    }

    // Implement your logic to reroll the bonus pool here
    private void RerollBonusPool()
    {
        // Add your logic to change or refresh the bonus pool
        Debug.Log("Rerolling bonus pool...");

        GetComponent<BonusManager>();
        TriggerBonus(currBonusPoolTier, true);
    }
}
