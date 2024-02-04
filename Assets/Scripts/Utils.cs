using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils 
{
    public enum BonusTier { Normal, Rare, Legendary };
    public enum BonusStatus { None, Triggered, Ad, Drawn };

    public enum GameStatus
    {
        MainMenu,
        InGame,
        Pause,
        IsDead,
        GameDone,
        Leaderboard
    }

    public enum CurrScreen
    {
        Connection,
        MainMenu,
        Character,
        DailyReward,
        Settings,
        Game,
        GameOver,
    }

    public enum LogInState
    {
        Disconnected,
        Connecting,
        Registering,
        Connected
    }

    public static readonly string registerURL = "https://www.vylax.fr/coldrefuge/register.php";
    public static readonly string logInURL = "https://www.vylax.fr/coldrefuge/connexion.php";
    public static readonly string dailyURL = "https://www.vylax.fr/coldrefuge/getdaily.php";

    public static readonly int itemCount = 18;

    public static bool DisplayBonusSelectionUI(BonusStatus status) => (status == BonusStatus.Triggered || status == BonusStatus.Drawn) && !AdDisplay.adStarted;
    // TODO: remove this when done testing
    public static bool DisplayBonusTriggerButton(BonusStatus status) => status != BonusStatus.Triggered && status != BonusStatus.Drawn && !AdDisplay.adStarted;

    public static List<Bonus> bonuses = new List<Bonus>()
    {
        new Bonus("speed"),
        new Bonus("health"),
        new Bonus("strength"),
        new Bonus("range"),
        new Bonus("speed"),
        new Bonus("health"),
        new Bonus("strength"),
        new Bonus("range"),
        new Bonus("speed"),
        new Bonus("health"),
        new Bonus("strength"),
        new Bonus("range"),
        //new Bonus("bonus13"),
        //new Bonus("bonus14"),
    };

    [System.Serializable]
    public class Bonus
    {
        public string name;
        public Texture2D image;
        public BonusTier tier;

        public Bonus(string name, BonusTier tier=BonusTier.Normal)
        {
            this.name = name;
            this.image = SpriteToTexture2D(Resources.Load<Sprite>("Bonus/" + name));
            this.tier = tier;
        }

        // Override Equals method to define equality based on name and tier
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Bonus otherBonus = (Bonus)obj;
            return name == otherBonus.name && tier == otherBonus.tier;
        }

        // Override GetHashCode to satisfy compiler warnings
        public override int GetHashCode()
        {
            return HashCode.Combine(name, tier);
        }
    }

    public static Vector3 CustomClamp(float horizontal, float vertical)
    {
        float magnitude = Mathf.Sqrt(Mathf.Pow(horizontal, 2) + Mathf.Pow(vertical, 2));
        return magnitude > 1 ?
            new Vector3(horizontal / magnitude, vertical / magnitude, 0) :
            new Vector3(horizontal, vertical, 0);
    }

    public static Texture2D LoadItemTexture(int itemId)
    {
        // Assuming your sprites are in a "Sprites" folder inside the "Resources" folder
        string spritePath = "Items/" + itemId;

        // Load the sprite from the Resources folder
        Sprite sprite = Resources.Load<Sprite>(spritePath);

        if (sprite != null)
        {
            // Convert the sprite texture to Texture2D
            return SpriteToTexture2D(sprite);
        }
        else
        {
            Debug.LogError($"Sprite not found for item ID: {itemId}");
            return null;
        }
    }

    public static Sprite LoadItemSprite(int itemId)
    {
        // Assuming your sprites are in a "Sprites" folder inside the "Resources" folder
        string spritePath = "Items/" + itemId;

        // Load the sprite from the Resources folder
        Sprite sprite = Resources.Load<Sprite>(spritePath);

        if (sprite != null)
        {
            // Convert the sprite texture to Texture2D
            return sprite;
        }
        else
        {
            Debug.LogError($"Sprite not found for item ID: {itemId}");
            return null;
        }
    }

    // Helper method to convert Sprite to Texture2D
    private static Texture2D SpriteToTexture2D(Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.LogError("Sprite is null");
            return null;
        }

        // Create a new texture and copy the sprite texture data
        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        texture.SetPixels(sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y,
                        (int)sprite.rect.width, (int)sprite.rect.height));
        texture.Apply();

        return texture;
    }

    public static List<int> GenerateRandomIntegers(int N=12)
    {
        if (N > itemCount)
        {
            Debug.LogError("Number of requested random integers (N) should be less than or equal to itemCount.");
            return null;
        }

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < itemCount; i++)
        {
            availableIndices.Add(i);
        }

        List<int> randomIntegers = new List<int>();

        System.Random random = new System.Random();

        for (int i = 0; i < N; i++)
        {
            int randomIndex = random.Next(0, availableIndices.Count);
            randomIntegers.Add(availableIndices[randomIndex]);
            availableIndices.RemoveAt(randomIndex);
        }

        return randomIntegers;
    }
}
