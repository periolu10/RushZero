using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CamSwitch : MonoBehaviour
{
    [SerializeField] CinemachineCamera switchCam;
    CinemachinePositionComposer composer;

    private void Start()
    {
        switchCam.gameObject.SetActive(false);
        composer = switchCam.GetComponent<CinemachinePositionComposer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SwitchCam(switchCam);
        }
    }

    void SwitchCam(CinemachineCamera camToPrio)
    {
        camToPrio.gameObject.SetActive(true);
        camToPrio.Prioritize();

        StartCoroutine(ResetSize());
    }

    IEnumerator ResetSize()
    {
        yield return new WaitForSeconds(1f);

        composer.Composition.DeadZone.Size.Set(0, 0.2f);
        Destroy(gameObject);
    }
}
