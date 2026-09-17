using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl
{

    // TODO Camera controls should go to their own file eventually
    private Vector3 cameraStartPosition;

    private const float cameraZoomSpeedBase = 300f;
    private const float cameraPanSpeedBase = 200f;

    private const float minCameraY = 10f;

    private float maxCameraY;

    public void Initialize()
    {
        cameraStartPosition = Camera.main.transform.position;
        maxCameraY = cameraStartPosition.y * 2;
    }

    public void ControlCamera()
    {
        float cameraZoomSpeed = cameraZoomSpeedBase;
        float cameraPanSpeed = cameraPanSpeedBase;


        if (Keyboard.current == null) return; // TODO log error?

        var cameraTransform = Camera.main.transform;

        float cameraY = cameraTransform.position.y;
        float cameraZoomLevel = Mathf.InverseLerp(minCameraY, maxCameraY, cameraY);
        float speedFactor = Mathf.Max(cameraZoomLevel, 0.15f);
        cameraZoomSpeed *= speedFactor;
        cameraPanSpeed *= speedFactor;

        if (Keyboard.current.shiftKey.isPressed)
        {
            cameraZoomSpeed = cameraZoomSpeed * 4;
            cameraPanSpeed = cameraPanSpeed * 4;
        }


        if (cameraY > minCameraY && Keyboard.current.eKey.isPressed)
        {
            cameraTransform.position += cameraTransform.forward * cameraZoomSpeed * Time.deltaTime;
        }
        if (cameraY < maxCameraY && Keyboard.current.qKey.isPressed)
        {
            cameraTransform.position -= cameraTransform.forward * cameraZoomSpeed * Time.deltaTime;
        }


        if (Keyboard.current.aKey.isPressed)
        {
            cameraTransform.position -= cameraTransform.right * cameraPanSpeed * Time.deltaTime;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            cameraTransform.position += cameraTransform.right * cameraPanSpeed * Time.deltaTime;
        }

        Vector3 groundForward = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
        if (Keyboard.current.wKey.isPressed)
        {
            cameraTransform.position += groundForward * cameraPanSpeed * Time.deltaTime;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            cameraTransform.position -= groundForward * cameraPanSpeed * Time.deltaTime;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log($"Camera position WAS {cameraTransform.position}");
            cameraTransform.position = cameraStartPosition;
        }

    }
}
