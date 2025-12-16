using FMOD.Studio;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerController playerController;
    public GameData gameData;
    public GameSettings gameSettings;
    public CinemachineCamera currentCamera;

    public enum GameState
    {
        Menu,
        Gameplay,
        Cutscene
    }

    public GameState gameState;

    private void Awake()
    {
        if (Instance != null & Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadGame();
        LoadSettings();
    }

    private void Start()
    {
        Cursor.visible = false;

        gameState = GameState.Menu;
    }

    public void LoadGame()
    {
        gameData = SaveSystem.LoadGame();
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame(gameData);
    }

    public void SaveSettings()
    {
        SaveSystem.SaveSettings(gameSettings);
    }

    public void LoadSettings()
    {
        gameSettings = SaveSystem.LoadSettings();
    }
    public void SaveLevelStats(LevelStats newStats)
    {
        LevelStats existing = gameData.levelStats.Find(l => l.levelID == newStats.levelID);

        if (existing != null)
        {
            existing.completedLevel = true;

            if (existing.bestTime <= 0f || newStats.bestTime < existing.bestTime)
                existing.bestTime = newStats.bestTime;

            existing.collectedGallery |= newStats.collectedGallery;
            existing.collectedCrystal |= newStats.collectedCrystal;
            existing.rank = newStats.rank; // TODO: compare rank
            existing.score = Mathf.Max(existing.score, newStats.score);
        }
        else
        {
            gameData.levelStats.Add(newStats);
        }

        SaveGame();
    }

    public void ChangeGameState(GameState newState)
    {
        if (newState != gameState)
        {
            gameState = newState;
        }
    }

    /// <summary>
    /// Use this to toggle a setting in the menu.
    /// </summary>
    /// <param name="settingToToggle"></param>
    /// <returns></returns>
    public void ToggleSetting(ref bool settingToToggle)
    {
        if (settingToToggle)
        {
            Debug.Log("Set: " + nameof(settingToToggle) + " to" + false);
            settingToToggle = false;
        }
        else
        {
            Debug.Log("Set: " + nameof(settingToToggle) + "to" + true);
            settingToToggle = true;
        }

        SaveSettings();
    }

    #region Mechanics/Stats
    /// <summary>
    /// Unlock a mechanic for gameplay usage.
    /// </summary>
    /// <param name="mechanic"> The mechanic to be unlocked. </param>
    public void UnlockMechanic(GameData.PlayerMechanics mechanic)
    {
        if (!gameData.unlockedMechanics.Contains(mechanic))
        {
            gameData.unlockedMechanics.Add(mechanic);
        }
    }

    /// <summary>
    /// Remove a mechanic from gameplay usage.
    /// </summary>
    /// <param name="mechanic"> The mechanic to be removed. </param>
    public void RemoveMechanic(GameData.PlayerMechanics mechanic)
    {
        if (gameData.unlockedMechanics.Contains(mechanic))
        {
            gameData.unlockedMechanics.Remove(mechanic);
        }
    }

    /// <summary>
    /// Checks if the mechanic is unlocked and enables it.
    /// </summary>
    /// <param name="mechanic">The mechanic to check.</param>
    /// <param name="enable">The mechanic bool in PlayerController.</param>
    public void CheckMechanic(GameData.PlayerMechanics mechanic, out bool enable)
    {
        if (gameData.unlockedMechanics.Contains(mechanic))
        {
            enable = true;
        }
        else
        {
            enable = false;
        }
    }

    /// <summary>
    /// Change the player's lives count.
    /// </summary>
    /// <param name="amount">Add -N to subtract lives or +N to add.</param>
    public void ChangeLives(int amount)
    {
        gameData.playerLives += amount;
    }
    #endregion

    #region CHEATS

    /// <summary>
    /// Unlock all mechanics.
    /// </summary>
    public void UnlockAllMechanics()
    {
        UnlockMechanic(GameData.PlayerMechanics.DoubleJump);
        UnlockMechanic(GameData.PlayerMechanics.Dash);
    }

    /// <summary>
    /// Remove all mechanics
    /// </summary>
    public void RemoveAllMechanics()
    {
        gameData.unlockedMechanics.Clear();
    }

    public void LoadLevel(LevelData levelData)
    {
        SceneController.Instance.stageNameText.text = levelData.levelName;
        SceneController.Instance.goalTipText.text = levelData.loadingSubtitle;
        SceneController.Instance.loadingImage.sprite = levelData.loadingImage;
        SceneController.Instance.LoadScene(levelData.sceneToLoad);
    }

    public void LoadCutscene(LevelData levelData)
    {
        SceneController.Instance.LoadRoom(levelData.sceneToLoad);

        if (levelData.stageType == LevelData.StageType.Cutscene)
        {
            AudioManager.Instance.StopMusic(true);
        }
    }

    #endregion

    #region Helper Methods
    public void MainCamPrioritize()
    {
        currentCamera.Prioritize();
    }
    #endregion
}
