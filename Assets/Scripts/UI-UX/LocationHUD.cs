using UnityEngine;

public class LocationHUD : MonoBehaviour
{
    public bool locationShown = false;

    private void OnEnable()
    {
        if (locationShown)
        {
            gameObject.SetActive(false);
        }
    }
}
