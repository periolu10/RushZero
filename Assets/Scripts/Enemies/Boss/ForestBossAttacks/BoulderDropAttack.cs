using System.Collections;
using UnityEngine;

public class BoulderDropAttack : BossAttack
{
    public GameObject boulderPrefap;
    public Transform[] spawnPoints;

    [Header("Boss Move Points")]
    public Transform restPosition;
    public Transform returnPosition;

    protected override void Attack()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator SpawnBoulder(Transform t)
    {
        SpriteRenderer line = t.GetComponentInChildren<SpriteRenderer>();

        for (int i = 0; i < 3; i++)
        {
            line.enabled = true;
            yield return new WaitForSeconds(0.05f);
            line.enabled = false;
            yield return new WaitForSeconds(0.05f);
        }

        Instantiate(boulderPrefap, t.position, Quaternion.identity);
    }

    IEnumerator AttackRoutine()
    {
        boss.rb.AddForce(new Vector2(0, 100), ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.5f);

        boss.rb.linearVelocity = Vector2.zero;
        boss.transform.position = restPosition.position;
        boss.rb.gravityScale = 0f;

        AudioManager.Instance.PlaySFX("warning_attack");

        foreach (Transform t in spawnPoints)
        {
            StartCoroutine(SpawnBoulder(t));
        }

        yield return new WaitForSeconds(3f);

        boss.transform.position = returnPosition.position;
        boss.rb.gravityScale = 6f;
    }
}
