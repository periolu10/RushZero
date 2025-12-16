using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Simple class that holds level data for each playable stage.
    public LevelData levelData;

    HUDManager hudManager;

    [Header("Current Attempt Bools")]
    public bool zeroCrystalCollected = false;

    [Header("Level Transform Points")]
    public Transform startPoint;

    private void Start()
    {
        zeroCrystalCollected = false;

        GameManager.Instance.gameState = GameManager.GameState.Gameplay;
        hudManager = FindAnyObjectByType<HUDManager>();

        CheckCollectables();

        if (levelData.stageType == LevelData.StageType.Hub)
        {
            GameManager.Instance.gameData.savedHub = levelData;
            GameManager.Instance.SaveGame();
            Debug.Log("Saved: " + GameManager.Instance.gameData.savedHub);
        }

        if (levelData.stageType == LevelData.StageType.Cutscene)
        {
            GameManager.Instance.gameState = GameManager.GameState.Cutscene;
        }
    }

    public void CompleteStage()
    {
        GameManager.Instance.gameState = GameManager.GameState.Menu;

        // Disable player Controls
        GameManager.Instance.playerController.DisableAllControl();

        // Enable level complete HUD
        StartCoroutine(hudManager.EnableStageComplete(levelData));
    }

    public void CollectZeroCrystal()
    {
        zeroCrystalCollected = true;
        hudManager.CollectableNotification();
    }

    public void CheckCollectables()
    {
        LevelStats savedStats = GameManager.Instance.gameData.GetLevelStats(levelData.levelName);

        if (savedStats == null) return;

        if (levelData.hasZeroCrystal && savedStats.collectedCrystal)
        {
            // Find and disable the crystal object.
            ZeroCrystal zeroCrystal = FindAnyObjectByType<ZeroCrystal>();
            if (zeroCrystal != null) Destroy(zeroCrystal.gameObject);
            zeroCrystalCollected = true;
        }
    }
}
