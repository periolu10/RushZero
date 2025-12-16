using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float _startingPosX;
    private float _startingPosY;

    public float AmountOfParallax;
    public Camera virtualCamera;

    private void Start()
    {
        // Get the starting X position of sprite.
        _startingPosX = transform.position.x;
        _startingPosY = transform.position.y;
    }

    private void Update()
    {
        Vector3 camPos = virtualCamera.transform.position;

        float offsetX = camPos.x * AmountOfParallax;
        float offsetY = camPos.y * AmountOfParallax;

        Vector3 newPosition = new Vector3(_startingPosX + offsetX, _startingPosY + offsetY, transform.position.z);
        transform.position = newPosition;
    }
}
