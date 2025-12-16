using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("The scene that corresponds to this data.")]
    public Scenes.Scene sceneToLoad;

    public enum ControlType
    {
        Hub,
        Action,
        Runner,
        Cutscene
    }

    [Header("Player Control Scheme")]
    public ControlType controlType;

    public enum StageType
    {
        Hub,
        Action,
        Boss,
        Cutscene
    }

    [Header("Stage Type/Mode")]
    public StageType stageType;

    [Header("Level/Stage Name")]
    public string levelName;

    [Header("Loading Screen")]
    public string loadingSubtitle;
    public Sprite loadingImage;

    [Header("Unlockable Triggers")]
    public bool hasGallery;
    public bool hasZeroCrystal;
}
