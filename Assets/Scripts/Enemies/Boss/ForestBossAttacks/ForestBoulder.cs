using System.Collections;
using UnityEngine;

public class ForestBoulder : MonoBehaviour
{
    ParticleSystem particles;
    Collider2D col;
    SpriteRenderer spriteRenderer;
    TrailRenderer trailRenderer;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        particles = GetComponentInChildren<ParticleSystem>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(25, gameObject.transform);
            StartCoroutine(DestroyObject());
        }
        else
        {
            StartCoroutine(DestroyObject());
        }
    }


    IEnumerator DestroyObject()
    {
        particles.Play();
        trailRenderer.emitting = false;
        spriteRenderer.enabled = false;
        col.enabled = false;
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
