using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public LevelData levelToLoad;
    public float cutsceneTime;
    float timer;

    private void Start()
    {
        timer = cutsceneTime;
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (timer <= 0)
        {
            GameManager.Instance.LoadLevel(levelToLoad);
        }
    }
}
