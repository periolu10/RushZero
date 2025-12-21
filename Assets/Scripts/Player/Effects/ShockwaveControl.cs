using System.Collections;
using UnityEngine;

public class ShockwaveControl : MonoBehaviour
{
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private float lifetime;
    private Material material;
    private int propID = Shader.PropertyToID("_StartTimeOffset");

    public void CallShockwave(Transform parent)
    {
        GameObject boostRing = Instantiate(ringPrefab, parent);
        material = boostRing.GetComponent<SpriteRenderer>().material;
        material.SetFloat(propID, Time.time);
        Destroy(boostRing, lifetime);
    }
}
