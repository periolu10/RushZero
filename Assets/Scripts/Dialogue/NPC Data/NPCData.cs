using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "Scriptable Objects/NPCData")]
public class NPCData : ScriptableObject
{
    public string npcName;
    public Sprite npcIcon;
    public Color nameColor;
    public Color dialogueColor;

    [Header("Sound")]
    public EventReference characterVoiceEvent;

}
