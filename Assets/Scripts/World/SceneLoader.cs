using System.Collections;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public enum LoadType
    {
        Room,
        Scene
    }

    [Header("LoadGame Type")]
    public LoadType loadType;

    public LevelData levelData;
    public GameObject indicatorPrefab;

    GameObject indicatorInstance;

    HUDManager hudManager;
    public bool stageHUDActive;
    public bool canOpenHud = true;
    public bool canCloseHud = false;

    private void Start()
    {
        hudManager = FindAnyObjectByType<HUDManager>();
    }

    private void Update()
    {
        if (stageHUDActive)
        {
            if (GameManager.Instance.playerController.confirmPressed)
            {
                AudioManager.Instance.PlaySFX("ui_confirm");
                LoadScene();
            }
            else if (GameManager.Instance.playerController.cancelPressed)
            {
                if (!canCloseHud) return;

                AudioManager.Instance.PlaySFX("ui_cancel");
                CancelStageHUD();
            }
        }
    }

    public void OpenStageHUD()
    {
        if (stageHUDActive) return;

        stageHUDActive = true;
        GameManager.Instance.playerController.DisableAllControl();
        hudManager.EnableStageSelect(levelData);
        AudioManager.Instance.PlaySFX("ui_navigate");
        StartCoroutine(HudStatusDelayClose(true));
    }

    public void CancelStageHUD()
    {
        if (!stageHUDActive) return;

        stageHUDActive = false;
        canCloseHud = false;
        GameManager.Instance.playerController.EnableControls();
        hudManager.DisableStageSelect();
        StartCoroutine(HudStatusDelay(true));
    }

    public void LoadScene()
    {
        switch (loadType)
        {
            case LoadType.Room:
                LoadRoom();
                break;
            case LoadType.Scene:
                LoadLevel();
                break;
            default:
                break;
        }
    }

    public void LoadRoom()
    {
        SceneController.Instance.LoadRoom(levelData.sceneToLoad);
    }

    public void LoadLevel()
    {
        SceneController.Instance.stageNameText.text = levelData.levelName;
        SceneController.Instance.goalTipText.text = levelData.loadingSubtitle;
        SceneController.Instance.loadingImage.sprite = levelData.loadingImage;
        SceneController.Instance.LoadScene(levelData.sceneToLoad);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            indicatorInstance = Instantiate(indicatorPrefab, new Vector2(transform.position.x, transform.position.y + 2), Quaternion.identity);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Destroy(indicatorInstance);
    }

    public IEnumerator HudStatusDelay(bool status)
    {
        yield return new WaitForSeconds(0.5f);
        canOpenHud = status;
    }

    public IEnumerator HudStatusDelayClose(bool status)
    {
        yield return new WaitForSeconds(0.5f);
        canCloseHud = status;
    }
}
