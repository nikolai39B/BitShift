using UnityEngine;
using System.Collections;

public class PlayerEyeManager : MonoBehaviour
{
    // Eye distance
    public float distanceFromEyeToFaceCenter = 0.1f;
    public float DistanceFromEyeToFaceCenter { get { return distanceFromEyeToFaceCenter; } }

    // Player eye instance
    public GameObject playerEyeIntance;

    // Current camera
    public Camera currCamera;

	void Start()
    {
	
	}
	
	void Update()
    {
        // Get the location of ourself and of the mouse
        Vector3 facePosition = GetComponent<Transform>().position;
        Vector3 mousePosition = currCamera.ScreenToWorldPoint(Input.mousePosition);

        // Get the direction the eye should look
        Vector2 eyeDirection = new Vector2(
            mousePosition.x - facePosition.x,
            mousePosition.y - facePosition.y);
        eyeDirection.Normalize();
        eyeDirection *= distanceFromEyeToFaceCenter;

        // Get the new position for the eye
        Vector3 eyeLocation = new Vector3(
            facePosition.x + eyeDirection.x,
            facePosition.y + eyeDirection.y,
            playerEyeIntance.transform.position.z);
        playerEyeIntance.transform.position = eyeLocation;
	}
}
