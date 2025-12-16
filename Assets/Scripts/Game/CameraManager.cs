using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    LevelData.ControlType controlType;
    LevelManager levelManager;

    CinemachineCamera cam;

    [Header("Target Reference")]
    [SerializeField] Transform cameraTrack;

    private void Start()
    {
        levelManager = FindAnyObjectByType<LevelManager>();
        controlType = levelManager.levelData.controlType;

        cam = GetComponentInChildren<CinemachineCamera>();

        SetupCameraTracking();
    }

    void SetupCameraTracking()
    {
        if (controlType == LevelData.ControlType.Hub || controlType == LevelData.ControlType.Action)
        {
            cam.Follow = FindAnyObjectByType<PlayerController>().transform;
        }
        else if (controlType == LevelData.ControlType.Runner)
        {
            cam.Follow = cameraTrack;
        }
    }
}
