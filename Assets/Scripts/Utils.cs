using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils 
{
    public enum bonusPool { Normal, Rare, Legendary };
    public enum BonusStatus { None, Triggered, Ad };

    [System.Serializable]
    public class Bonus
    {
        public string name;
        public Texture2D image;

        public Bonus(string name, Texture2D image)
        {
            this.name = name;
            this.image = image;
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
