using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using static Utils;

public class BonusManager : MonoBehaviour
{
    private static BonusStatus bonusStatus = BonusStatus.None;
    //private static bool bonusTriggered = false;
    private Queue<System.Action> bonusQueue = new Queue<System.Action>();

    public List<Bonus> playerBonuses = new List<Bonus>();
    public List<Bonus> drawnBonusPool = new List<Bonus>()
    { 
        new Bonus("1",null),
        new Bonus("2",null),
        new Bonus("3",null),
        new Bonus("4",null),
        new Bonus("5",null),
    }; // Your available bonuses

    /*// TODO: REMVOE WHEN DONE TESTING
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            
        }
    }*/

    private IEnumerator WaitABit()
    {
        yield return new WaitForSeconds(5);
        BonusDone();
    }


    public void TriggerBonus(bonusPool bonusPool) {

        // Do stuff here

        // Display Bonus selection UI
        // Note, maybe just enable the bonus selection display when bonusTriggered is set to true

        // Wait until player selects a bonus (or watch an add later on)
        // Apply selected buff here
        // Update the global bonus pool so that the buff cannot be drawn again if it is unique

        // TODO: remove the intermediate coroutine when done testing and call BonusDone() directly instead
        StartCoroutine(WaitABit());
    }

    public void BonusDone()
    {
        // turn the bool back to false when we're done and process the next bonus if there is one
        //bonusTriggered = false;
        bonusStatus = BonusStatus.None;
        TryTriggerBonus();
    }

    // Call this method to queue another call
    public void QueueBonus(bonusPool bonusPool)
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
            System.Action methodToCall = bonusQueue.Dequeue();
            methodToCall?.Invoke();
            //bonusTriggered = true;
            bonusStatus = BonusStatus.Triggered;
            break;
        }
    }

    void OnGUI()
    {
        if (bonusStatus != BonusStatus.Triggered && !AdDisplay.adStarted)
        {
            float squareButtonSize = Screen.width / 4f;

            // Display a square button at the bottom-left corner
            if (GUI.Button(new Rect(10f, Screen.height - squareButtonSize - 10f, squareButtonSize, squareButtonSize), "Bonus"))
            {
                QueueBonus(bonusPool.Normal);
            }
        }

        if (bonusStatus != BonusStatus.Triggered || AdDisplay.adStarted) return;

        // Set up a GUIStyle for centered label and button text
        GUIStyle centeredLabelStyle = GUI.skin.GetStyle("Label");
        centeredLabelStyle.alignment = TextAnchor.UpperCenter;

        GUIStyle buttonTextStyle = new GUIStyle(GUI.skin.button);

        // Display buttons for choosing bonuses in a circular arrangement
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        float buttonRadius = 125f; // Increased button size
        float buttonSize = 250f; // Increased button size
        float angleIncrement = 360f / Mathf.Min(drawnBonusPool.Count, 5);

        for (int i = 0; i < Mathf.Min(drawnBonusPool.Count, 5); i++)
        {
            float angle = i * angleIncrement - 90; // Place the first bonus at 12 o'clock
            float buttonX = centerX + Mathf.Cos(angle * Mathf.Deg2Rad) * (buttonRadius + buttonSize);
            float buttonY = centerY + Mathf.Sin(angle * Mathf.Deg2Rad) * (buttonRadius + buttonSize);

            string buttonText = "Choose Bonus " + (i + 1);
            buttonTextStyle.fontSize = CalculateFontSize(buttonText, buttonSize, buttonSize);

            if (GUI.Button(new Rect(buttonX - buttonSize / 2f, buttonY - buttonSize / 2f, buttonSize, buttonSize), buttonText, buttonTextStyle))
            {
                // Handle bonus selection logic here
                playerBonuses.Add(drawnBonusPool[i]);
            }
        }

        // Display ad button twice as big as bonus buttons, centered below the circle
        float adButtonWidth = 400f;
        float adButtonHeight = 100f;

        string adButtonText = "Reroll with Ad";
        buttonTextStyle.fontSize = CalculateFontSize(adButtonText, adButtonWidth, adButtonHeight);

        if (GUI.Button(new Rect(centerX - adButtonWidth / 2, centerY + (buttonRadius + buttonSize*3/2f)+25, adButtonWidth, adButtonHeight), adButtonText, buttonTextStyle))
        {
            // Handle ad logic and reroll bonus pool here
            GameManager.Instance.GetComponent<AdDisplay>().TryShowAd();
        }

        // Display possessed bonuses from bottom left to bottom right
        float labelWidth = 200;
        float labelHeight = 25;
        float labelSpacing = 10;
        float boxHeight = labelHeight + 10;

        for (int i = 0; i < playerBonuses.Count; i++)
        {
            float labelX = labelSpacing + i * (labelWidth + labelSpacing);
            float labelY = Screen.height - labelHeight - labelSpacing;

            // Display GUI Box behind each owned bonus
            GUI.Box(new Rect(labelX - labelSpacing / 2, labelY - 5, labelWidth + labelSpacing, boxHeight), "");
            GUI.Label(new Rect(labelX, labelY, labelWidth, labelHeight), "" + playerBonuses[i].name, centeredLabelStyle);
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






}
