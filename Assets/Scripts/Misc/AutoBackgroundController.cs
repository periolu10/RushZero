using UnityEngine;

public class ScrollBackgroundController : MonoBehaviour
{
    public Transform[] backgroundLayers;
    public float[] parallaxSpeeds; // Smaller = farther away, slower
    float tileWidth;

    private void Start()
    {

    }

    void Update()
    {
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            tileWidth = backgroundLayers[i].GetComponent<SpriteRenderer>().bounds.size.x;

            // Move each layer left to simulate running right
            backgroundLayers[i].position += Vector3.left * parallaxSpeeds[i] * Time.deltaTime;

            // Optional: Reset position if off-screen (for looping)
            if (backgroundLayers[i].position.x <= -tileWidth)
            {
                backgroundLayers[i].position += Vector3.right * tileWidth * 2;
            }
        }
    }
}
