using UnityEngine;

public class DashGhost : MonoBehaviour
{
    public float fadeDuration = 0.3f;

    SpriteRenderer sprite;
    Color color;
    float timer;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        color = Color.deepSkyBlue;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(color.a/2, 0, timer / fadeDuration);
        sprite.color = new Color(color.r, color.g, color.b, alpha);

        if (alpha <= 0.01f)
        {
            Destroy(gameObject);
        }
    }
}
