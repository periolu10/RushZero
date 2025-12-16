using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject gameSettingsMenu;
    public GameObject audioSettingsMenu;

    [Header("Game Options Texts")]
    public TMP_Text enemyHPBarText;

    [Header("Audio Options Texts")]
    public TMP_Text masterVolumeText;
    public TMP_Text musicVolumeText;
    public TMP_Text sfxVolumeText;

    private void Start()
    {
        GameManager.Instance.LoadSettings();

        SwitchMenus(mainMenu, optionsMenu);

        SetupSettings();
    }

    public void SwitchMenus(GameObject menuToOpen, GameObject menuToClose)
    {
        menuToOpen.SetActive(true);
        menuToClose.SetActive(false);
    }

    public void UpdateSettingsText()
    {
        SetupSettings();
    }

    void SetupSettings()
    {
        if (GameManager.Instance.gameSettings.showEnemyHealthBars) enemyHPBarText.text = "Enabled";
        else enemyHPBarText.text = "Disabled";
    }

    public void UpdateVolumeText()
    {
        masterVolumeText.text = Mathf.Round(GameManager.Instance.gameSettings.masterVolume).ToString();
        musicVolumeText.text = Mathf.Round(GameManager.Instance.gameSettings.musicVolume).ToString();
        sfxVolumeText.text = Mathf.Round(GameManager.Instance.gameSettings.sfxVolume).ToString();
    }
}
