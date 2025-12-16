using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QTEManager : MonoBehaviour
{
    public float timeLimit = 1.5f;
    public int minSequenceLength = 3;
    public int maxSequenceLength = 5;

    private InputAction[] qteActions;
    private InputActions controls;
    private bool qteActive;

    private QTEEvent activeEvent;
    private HUDManager hudManager;
    private PlayerController playerController;

    private List<InputAction> sequence = new List<InputAction>();
    private int currentIndex = 0;
    private Coroutine stepCoroutine;
    private Coroutine timeScaleCoroutine;

    private void Awake()
    {
        controls = new InputActions();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        hudManager = FindAnyObjectByType<HUDManager>();
        playerController = FindAnyObjectByType<PlayerController>();
        qteActions = new InputAction[]
        {
            controls.QTE.Up,
            controls.QTE.Down,
            controls.QTE.Left,
            controls.QTE.Right
        };
    }

    public void StartQTE(QTEEvent qteEvent)
    {
        if (qteActive) return;

        if (timeScaleCoroutine != null) StopCoroutine(timeScaleCoroutine);
        timeScaleCoroutine = StartCoroutine(SmoothTimeScale(1f, 0.1f, 0.5f));
        playerController.DisableAllControl();
        playerController.SpriteFlipX();

        activeEvent = qteEvent;
        qteActive = true;

        int length = Random.Range(minSequenceLength, maxSequenceLength + 1);
        sequence.Clear();
        for (int i = 0; i < length; i++)
            sequence.Add(qteActions[Random.Range(0, qteActions.Length)]);

        currentIndex = 0;
        ShowCurrentPrompt();

        foreach (var action in qteActions)
            action.performed += OnInput;



        stepCoroutine = StartCoroutine(QTEStepCoroutine());
    }

    private void ShowCurrentPrompt()
    {
        if (currentIndex < sequence.Count)
            hudManager.ShowQTE(sequence[currentIndex].name);
    }

    private void OnInput(InputAction.CallbackContext ctx)
    {
        if (!qteActive) return;

        Animator promptAnim = hudManager.prompt.GetComponent<Animator>();
        promptAnim.updateMode = AnimatorUpdateMode.UnscaledTime;

        bool correct = ctx.action == sequence[currentIndex];

        if (correct)
        {
            AudioManager.Instance.PlaySFX("qte_hit");
            currentIndex++;
            if (stepCoroutine != null) StopCoroutine(stepCoroutine);

            StartCoroutine(PlayHitThenIdle(promptAnim));

            if (currentIndex >= sequence.Count)
                StartCoroutine(EndQTEAfterAnimation(true));
            else
            {
                ShowCurrentPrompt();
                stepCoroutine = StartCoroutine(QTEStepCoroutine());
            }
        }
        else
        {
            promptAnim.Play("qte_Miss");
            AudioManager.Instance.PlaySFX("ui_cancel");

            if (stepCoroutine != null)
                StopCoroutine(stepCoroutine);

            StartCoroutine(EndQTEAfterAnimation(false));
        }
    }

    private IEnumerator QTEStepCoroutine()
    {
        float timer = 0f;
        while (timer < timeLimit && qteActive && currentIndex < sequence.Count)
        {
            timer += Time.unscaledDeltaTime;
            hudManager.UpdateQTEFill(timer / timeLimit);
            yield return null;
        }

        if (qteActive && currentIndex < sequence.Count)
            StartCoroutine(EndQTEAfterAnimation(false));
    }

    private IEnumerator EndQTEAfterAnimation(bool success)
    {
        qteActive = false;

        foreach (var action in qteActions)
            action.performed -= OnInput;

        if (timeScaleCoroutine != null)
        {
            StopCoroutine(timeScaleCoroutine);
            timeScaleCoroutine = null;
        }

        yield return StartCoroutine(SmoothTimeScale(Time.timeScale, 1f, 0.2f));

        yield return new WaitForSecondsRealtime(0.15f);

        hudManager.HideQTE();
        playerController.EnableControls();

        if (success)
        {
            activeEvent.OnSuccess();
            AudioManager.Instance.PlaySFX("success");
            hudManager.AddScore(1000);
        }
        else
        {
            activeEvent.OnFail();
            AudioManager.Instance.PlaySFX("ui_cancel");
        }
    }

    IEnumerator PlayHitThenIdle(Animator promptAnim)
    {
        promptAnim.Play("qte_Hit");
        yield return new WaitForSecondsRealtime(0.15f);
        promptAnim.Play("qte_Idle");
    }

    private IEnumerator SmoothTimeScale(float from, float to, float duration)
    {
        float elapsed = 0f;
        Time.timeScale = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        Time.timeScale = to;
    }
}
