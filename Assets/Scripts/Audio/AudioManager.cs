using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public Bus masterBus;
    public Bus musicBus;
    public Bus sfxBus;

    public bool playMusic;
    public bool playSFX;

    private EventInstance musicInstance;
    private string currentMusicPath = "";

    private EventInstance boostLoop;

    private static readonly Dictionary<Scenes.Scene, string> sceneMusicMap = new()
    {
        // UI and MENUS
        { Scenes.Scene.ui_TitleScreen, "Menu" },
        { Scenes.Scene.ui_MainMenu, "Menu" },

        // HUBS
        { Scenes.Scene.hub_Village, "Village" },

        // STAGES
        { Scenes.Scene.stage_Tutorial, "Tutorial" },
        { Scenes.Scene.stage_WildvinePath, "ForestAct1" },
        { Scenes.Scene.stage_ForestPath, "ForestAct2" },

        // BOSS
        { Scenes.Scene.boss_Forest, "ForestBoss" },

        // TEST STAGES
        { Scenes.Scene.stage_TestAction, "ForestAct1" },
        { Scenes.Scene.stage_TestRunner, "PlanetCoreBoss" },
        { Scenes.Scene.room_hub_Test1, "Village" },
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (SceneManager.GetActiveScene().buildIndex == 0) PlaySFX("jingle");
        else 
        { 
            LevelManager levelManager = FindAnyObjectByType<LevelManager>();
            PlayMusic(levelManager.levelData.sceneToLoad);
        }

            masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    #region PUBLIC METHODS
    public void PlayBoostLoop()
    {
        boostLoop = RuntimeManager.CreateInstance("event:/SFX/player_boost_loop");
        boostLoop.start();
    }

    public void StopBoostLoop(bool fadeOut)
    {
        boostLoop.stop(fadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    private void StartMusic(string eventPath)
    {
        string fullPath = "event:/Music/" + eventPath;

        // If music is the same do NOT restart (return)
        if (currentMusicPath == fullPath && musicInstance.isValid()) return;

        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        musicInstance = RuntimeManager.CreateInstance("event:/Music/" + eventPath);
        musicInstance.start();

        currentMusicPath = fullPath;
    }

    public void PlayMusic(Scenes.Scene scene)
    {
        if (!playMusic) return;

        if (sceneMusicMap.TryGetValue(scene, out string musicTrack))
        {
            StartMusic(musicTrack);
        }
    }

    public void PlayMusic(string path)
    {
        if (!playMusic) return;

        StartMusic(path);
    }

    public void SetParameter(string parameter, float value, float duration)
    {
        StartCoroutine(SmoothParam(parameter, value, duration));
    }

    IEnumerator SmoothParam(string parameter, float targetValue, float duration)
    {
        musicInstance.getParameterByName(parameter, out float startValue);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float newValue = Mathf.Lerp(startValue, targetValue, t);

            musicInstance.setParameterByName(parameter, newValue);

            yield return null;
        }

        musicInstance.setParameterByName(parameter, targetValue);
    }

    public void StopMusic(bool fadeOut = true)
    {
        musicInstance.stop(fadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void PauseMusic(bool paused)
    {
        musicInstance.setPaused(paused);
    }

    public void PlaySFX(string sFXName)
    {
        if (!playSFX) return;

        RuntimeManager.PlayOneShot("event:/SFX/" + sFXName);
    }

    public void PlayDialogueSound(NPCData data)
    {
        if (!playSFX) return;

        RuntimeManager.PlayOneShot(data.characterVoiceEvent);
    }
    #endregion

    #region SETTINGS METHODS
    /// <summary>
    /// Set volume from 0 to 100 by increments of 5.
    /// </summary>
    /// <param name="volume"></param>
    public void SetVolume(Bus bus, float volume)
    {
        volume = Mathf.Clamp(volume, 0f, 100f);
        bus.setVolume(volume / 100f);
    }

    public void IncreaseVolume(Bus bus, float step = 5f)
    {
        bus.getVolume(out float current);
        float newVolume = Mathf.Clamp(current * 100f + step, 0f, 100f);
        SetVolume(bus, newVolume);
    }

    public void DecreaseVolume(Bus bus, float step = 5f)
    {
        bus.getVolume(out float current);
        float newVolume = Mathf.Clamp(current * 100f - step, 0f, 100f);
        SetVolume(bus, newVolume);
    }

    public float GetCurrentVolume(Bus bus)
    {
        bus.getVolume(out float current);
        return current * 100f;
    }
    #endregion
}
