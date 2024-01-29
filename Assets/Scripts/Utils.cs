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
        Game
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

    public static bool DisplayBonusSelectionUI(BonusStatus status) => (status == BonusStatus.Triggered || status == BonusStatus.Drawn) && !AdDisplay.adStarted;
    // TODO: remove this when done testing
    public static bool DisplayBonusTriggerButton(BonusStatus status) => status != BonusStatus.Triggered && status != BonusStatus.Drawn && !AdDisplay.adStarted;

    public static List<Bonus> bonuses = new List<Bonus>()
    {
        new Bonus("bonus1"),
        new Bonus("bonus2"),
        new Bonus("bonus3"),
        new Bonus("bonus4"),
        new Bonus("bonus5"),
        new Bonus("bonus6"),
        new Bonus("bonus7"),
        new Bonus("bonus8"),
        new Bonus("bonus9"),
        new Bonus("bonus10"),
        new Bonus("bonus11"),
        new Bonus("bonus12"),
        new Bonus("bonus13"),
        new Bonus("bonus14"),
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
            this.image = Resources.Load<Texture2D>(name);
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
}
