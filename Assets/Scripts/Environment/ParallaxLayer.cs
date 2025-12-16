using UnityEngine;

public class ParallaxHorizontalRepeat : MonoBehaviour
{
    [Range(0f, 1f)] public float parallaxFactor = 0.5f;
    [Range(0f, 1f)] public float verticalFollowStrength = 0.95f; // 1.0 = follow camera exactly
    public float tileWidth = 16f;

    private Transform cam;
    private Vector3 lastCamPos;
    private Transform[] tiles;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;

        tiles = new Transform[transform.childCount];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = transform.GetChild(i);
        }
    }

    void FixedUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;

        // Horizontal parallax
        float moveX = delta.x * parallaxFactor;

        // Vertical follow (nearly identical to cam Y movement)
        float moveY = delta.y * verticalFollowStrength;

        transform.position += new Vector3(moveX, moveY, 0f);
        lastCamPos = cam.position;

        foreach (Transform tile in tiles)
        {
            float diffX = cam.position.x - tile.position.x;

            if (Mathf.Abs(diffX) >= tileWidth * 1.5f)
            {
                float direction = Mathf.Sign(diffX);
                float newX = tile.position.x + tileWidth * tiles.Length * direction;
                tile.position = new Vector3(newX, tile.position.y, tile.position.z);
            }
        }
    }
}
