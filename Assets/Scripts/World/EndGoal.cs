using UnityEngine;

public class EndGoal : MonoBehaviour
{
    // The data of the HUB this stage belongs to.
    public LevelData hubLevelData;
    HUDManager hudManager;
    private void Start()
    {
        hudManager = FindAnyObjectByType<HUDManager>();
    }

    private void Update()
    {
        if (hudManager.isStageResultsFinished && hudManager.isStageCompleteActive)
        {
            if (GameManager.Instance.playerController.confirmPressed)
            {
                AudioManager.Instance.PlaySFX("ui_confirm");
                LoadHub();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LevelManager levelManager = FindAnyObjectByType<LevelManager>();
            levelManager.CompleteStage();
        }
    }

    public void LoadHub()
    {
        SceneController.Instance.stageNameText.text = hubLevelData.levelName;
        SceneController.Instance.goalTipText.text = hubLevelData.loadingSubtitle;
        SceneController.Instance.loadingImage.sprite = hubLevelData.loadingImage;
        SceneController.Instance.LoadScene(hubLevelData.sceneToLoad);
    }
}
