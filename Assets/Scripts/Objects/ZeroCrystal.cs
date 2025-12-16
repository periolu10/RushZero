using UnityEngine;

public class ZeroCrystal : MonoBehaviour
{
    LevelManager levelManager;
    HUDManager hudManager;

    private void Start()
    {
        levelManager = GameObject.FindAnyObjectByType<LevelManager>();
        hudManager = GameObject.FindAnyObjectByType<HUDManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySFX("collect_zerocrystal");

            levelManager.CollectZeroCrystal();

            Destroy(gameObject);
        }
    }
}
