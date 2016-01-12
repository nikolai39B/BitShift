using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 5.0f;
    public float maxSpeed = 5.0f;

	void FixedUpdate()
    {
        Rigidbody2D currRigidbody = GetComponent<Rigidbody2D>();

        // Move the player
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            currRigidbody.AddForce(new Vector2(0, movementSpeed));
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            currRigidbody.AddForce(new Vector2(0, -movementSpeed));
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            currRigidbody.AddForce(new Vector2(-movementSpeed, 0));
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            currRigidbody.AddForce(new Vector2(movementSpeed, 0));
        }

        Debug.Log(currRigidbody.velocity.magnitude);

        // Restrict the player's max speed
        if (currRigidbody.velocity.magnitude > maxSpeed)
        {
            currRigidbody.velocity = currRigidbody.velocity.normalized * maxSpeed;
        }
    }
}
