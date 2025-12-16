using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class HUDManager : MonoBehaviour
{
    [Header("Stage Type")]
    public LevelData.StageType stageType;
    LevelManager levelManager;

    Camera uiCamera;
    Canvas canvas;

    [Header("Global Elements")]
    [SerializeField] GameObject mainActionHUD;
    [SerializeField] GameObject lives;
    [SerializeField] TMP_Text lifeCount;

    [Header("Action Only Elements")]
    [SerializeField] GameObject time;
    [SerializeField] TMP_Text timer;
    [SerializeField] GameObject score;
    float scorePoints;
    [SerializeField] TMP_Text scoreCount;
    [SerializeField] GameObject playerHP;
    [SerializeField] Image playerHealthBar;
    float targetFillAmountPlayer  = 1;
    [SerializeField] GameObject bossHP;
    [SerializeField] Image bossHealthBar;
    float targetFillAmountBoss;
    [SerializeField] GameObject collectableNotif;

    [Header("Hub Only Elements")]
    [SerializeField] GameObject location;
    [SerializeField] TMP_Text title;
    [SerializeField] Image line;

    [Header("Cutscene Panel")]
    [SerializeField] GameObject cutscenePanel;

    [Header("DialoguePanel")]
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] Image dialogueIcon;
    [SerializeField] Image dialogueBG;
    [SerializeField] Image nameBG;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] TMP_Text nameText;
    public bool isTyping = false;
    public Coroutine typeCoroutine;

    [Header("Stage Select Screen")]
    [SerializeField] GameObject stageSelectScreen;
    [SerializeField] TMP_Text stageTypeText;
    [SerializeField] TMP_Text stageNameText;
    [SerializeField] TMP_Text bestTimeText;
    [SerializeField] TMP_Text highScoreText;

    [SerializeField] Image stagePreviewImage;
    [SerializeField] Image bestRankImage;
    [SerializeField] Image badgeGallery;
    [SerializeField] Image badgeLevelDone;
    [SerializeField] Image badgeZeroCrystal;

    [Header("Stage Complete Screen")]
    [SerializeField] GameObject stageCompleteScreen;
    [SerializeField] TMP_Text finalTimeText;
    [SerializeField] TMP_Text finalScoreText;

    [SerializeField] Image rankImage;
    [SerializeField] Image badgeGalleryCollected;
    [SerializeField] Image badgeLevelDoneCollected;
    [SerializeField] Image badgeZeroCrystalCollected;

    [SerializeField] GameObject legendButtons;
    public bool isStageCompleteActive;
    public bool isStageResultsFinished;

    [Header("Timer Settings")]
    bool isTimerRunning = false;
    public float currentTime = 0f;
    float initialTime = 0f;

    [Header("QTE Settings")]
    [SerializeField] GameObject QTEPanel;
    public List<Sprite> buttonPromptsKB;
    public Image timerBar;
    public Image prompt;

    private void Start()
    {
        stageCompleteScreen.SetActive(false);
        stageSelectScreen.SetActive(false);
        QTEPanel.SetActive(false);
        dialoguePanel.SetActive(false);
        mainActionHUD.SetActive(true);

        levelManager = FindAnyObjectByType<LevelManager>();
        stageType = levelManager.levelData.stageType;

        uiCamera = GameObject.FindWithTag("UI_Camera").GetComponent<Camera>();
        canvas = GetComponent<Canvas>();

        UpdateHealthBar();

        SetupHUD();
        SetupText();

        StartTimer();

        isStageCompleteActive = false;

        // Show location only on start if on Hub level
        if (stageType == LevelData.StageType.Hub && !location.GetComponent<LocationHUD>().locationShown)
        {
            location.SetActive(true);
            location.GetComponent<LocationHUD>().locationShown = true;
        }
    }

    private void Update()
    {
        UpdateTimerDisplay();
        UpdateHealthBar();
        UpdateScore();
    }

    void SetupHUD()
    {
        // Make sure main action is enabled
        mainActionHUD.SetActive(true);

        // Setup UI Camera
        canvas.worldCamera = uiCamera;

        // Always enable life counter
        lives.SetActive(true);

        // Always disable notif
        collectableNotif.SetActive(false);

        // First, determine which UI elements to show, depending on level type (Hub, Action)
        if (stageType == LevelData.StageType.Hub)
        {
            title.text = levelManager.levelData.levelName;

            time.SetActive(false);
            score.SetActive(false);
            playerHP.SetActive(false);
            bossHP.SetActive(false);

            cutscenePanel.SetActive(false);
        }
        else if (stageType == LevelData.StageType.Action)
        {
            time.SetActive(true);
            score.SetActive(true);
            playerHP.SetActive(true);
            bossHP.SetActive(false);
            location.SetActive(false);

            cutscenePanel.SetActive(false);
        }
        else if (stageType == LevelData.StageType.Boss)
        {
            time.SetActive(true);
            score.SetActive(true);
            playerHP.SetActive(true);
            bossHP.SetActive(true);
            location.SetActive(false);

            cutscenePanel.SetActive(false);
        }
        else if (stageType == LevelData.StageType.Cutscene)
        {
            time.SetActive(false);
            score.SetActive(false);
            playerHP.SetActive(false);
            bossHP.SetActive(false);
            location.SetActive(false);

            cutscenePanel.SetActive(true);
        }
    }

    void SetupText()
    {
        // Always update lives
        lifeCount.text = GameManager.Instance.gameData.playerLives.ToString();

        if (stageType == LevelData.StageType.Hub)
        {
            // Nothing Because no timers are enabled on hub stages.
        }
        else if (stageType == LevelData.StageType.Action)
        {
            timer.text = "00:00:00";
            scoreCount.text = "0";
            playerHealthBar.fillAmount = 1;
        }
        else if (stageType == LevelData.StageType.Boss)
        {
            timer.text = "00:00:00";
            scoreCount.text = "0";
            playerHealthBar.fillAmount = 1;
            bossHealthBar.fillAmount = 1;
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        targetFillAmountPlayer = currentHealth / maxHealth;
    }

    void UpdateHealthBar()
    {
        playerHealthBar.fillAmount = Mathf.Lerp(playerHealthBar.fillAmount, targetFillAmountPlayer, Time.deltaTime * 5);
    }

    void UpdateScore()
    {
        scoreCount.text = scorePoints.ToString();
    }
    public void ShakeHealthBar()
    {
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        Vector3 originalPosition = playerHP.GetComponent<RectTransform>().anchoredPosition;
        float elapsed = 0f;

        while (elapsed < 0.2f)
        {
            float offsetX = Random.Range(-1.5f, 1.5f) * 5;
            float offsetY = Random.Range(-1.5f, 1.5f) * 5;

            playerHP.GetComponent<RectTransform>().anchoredPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerHP.GetComponent<RectTransform>().anchoredPosition = originalPosition;
    }

    void UpdateTimerDisplay()
    {
        if (!isTimerRunning) return;

        currentTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        int milliseconds = Mathf.FloorToInt((currentTime * 100f) % 100f);

        timer.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        currentTime = initialTime;
        timer.text = "00:00:00";
    }

    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);

        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void CollectableNotification()
    {
        collectableNotif.SetActive(true);
    }

    public void ShowQTE(string name)
    {
        QTEPanel.SetActive(true);

        ShowKey(name);

        // Do other stuff
    }

    public void UpdateQTEFill(float time)
    {
        timerBar.fillAmount = 1f - time;
    }

    public void HideQTE()
    {
        QTEPanel.SetActive(false);
    }

    void ShowKey(string name)
    {
        if (name == "Up") prompt.sprite = buttonPromptsKB[0];
        else if (name == "Down") prompt.sprite = buttonPromptsKB[1];
        else if (name == "Left") prompt.sprite = buttonPromptsKB[2];
        else if (name == "Right") prompt.sprite = buttonPromptsKB[3];
    }

    public void AddScore(float scoreToAdd)
    {
        scorePoints += scoreToAdd;
    }

    #region Stage Select
    public void EnableStageSelect(LevelData levelData)
    {
        // Enable stage hud object and hide main hud
        stageSelectScreen.SetActive(true);
        mainActionHUD.SetActive(false);

        // STAGE TYPE
        if (levelData.stageType == LevelData.StageType.Action) stageTypeText.text = "ACTION STAGE";
        else if (levelData.stageType == LevelData.StageType.Boss) stageTypeText.text = "BOSS STAGE";

        // STAGE NAME
        stageNameText.text = levelData.levelName;

        LevelStats savedStats = GameManager.Instance.gameData.GetLevelStats(levelData.levelName);

        if (savedStats != null)
        {
            // TIME
            bestTimeText.text = FormatTime(savedStats.bestTime);

            // SCORE
            highScoreText.text = savedStats.score.ToString();

            // BADGES / COMPLETION
            badgeGallery.gameObject.SetActive(levelData.hasGallery);
            badgeZeroCrystal.gameObject.SetActive(levelData.hasZeroCrystal);
            badgeLevelDone.gameObject.SetActive(true);

            // Change badge colors for completion
            badgeGallery.color = savedStats.collectedGallery ? Color.cyan : Color.black;
            badgeZeroCrystal.color = savedStats.collectedCrystal ? Color.white : Color.black;
            badgeLevelDone.color = savedStats.completedLevel ? Color.yellow : Color.black;
        }
        else
        {
            bestTimeText.text = "00:00:00";
            highScoreText.text = "0";

            badgeGallery.gameObject.SetActive(levelData.hasGallery);
            badgeZeroCrystal.gameObject.SetActive(levelData.hasZeroCrystal);
            badgeLevelDone.gameObject.SetActive(true);

            badgeGallery.color = Color.black;
            badgeLevelDone.color = Color.black;
            badgeZeroCrystal.color = Color.black;
        }
    }

    public void DisableStageSelect()
    {
        // Enable main hud object and hide stage hud
        stageSelectScreen.SetActive(false);
        mainActionHUD.SetActive(true);
    }
    #endregion

    #region Level Complete
    public IEnumerator EnableStageComplete(LevelData levelData)
    {
        isStageCompleteActive = true;
        isStageResultsFinished = false;

        // Enable complete hud and disable main hud
        stageCompleteScreen.SetActive(true);
        legendButtons.SetActive(false);
        mainActionHUD.SetActive(false);

        // Start Music
        AudioManager.Instance.PlayMusic("StageClear_Normal");
        AudioManager.Instance.SetParameter("ResultShow", 0);

        // Update text values and initialize images
        PauseTimer();
        finalTimeText.text = "00:00:00";
        finalScoreText.text = "0000";
        badgeGalleryCollected.gameObject.SetActive(false);
        badgeLevelDoneCollected.gameObject.SetActive(false);
        badgeZeroCrystalCollected.gameObject.SetActive(false);
        rankImage.gameObject.SetActive(false);

        // badge color
        badgeGalleryCollected.color = Color.black;
        badgeLevelDoneCollected.color = Color.yellow;
        badgeZeroCrystalCollected.color = Color.black;

        // START SHOWING TIME AND TEXT
        yield return new WaitForSeconds(0.5f);
        finalTimeText.text = timer.text;
        yield return new WaitForSeconds(0.5f);
        finalScoreText.text = scoreCount.text;

        yield return new WaitForSeconds(1f);

        // NEXT SHOW AVAILABLE BADGES
        // icon enable/disable (level done badge should always be active)
        badgeGalleryCollected.gameObject.SetActive(levelData.hasGallery);
        badgeZeroCrystalCollected.gameObject.SetActive(levelData.hasZeroCrystal);
        badgeLevelDoneCollected.gameObject.SetActive(true);
        // TO-DO: here they should also be colored if collected.

        if (levelManager.zeroCrystalCollected) badgeZeroCrystalCollected.color = Color.white;

        yield return new WaitForSeconds(1f);

        // SHOW RANK

        // TO-DO: determine rank image from list, depending on the player's performance
        // DEPENDENCY: NEEDS RANK SYSTEM.
        rankImage.color = Color.yellow;

        rankImage.gameObject.SetActive(true);
        AudioManager.Instance.PlaySFX("perfect_parry");

        yield return new WaitForSeconds(1);
        isStageResultsFinished = true;
        legendButtons.SetActive(true);
        AudioManager.Instance.SetParameter("ResultShow", 1);

        // Convert LevelData to LevelStats and include the timer
        LevelStats statsToSave = new()
        {
            levelID = levelData.levelName,
            bestTime = currentTime,
            completedLevel = true,
            collectedGallery = false,
            collectedCrystal = levelManager.zeroCrystalCollected,
            //rank = levelData.rank,
            score = scorePoints
        };

        // SaveGame stats
        GameManager.Instance.SaveLevelStats(statsToSave);
    }
    #endregion

    #region DIALOGUE

    public void ShowDialogue()
    {
        stageCompleteScreen.SetActive(false);
        stageSelectScreen.SetActive(false);
        QTEPanel.SetActive(false);
        mainActionHUD.SetActive(false);

        dialoguePanel.SetActive(true);
    }

    public void SetupDialogue(NPCData data)
    {
        nameText.text = data.npcName;
        dialogueIcon.sprite = data.npcIcon;
        dialogueBG.color = data.dialogueColor;
        nameBG.color = data.nameColor;
    }

    public void SetDialogueText(string text, float delay, NPCData data)
    {
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);

        typeCoroutine = StartCoroutine(RevealText(text, delay, data));
    }

    IEnumerator RevealText(string text, float delay, NPCData data)
    {
        isTyping = true;

        dialogueText.text = text;
        dialogueText.ForceMeshUpdate();
        dialogueText.maxVisibleCharacters = 0;

        int totalCharacters = dialogueText.textInfo.characterCount;

        bool playSound = true;

        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
            if (playSound) AudioManager.Instance.PlayDialogueSound(data);
            playSound = !playSound;
        }

        isTyping = false;
    }

    public void SkipType()
    {
        StopCoroutine(typeCoroutine);
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        isTyping = false;
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        SetupHUD();
    }

    #endregion
}
