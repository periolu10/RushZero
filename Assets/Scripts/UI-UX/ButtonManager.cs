using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    bool saveCreated = false;

    public List<Button> buttons;
    public Button selectedButton;
    public int buttonIndex;
    float inputY;
    float inputX;
    bool buttonLock;

    InputActions controls;

    public MainMenuManager mainMenuManager;

    public LevelData firstLevelData;

    private void Start()
    {
        ApplyAllVolumes();

        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => OnButtonConfirm(button));
        }

        selectedButton = buttons[0];
        buttonIndex = 0;
        OnButtonSelect();
    }

    private void Awake()
    {
        controls = new InputActions();

        controls.UI.NavMenu.performed += ctx => inputY = ctx.ReadValue<Vector2>().y;
        controls.UI.NavMenu.canceled += ctx => inputY = 0;

        controls.UI.NavMenuX.performed += ctx => inputX = ctx.ReadValue<Vector2>().x;
        controls.UI.NavMenuX.canceled += ctx => inputX = 0;

        controls.UI.Submit.performed += ctx => OnButtonConfirm(selectedButton);

        controls.UI.Cancel.performed += ctx =>
        {
            if (buttonLock)
            {
                ButtonLockCancel();
            }
        };
    }

    void OnEnable()
    {
        if (File.Exists(SaveSystem.GameSavePath)) saveCreated = true;
        else saveCreated = false;
        OnButtonSelect();
        controls.Enable();
    }
    void OnDisable() => controls.Disable();

    private void Update()
    {
        if (inputY != 0 && !buttonLock)
        {
            if (inputY >= 0.1f)
            {
                if (selectedButton != buttons[0] && buttonIndex != 0)
                {
                    buttonIndex--;

                    // Check if continue button is active
                    if (buttons[buttonIndex].gameObject.name == "ContinueButton" && !saveCreated)
                    {
                        // If not, skip to new game button.
                        buttonIndex--;
                    }

                    selectedButton = buttons[buttonIndex];
                }
                else
                {
                    buttonIndex = buttons.Count - 1;
                    selectedButton = buttons[buttonIndex];
                }
            }
            else if (inputY <= -0.1f && !buttonLock)
            {
                if (selectedButton != buttons[buttons.Count - 1] && buttonIndex != buttons.Count - 1)
                {
                    buttonIndex++;

                    // Check if continue button is active
                    if (buttons[buttonIndex].gameObject.name == "ContinueButton" && !saveCreated)
                    {
                        // If not, skip to options button.
                        buttonIndex++;
                    }

                    selectedButton = buttons[buttonIndex];
                }
                else
                {
                    buttonIndex = 0;
                    selectedButton = buttons[0];
                }
            }

            inputY = 0;
            OnButtonSelect();
            AudioManager.Instance.PlaySFX("ui_menu_select");
        }
        else if (inputX != 0 && buttonLock)
        {
            if (inputX >= 0.1f)
            {
                IncreaseSelectedVolume();
            }
            else if (inputX <= 0.1f)
            {
                DecreaseSelectedVolume();
            }

            inputX = 0;
            ApplyAllVolumes();
        }
    }


    void OnButtonSelect()
    {
        foreach (var button in buttons)
        {
            // Change selected button color.
            if (button == selectedButton)
            {
                button.gameObject.GetComponentInChildren<TMP_Text>().color = Color.yellow;
                button.GetComponent<Animator>().Play("menu_button_selected");
            }
            else if (button.gameObject.name == "ContinueButton" && !saveCreated)
            {
                button.gameObject.GetComponentInChildren<TMP_Text>().color = Color.gray;
                button.GetComponent<Animator>().Play("menu_button_idle");
            }
            else
            {
                button.gameObject.GetComponentInChildren<TMP_Text>().color = Color.white;
                button.GetComponent<Animator>().Play("menu_button_idle");
            }
        }
    }

    void OnButtonConfirm(Button confirm)
    {
        Debug.Log("Clicked: " + confirm.name);

        switch (confirm.name)
        {
            #region MAIN MENU
            case "NewGameButton":
                NewGame();
                break;
            case "ContinueButton":
                LoadGame();
                break;
            case "OptionsButton":
                OptionsButton();
                break;
            case "QuitButton":
                QuitGame();
                break;
            #endregion
            #region MAIN SETTINGS MENU
            case "GameSettingsButton":
                GameSettingsButton();
                break;
            case "AudioSettingsButton":
                AudioSettingsButton();
                break;
            case "BackToMenuButton":
                BackToMenuButton();
                break;
            #endregion
            #region GAME OPTIONS MENU
            case "ShowEnemyHPButton":
                ShowEnemyHPButton();
                break;
            case "DeleteSaveButton":
                DeleteSaveButton();
                break;
            case "BackToSettingsButtonGame":
                BackToSettingsButtonGame();
                break;
            #endregion
            #region AUDIO OPTIONS MENU
            case "MasterVolume":
                MasterVolumeButton();
                break;
            case "MusicVolume":
                MusicVolumeButton();
                break;
            case "SFXVolume":
                SFXVolumeButton();
                break;
            case "BackToSettingsButtonAudio":
                BackToSettingsButtonAudio();
                break;
                #endregion
        }
    }

    void ButtonLockCancel()
    {
        buttonLock = false;
        AudioManager.Instance.PlaySFX("ui_cancel");
        selectedButton.transform.GetChild(2).gameObject.SetActive(false);
    }

    #region MAIN MENU
    void NewGame()
    {
        Debug.Log("Starting game...");
        AudioManager.Instance.PlaySFX("ui_confirm");

        SaveSystem.DeleteSave();
        GameManager.Instance.LoadGame();

        GameManager.Instance.LoadCutscene(firstLevelData);
    }

    void LoadGame()
    {
        Debug.Log("Loading game...");
        AudioManager.Instance.PlaySFX("ui_confirm");

        GameManager.Instance.LoadGame();

        GameManager.Instance.LoadLevel(GameManager.Instance.gameData.savedHub);
    }

    void OptionsButton()
    {
        Debug.Log("Options Menu");
        AudioManager.Instance.PlaySFX("ui_confirm");

        mainMenuManager.SwitchMenus(mainMenuManager.optionsMenu, mainMenuManager.mainMenu);
    }

    void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    #endregion
    #region MAIN SETTINGS MENU
    void GameSettingsButton()
    {
        AudioManager.Instance.PlaySFX("ui_confirm");
        mainMenuManager.SwitchMenus(mainMenuManager.gameSettingsMenu, mainMenuManager.optionsMenu);
    }
    void AudioSettingsButton()
    {
        AudioManager.Instance.PlaySFX("ui_confirm");
        mainMenuManager.SwitchMenus(mainMenuManager.audioSettingsMenu, mainMenuManager.optionsMenu);
    }
    void BackToMenuButton()
    {
        AudioManager.Instance.PlaySFX("ui_cancel");

        mainMenuManager.SwitchMenus(mainMenuManager.mainMenu, mainMenuManager.optionsMenu);
    }
    #endregion
    #region GAME OPTIONS MENU
    void ShowEnemyHPButton()
    {
        AudioManager.Instance.PlaySFX("ui_confirm");

        GameManager.Instance.ToggleSetting(ref GameManager.Instance.gameSettings.showEnemyHealthBars);

        mainMenuManager.UpdateSettingsText();
    }
    void DeleteSaveButton()
    {
        AudioManager.Instance.PlaySFX("ui_confirm");

        SaveSystem.DeleteSave();
    }
    void BackToSettingsButtonGame()
    {
        AudioManager.Instance.PlaySFX("ui_cancel");

        mainMenuManager.SwitchMenus(mainMenuManager.optionsMenu, mainMenuManager.gameSettingsMenu);
    }
    #endregion
    #region AUDIO SETTING MENU
    void MasterVolumeButton()
    {
        if (buttonLock) return;

        selectedButton.transform.GetChild(2).gameObject.SetActive(true);

        AudioManager.Instance.PlaySFX("ui_confirm");
        buttonLock = true;
    }
    void MusicVolumeButton()
    {
        if (buttonLock) return;

        selectedButton.transform.GetChild(2).gameObject.SetActive(true);

        AudioManager.Instance.PlaySFX("ui_confirm");
        buttonLock = true;
    }
    void SFXVolumeButton()
    {
        if (buttonLock) return;

        selectedButton.transform.GetChild(2).gameObject.SetActive(true);

        AudioManager.Instance.PlaySFX("ui_confirm");
        buttonLock = true;
    }
    void BackToSettingsButtonAudio()
    {
        AudioManager.Instance.PlaySFX("ui_cancel");

        mainMenuManager.SwitchMenus(mainMenuManager.optionsMenu, mainMenuManager.audioSettingsMenu);
    }

    public void IncreaseSelectedVolume()
    {
        if (selectedButton.gameObject.name == "MasterVolume")
            GameManager.Instance.gameSettings.masterVolume = Mathf.Clamp(GameManager.Instance.gameSettings.masterVolume + 5f, 0f, 100f);
        else if (selectedButton.gameObject.name == "MusicVolume")
            GameManager.Instance.gameSettings.musicVolume = Mathf.Clamp(GameManager.Instance.gameSettings.musicVolume + 5f, 0f, 100f);
        else if (selectedButton.gameObject.name == "SFXVolume")
            GameManager.Instance.gameSettings.sfxVolume = Mathf.Clamp(GameManager.Instance.gameSettings.sfxVolume + 5f, 0f, 100f);

        AudioManager.Instance.PlaySFX("ui_menu_select");
        ApplyAllVolumes();
    }

    public void DecreaseSelectedVolume()
    {
        if (selectedButton.gameObject.name == "MasterVolume")
            GameManager.Instance.gameSettings.masterVolume = Mathf.Clamp(GameManager.Instance.gameSettings.masterVolume - 5f, 0f, 100f);
        else if (selectedButton.gameObject.name == "MusicVolume")
            GameManager.Instance.gameSettings.musicVolume = Mathf.Clamp(GameManager.Instance.gameSettings.musicVolume - 5f, 0f, 100f);
        else if (selectedButton.gameObject.name == "SFXVolume")
            GameManager.Instance.gameSettings.sfxVolume = Mathf.Clamp(GameManager.Instance.gameSettings.sfxVolume - 5f, 0f, 100f);

        AudioManager.Instance.PlaySFX("ui_menu_select");
        ApplyAllVolumes();
    }

    void ApplyAllVolumes()
    {
        AudioManager.Instance.SetVolume(AudioManager.Instance.masterBus, GameManager.Instance.gameSettings.masterVolume);
        AudioManager.Instance.SetVolume(AudioManager.Instance.musicBus, GameManager.Instance.gameSettings.musicVolume);
        AudioManager.Instance.SetVolume(AudioManager.Instance.sfxBus, GameManager.Instance.gameSettings.sfxVolume);

        mainMenuManager.UpdateVolumeText();
        GameManager.Instance.SaveSettings();
    }
    #endregion
}
