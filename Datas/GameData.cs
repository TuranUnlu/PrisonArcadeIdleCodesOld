using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "StarterKit/Game Data")]
public class GameData : ScriptableObject
{
    public int Level;
    public int LevelForCanvas;
    public bool Loop;
    public int Coin;

    [System.Serializable]
    public class player
    {
        public float Speed;
        public float SwerveSensivity;
        public float ClampX;
    }
    public player Player;

    [Serializable]
    public class TargetSprites
    {
        public Sprite food;
        public Sprite yard;
        public Sprite cloth;
        public Sprite money;
        public Sprite handcuff;
        public Sprite prisoner;
        // public Sprite 
    }

    public TargetSprites targetSprites;

    [Serializable]
    public class SpriteColors
    {
        public Color food;
        public Color yard;
        public Color cloth;
        public Color money;
    }

    public SpriteColors spriteColors;

    public GameObject foodPrefab;
    public GameObject moneyPrefab;
    public GameObject prisonerPrefab;
    public GameObject guardPrefab;

}
