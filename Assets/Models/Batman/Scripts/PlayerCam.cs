using System;
using UnityEditor;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public float mouseSens = 4f;
    public Animator anime;
    
    public float camYawX, camYawY, convertedYawY, camYawYCut;
    public float camDist, startAngle;
    private float camPosX, camPosY, camPosZ;
    private float camSmooth = 8f;
    private Vector3 playerPos;
    private LayerMask mask;
    private int cameraMode = 0;
    private Grapple grappleScript;
    private PlayerMovement movementScript;
    void Start()
    {
        Cursor.visible = false; //Cursor is invisible
        Cursor.lockState = CursorLockMode.Locked; //Cursor is locked
        startAngle = Mathf.Atan2(-offset.z, -offset.x) * Mathf.Rad2Deg;
        camYawX = startAngle; //Camera starting degree
        camDist = Vector3.Distance(new Vector3(0f,1f,-15f), new Vector3(0.6f,1f,-16.1f)); //Get the distance between player and the camera
        mask = 1 << LayerMask.NameToLayer("Clippable") | 0 << LayerMask.NameToLayer("NotClippable"); //Set rules for which layer can be raycasted
        grappleScript = GameObject.Find("Batman").GetComponent<Grapple>(); //Reference other scripts
        movementScript = GameObject.Find("Batman").GetComponent<PlayerMovement>(); //Reference other scripts
    }

    
    void Update()
    {
        cameraMode = movementScript.camMode; //Get access to the variable camMode in the PlayerMovement script and give the same value to cameraMode
        playerPos = new Vector3(player.position.x, player.position.y + 1f, player.position.z); //Player position
        camYawX -= Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime; //Camera angle degree for Horizontal mouse movement
        camYawX = Mathf.Repeat(camYawX, 360f); //Make sure the camera Yaw for Horizontal mouse movement is within 0 and 360 degrees
        camYawY -= Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime; //Camera angle degree for Vertical mouse movement
        camYawY = Mathf.Clamp(camYawY, -85f, 85f); //Clamp the Camera angle degree for Vertical mouse movement to the range of -90 to 90 degree
        convertedYawY = Mathf.Repeat(camYawY, 360f); //Converts from range -90 to 90 to the range of 0 to 360 for trig calculation
    }

    void LateUpdate()
    {
        if (cameraMode == 0) //Default camera mode
        {
            camPosY = Mathf.Sin(convertedYawY * Mathf.Deg2Rad) * 1.2f + playerPos.y; //Finds the camera position on the Y axis and add offset
            camYawYCut = 1.2f - Mathf.Cos(convertedYawY * Mathf.Deg2Rad) * 1.2f; //Finds the camera position cutoff for X and Z axis
            camPosX = Mathf.Cos(camYawX * Mathf.Deg2Rad) * (camDist-camYawYCut) + playerPos.x; //Finds the camera position on the X axis and add offset
            camPosZ = Mathf.Sin(camYawX * Mathf.Deg2Rad) * (camDist-camYawYCut) + playerPos.z; //Finds the camera position on the Z axis and add offset
        }
        
        else if (cameraMode == 1)
        {
            camPosY = Mathf.Sin(convertedYawY * Mathf.Deg2Rad) * 1.2f + (playerPos.y - 0.6f); //Finds the camera position on the Y axis and add offset
            camYawYCut = 1.2f - Mathf.Cos(convertedYawY * Mathf.Deg2Rad) * 1.2f; //Finds the camera position cutoff for X and Z axis
            camPosX = Mathf.Cos((camYawX - startAngle - 90f) * Mathf.Deg2Rad) * (camDist-camYawYCut) + playerPos.x; //Finds the camera position on the X axis and add offset
            camPosZ = Mathf.Sin((camYawX - startAngle - 90f) * Mathf.Deg2Rad) * (camDist-camYawYCut) + playerPos.z; //Finds the camera position on the Z axis and add offset
        }
        
        else if (cameraMode == 2 || cameraMode == 3 || cameraMode == 5)
        {
            camPosY = Mathf.Sin(convertedYawY * Mathf.Deg2Rad) * 1.2f + playerPos.y; //Finds the camera position on the Y axis and add offset
            camYawYCut = 1.2f - Mathf.Cos(convertedYawY * Mathf.Deg2Rad) * 1.2f; //Finds the camera position cutoff for X and Z axis
            camPosX = Mathf.Cos((camYawX - startAngle - 90f) * Mathf.Deg2Rad) * (camDist-camYawYCut) + playerPos.x; //Finds the camera position on the X axis and add offset
            camPosZ = Mathf.Sin((camYawX - startAngle - 90f) * Mathf.Deg2Rad) * (camDist-camYawYCut) + playerPos.z; //Finds the camera position on the Z axis and add offset
        }
        

        else if (cameraMode == 4 || cameraMode == 6)
        {
            Vector3 tempOffset = player.TransformPoint(new Vector3(0f, 5.5f, -8f)); //Get the local position and translate it to world position
            camPosX = tempOffset.x; //New camera position
            camPosY = tempOffset.y;
            camPosZ = tempOffset.z;
        }
        


        Vector3 targetPos = new Vector3(camPosX, camPosY, camPosZ); //Combining all the co-ordinates to one Vector3 point
        Quaternion targetRot = Quaternion.Euler(camYawY, -(camYawX - startAngle), 0f); //Store the target camera rotation as a quaternion angle
        
        if (Physics.Linecast(playerPos, targetPos, out RaycastHit cameraHit, mask.value)) //Check if the new camera position is inside an obstacle
            targetPos = cameraHit.point + (playerPos - cameraHit.point).normalized * 0.1f; //If it hits, change the camera position to the place where the ray hits so camera don't go into objects like walls
        
        
        if (cameraMode < 3) //If camera mode is 0,1,2
        {
            targetPos = Vector3.Lerp(transform.position, targetPos, camSmooth * Time.deltaTime); //Set the camera smoothing
        }
        
        else if (cameraMode >= 3) //If camera mode is 3,4,5,6
        {
            targetPos = Vector3.Lerp(transform.position, targetPos, 10f * Time.deltaTime); //Set the camera smoothing but faster than the camera smoothing for camera mode 0,1,2
        }
        
        transform.position = targetPos; //Set the camera position to the new camera position
        transform.rotation = targetRot; //Rotate the camera to the Yaw degree
    }
}
