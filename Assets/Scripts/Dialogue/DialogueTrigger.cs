using Unity.Cinemachine;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("NPC Data")]
    [SerializeField] NPCData npcData;    

    [Header("Visual Cue")]
    public GameObject indicatorPrefab;
    GameObject indicatorInstance;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    [Header("NPC Cam")]
    [SerializeField] private CinemachineCamera npcCam;

    DialogueManager dialogueManager;

    public NPCData NPCData => npcData;

    private void Start()
    {
        dialogueManager = FindAnyObjectByType<DialogueManager>();
    }

    public void BeginDialogue()
    {
        dialogueManager.EnterDialogue(inkJSON, npcData);
        Destroy(indicatorInstance);
        npcCam.Prioritize();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            InstantiateIndicator();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Destroy(indicatorInstance);
    }

    public void InstantiateIndicator()
    {
        indicatorInstance = Instantiate(indicatorPrefab, new Vector2(transform.position.x, transform.position.y + 2), Quaternion.identity);
    }
}
