using Ink.Runtime;
using System.Collections;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    HUDManager hudManager;
    private Story currentStory;

    bool dialoguePlaying = false;

    public bool canOpenDialogue = true;

    private void Start()
    {
        hudManager = FindAnyObjectByType<HUDManager>();
        dialoguePlaying = false;
        canOpenDialogue = true;
    }

    private void Update()
    {
        if (!dialoguePlaying)
        {
            return;
        }

        if (GameManager.Instance.playerController.confirmPressed)
        {          
            if (hudManager.isTyping)
            {
                hudManager.SkipType();
            }
            else
            {
                ContinueStory();
            }

            GameManager.Instance.playerController.confirmPressed = false;
        }
    }

    public void EnterDialogue(TextAsset inkJSON, NPCData data)
    {
        GameManager.Instance.playerController.DisableAllControl();

        hudManager.SetupDialogue(data);

        currentStory = new Story(inkJSON.text);
        dialoguePlaying = true;
        hudManager.ShowDialogue();

        ContinueStory();
    }

    private void ExitDialogue()
    {
        dialoguePlaying = false;
        hudManager.HideDialogue();
        GameManager.Instance.MainCamPrioritize();
        StartCoroutine(DialogueCooldown());
    }

    void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            hudManager.SetDialogueText(currentStory.Continue(), 0.05f, GameManager.Instance.playerController.CurrentDialogueTrigger.NPCData);
        }
        else
        {
            ExitDialogue();
        }
    }

    IEnumerator DialogueCooldown()
    {
        yield return new WaitForSeconds(1);
        canOpenDialogue = true;
        GameManager.Instance.playerController.EnableControls();
        GameManager.Instance.playerController.CurrentDialogueTrigger.InstantiateIndicator();
    }
}
