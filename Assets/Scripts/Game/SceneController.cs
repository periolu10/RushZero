using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    [Header("Loading Screen Settings")]
    public GameObject loadingScreen;
    public float loadFadeDuration = 0.5f;

    [Header("Loading Screen References")]
    public TMP_Text stageNameText;
    public TMP_Text goalTipText;
    public Image loadingImage;

    bool sceneLoadStarted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        loadingScreen.SetActive(false);
    }

    /// <summary>
    /// Use this to load between rooms.
    /// </summary>
    /// <param name="sceneToLoad"></param>
    public void LoadRoom(Scenes.Scene sceneToLoad)
    {
        if (sceneLoadStarted) return;

        sceneLoadStarted = true;
        StartCoroutine(FadeAndSwitchScenes(sceneToLoad));

        if (GameManager.Instance.gameState != GameManager.GameState.Menu) AudioManager.Instance.playSFX = false;
        AudioManager.Instance.PlayMusic(sceneToLoad);
    }

    /// <summary>
    /// Use this to load between main scenes (Ui to Game / Hub to Stage)
    /// </summary>
    /// <param name="sceneToLoad"></param>
    public void LoadScene(Scenes.Scene sceneToLoad)
    {
        if (sceneLoadStarted) return;

        AudioManager.Instance.playSFX = false;
        sceneLoadStarted = true;
        StartCoroutine(LoadScreenAndSwitchScenes(sceneToLoad));
    }

    private IEnumerator FadeAndSwitchScenes(Scenes.Scene sceneToLoad)
    {
        if (GameManager.Instance.playerController != null) GameManager.Instance.playerController.DisableAllControl();
        yield return StartCoroutine(FadeOut(fadeCanvasGroup, fadeDuration));

        int sceneIndex = (int)sceneToLoad;

        AsyncOperation asyncload = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncload.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(FadeIn(fadeCanvasGroup, fadeDuration));
        if (GameManager.Instance.playerController != null) GameManager.Instance.playerController.EnableControls();
    }

    private IEnumerator LoadScreenAndSwitchScenes(Scenes.Scene sceneToLoad)
    {
        AudioManager.Instance.PlayMusic("LoadScreen");

        yield return StartCoroutine(LoadScreenIn());

        int sceneIndex = (int)sceneToLoad;

        AsyncOperation asyncload = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncload.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(LoadScreenOut());

        AudioManager.Instance.PlayMusic(sceneToLoad);
    }

    private IEnumerator LoadScreenIn()
    {
        loadingScreen.SetActive(true);
        loadingScreen.GetComponent<Animator>().Play("Screen_In");

        yield return new WaitForSeconds(3f);
    }

    private IEnumerator LoadScreenOut()
    {
        loadingScreen.GetComponent<Animator>().Play("Screen_Out");
        yield return new WaitForSeconds(0.2f);

        sceneLoadStarted = false;
        loadingScreen.SetActive(false);

        AudioManager.Instance.playSFX = true;
    }

    public IEnumerator FadeIn(CanvasGroup canvasGroup, float fateDur)
    {
        float t = fadeDuration;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fateDur);
            yield return null;
        }

        sceneLoadStarted = false;
        canvasGroup.blocksRaycasts = false;

        AudioManager.Instance.playSFX = true;
    }

    public IEnumerator FadeOut(CanvasGroup canvasGroup, float fadeDur)
    {
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDur);
            yield return null;
        }

        canvasGroup.blocksRaycasts = true;
    }
}
