using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    InputActions controls;

    private void Awake()
    {
        controls = new InputActions();

        controls.UI.AnyKey.performed += ctx => StartGame();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void StartGame()
    {
        AudioManager.Instance.PlaySFX("perfect_parry");
        SceneController.Instance.LoadRoom(Scenes.Scene.ui_MainMenu);
    }
}
