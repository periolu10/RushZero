using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public enum EnemyType
    {
        Minion,
        Normal,
        Elite,
        Boss,
        Special, // May be used for the final boss.
        Dummy
    }

    [Header("Enemy Type")]
    public EnemyType enemyType;

    [Header("Enemy Settings")]
    public string enemyName;
    public float enemyMaxHealth;
    public float enemyDamage;
}
