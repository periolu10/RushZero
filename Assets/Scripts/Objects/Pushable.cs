using System.Collections.Generic;
using UnityEngine;

public class Pushable : MonoBehaviour
{
    public List<PhysicsMaterial2D> PhysicsMaterials;

    Collider2D thisCollider;
    Rigidbody2D rb;

    private void Start()
    {
        thisCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Check for Push
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<PlayerController>().IsPushing && collision.gameObject.GetComponent<PlayerController>().IsGrounded)
        {
            // set Normal pushing values
            rb.mass = 5;
            rb.gravityScale = 1;

            // Pushing material 
            thisCollider.sharedMaterial = PhysicsMaterials[0];
            rb.sharedMaterial = PhysicsMaterials[0];
        }
        else
        {
            // Make the object physically unpushable otherwise
            rb.mass = 200;
            rb.gravityScale = 100;

            // No Friction Material (prevents player stucking to object)
            thisCollider.sharedMaterial = PhysicsMaterials[1];
            rb.sharedMaterial = PhysicsMaterials[1];
        }
    }
}
