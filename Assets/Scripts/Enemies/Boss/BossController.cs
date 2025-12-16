using Unity.VisualScripting;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Attacks")]
    public BossAttack[] attacks;

    [Header("Setitngs")]
    public float attackInterval = 5f;
    private float nextAttackTime;

    public Rigidbody2D rb;

    private void Awake()
    {
        if (attacks == null || attacks.Length == 0)
        {
            attacks = GetComponents<BossAttack>();
        }

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Time.time > nextAttackTime)
        {
            DoAttack();
            nextAttackTime = Time.time + attackInterval;
        }
    }

    void DoAttack()
    {
        if (attacks.Length == 0) return;

        int index = Random.Range(0, attacks.Length);
        attacks[index].TryAttack();
    }
}
