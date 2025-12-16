using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class QTEEvent : MonoBehaviour
{
    public Transform successSpawnPoint;
    public Transform failSpawnPoint;
    private GameObject player;
    public Vector2 launchForce;
    public float successForce;
    QTEManager manager;

    PlayerController pc;
    CinemachineImpulseSource impulse;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        pc = player.GetComponent<PlayerController>();
        manager = GetComponent<QTEManager>();
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            pc.PlayerRB.linearVelocity = Vector2.zero;
            pc.PlayerRB.AddForce(launchForce);
            manager.StartQTE(this);
            CinemachineImpulseManager.Instance.Clear();
        }
    }

    public void OnSuccess()
    {
        StartCoroutine(SuccessLaunch(successSpawnPoint.position, successForce));
    }

    public void OnFail()
    {
        LaunchPlayerTowards(failSpawnPoint.position, 1f);
    }

    IEnumerator SuccessLaunch(Vector3 targetPos, float burstForce)
    {
        Rigidbody2D rb = pc.PlayerRB;
        pc.DisableAllControl();

        AudioManager.Instance.PlaySFX("qte_charge");
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        pc.chargeParticles.Play();

        yield return new WaitForSeconds(0.1f);

        player.GetComponentInChildren<Animator>().Play("Charge", 0, 0);

        yield return new WaitForSeconds(0.3f);

        pc.trail.emitting = true;
        impulse.GenerateImpulse(2);
        AudioManager.Instance.PlaySFX("qte_burst");

        rb.gravityScale = 3f;
        Vector2 direction = (targetPos - player.transform.position).normalized;
        rb.AddForce(direction * burstForce, ForceMode2D.Impulse);

        StartCoroutine(ReenableControlAfter(0.5f));
    }

    public void LaunchPlayerTowards(Vector3 targetPos, float burstForce)
    {
        Rigidbody2D rb = pc.PlayerRB;
        pc.DisableAllControl();

        rb.linearVelocity = Vector2.zero;
        Vector2 direction = (targetPos - player.transform.position).normalized;
        rb.AddForce(direction * burstForce, ForceMode2D.Impulse);

        StartCoroutine(ReenableControlAfter(0.5f));
    }

    private IEnumerator ReenableControlAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        pc.EnableControls();
        pc.trail.emitting = false;
    }
}
