using Ink.Runtime;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RushOrb : MonoBehaviour
{
    GameObject orb;
    Animator animator;
    Light2D light;
    Collider2D col;
    SpriteRenderer sprite;

    public float attractRadius = 3f;
    public float attractSpeed = 10f;
    public float collectDistance = 0.5f;

    private Transform player;
    bool radiusEntered = false;

    private void Start()
    {
        orb = transform.GetChild(0).gameObject;
        animator = orb.GetComponent<Animator>();
        light = orb.GetComponentInChildren<Light2D>();
        col = GetComponent<Collider2D>();
        sprite = orb.GetComponent<SpriteRenderer>();

        float rndSpeed = Random.Range(0.8f, 1.1f);
        animator.speed = rndSpeed;

        radiusEntered = false;
    }

    private void Update()
    {
        if (player == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerController != null)
            {
                player = GameManager.Instance.playerController.transform;
            }
            else
            {
                return;
            }
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attractRadius && !radiusEntered)
        {
            radiusEntered = true;
        }

        if (radiusEntered)
        {
            animator.StopPlayback();

            float speed = attractSpeed + distance * 2f;
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }

        if (distance <= collectDistance)
        {
            CollectOrb();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CollectOrb();

            HUDManager hudManager = FindAnyObjectByType<HUDManager>();
            hudManager.AddScore(50);

            collision.gameObject.GetComponent<PlayerController>().AddBoostValue(10);
            hudManager.UpdateBoost(collision.gameObject.GetComponent<PlayerController>().BoostValue, collision.gameObject.GetComponent<PlayerController>().BoostMaxValue);
        }
    }

    public void CollectOrb()
    {
        AudioManager.Instance.PlaySFX("collect_orb");
        Destroy(gameObject);
    }

    // DEBUG COROUTINE
    IEnumerator Respawn()
    {
        col.enabled = false;
        sprite.enabled = false;
        light.enabled = false;

        yield return new WaitForSeconds(3);

        col.enabled = true;
        sprite.enabled = true;
        light.enabled = true;
    }
}
