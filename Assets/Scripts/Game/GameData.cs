using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public int playerLives = 3;

    public enum PlayerMechanics
    {
        DoubleJump = 0,
        Dash = 1
    }

    [Header("Unlocked Mechanics")]
    public List<PlayerMechanics> unlockedMechanics = new();

    [Header("Quest Progression")]
    public Dictionary<string, bool> quests = new();

    [Header("Level Stats")]
    public List<LevelStats> levelStats = new();

    public LevelStats GetLevelStats(string levelID)
    {
        return levelStats.Find(l => l.levelID == levelID);
    }

    public LevelData savedHub;
}

[Serializable]
public class LevelStats
{
    public string levelID;
    public float bestTime;
    public bool completedLevel;
    public bool collectedGallery;
    public bool collectedCrystal;
    public string rank;
    public float score;
}
