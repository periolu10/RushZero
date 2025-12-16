using Unity.Cinemachine;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    CinemachineCamera cam;

    [Header("Zoom")]
    public float maxHeight;
    public float currentHeight;
    public float maxZoom;
    public float minZoom;

    [Header("Position")]
    public float minPos;
    public float maxPos;

    bool isActionStage;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<CinemachineCamera>();
        GameManager.Instance.currentCamera = cam;
        cam.Prioritize();

        LevelManager levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager.levelData.stageType == LevelData.StageType.Action)
        {
            isActionStage = true;
        }
        else
        {
            isActionStage = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActionStage) return;

        currentHeight = this.gameObject.transform.position.y;
        float zoomFactor = currentHeight / maxHeight;
        float screenPosition = Mathf.Lerp(minPos, maxPos, zoomFactor);

        // Zoom out cam relative to height
        cam.Lens.OrthographicSize = Mathf.Lerp(minZoom, maxZoom, zoomFactor);
        cam.GetComponent<CinemachinePositionComposer>().Composition.ScreenPosition = new Vector2(-0.05f, screenPosition);
    }
}
