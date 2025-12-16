using UnityEngine;

public abstract class BossAttack : MonoBehaviour
{
    [Header("Name")]
    public string attackName;

    [Header("Cooldowns")]
    public float duration = 5f;
    public float cooldown = 2f;

    public BossController boss;
    float lastUsedTime = -Mathf.Infinity;

    public bool TryAttack()
    {
        if (Time.time > lastUsedTime + cooldown)
        {
            Attack();
            lastUsedTime = Time.time;
            return true;
        }
        return false;
    }

    protected abstract void Attack();
}
