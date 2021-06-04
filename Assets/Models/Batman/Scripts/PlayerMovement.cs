using System;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Access to components
    public Transform cam; //Transformation info of the camera
    public Animator anime; //Get access to the Animator Component
    public CharacterController CC; //Get access to the CharacterController Component
    public int camMode = 0;
    public float gravity;
    public Transform BatmanBody;
    

    private float rot;
    private float moveLR = 0f;
    private float moveFB = 0f;
    private Vector3 moveDirection;
    private Grapple GGScript;

    
    
    //Time.deltaTime is used to even out the framerate inconsistency

    void Start()
    {
        GGScript = GameObject.Find("Batman").GetComponent<Grapple>(); //Reference the grapple script
    }

    public bool grounded() //Check if Batman is on the ground with raycasting
    {
        bool leftFoot =  Physics.Raycast(BatmanBody.TransformPoint(new Vector3(-2f,0f,0f)), -Vector3.up, 0.5f);
        bool rightFoot =  Physics.Raycast(BatmanBody.TransformPoint(new Vector3(2f,0f,0f)), -Vector3.up, 0.5f);
        bool backSpot = Physics.Raycast(BatmanBody.TransformPoint(new Vector3(0f,0f,-2f)), -Vector3.up, 0.5f);
        bool forwardSpot = Physics.Raycast(BatmanBody.TransformPoint(new Vector3(0f,0f,2f)), -Vector3.up, 0.5f);
        bool centreSpot = Physics.Raycast(BatmanBody.TransformPoint(Vector3.zero), -Vector3.up, 0.5f);
        
        if (leftFoot || rightFoot || backSpot || forwardSpot || centreSpot)
            return true;
            
        
        return false;
    }
    
    public bool movingAnime() //If Batman is moving forward
    {
        if (anime.GetCurrentAnimatorStateInfo(0).IsName("Walk.Walk_Forward"))
        {
            return true;
        }

        if (anime.GetCurrentAnimatorStateInfo(0).IsName("Run.Run_Forward"))
        {
            return true;
        }

        if (anime.GetCurrentAnimatorStateInfo(0).IsName("Crouch.Run_Stealth_128"))
        {
            return true;
        }
        
        return false;
    }

    public float angleDifference() // Calculates what angle difference is between Batman facing direction and the moving direction
    {
        Vector3 playerDir = transform.forward; // Player facing direction
        Vector3 cameraDir = cam.forward; // Camera facing direction
        cameraDir.y = 0f; // Ignore the y camera rotation
        Vector3 moveDir = new Vector3(moveLR, 0, moveFB); // Movement direction
        
        
        Quaternion cameraToWorldDiff = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(cameraDir)); // Calculating rotation from camera to the z world axis
        Vector3 moveTargetDir = cameraToWorldDiff * moveDir; // Apply the rotation calculated on top of the moving direction
        Vector3 sign = Vector3.Cross(moveTargetDir, playerDir); // Find whether final angle is positive or negative by finding the cross product of the two vector angle
        float angleDiff = Vector3.Angle(playerDir, moveTargetDir) * (sign.y < 0 ? 1f : -1f); // Calculate the angle difference by finding the difference in angle and apply the sign
        
        return angleDiff; // Returns the calculated angle difference
    }

    void FixedUpdate()
    {

    }

    void Update()
    {
        if (anime.GetBool("Crouching"))
        {
            CC.center = new Vector3(0f,1.6f,0f);
            CC.height = 2.8f;
        }

        else
        {
            CC.center = new Vector3(0f,2.7f,0f);
            CC.height = 5f;
        }
        


        // Reset animation
        if (anime.GetInteger("Speed") != 0) //If Batman is in movement animation but no movement keys are pressed, reset it to idle
        {
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) &&
                !Input.GetKey(KeyCode.D) && anime.GetInteger("Grapple") == 0)
            {
                anime.SetInteger("Speed", 0);
            }
        }

        //Set camera mode back to zero if player is not running
        if (camMode == 2 && anime.GetInteger("Speed") != 2)
            camMode = 0;
        
        
        //Movement Key Pressed
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            moveFB = 0f;
        
        else if (Input.GetKey(KeyCode.W))
            moveFB = 1f;
        
        else if (Input.GetKey(KeyCode.S))
            moveFB = -1f;
        
        else
            moveFB = 0f;
        
        
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            moveLR = 0f;
        
        else if (Input.GetKey(KeyCode.D))
            moveLR = 1f;
        
        else if (Input.GetKey(KeyCode.A))
            moveLR = -1f;
        
        else
            moveLR = 0f;
        
        
        //Crouching
        if (grounded()) //If Batman is on the ground
        {
            anime.SetBool("Air",false); //Set Air Boolean in the animator to false
            
            if (!anime.GetBool("Crouching")) //If Batman is not Crouching then set the camera to default mode
                camMode = 0;
            
            
            if (Input.GetKeyDown(KeyCode.LeftControl)) // If player hit the left control key which toggles crouching mode
            {
                if (anime.GetBool("Crouching")) // If it's currently crouching
                {
                    anime.SetBool("Crouching", false); // Set it to not crouching
                    camMode = 0;
                }
                else // If it's currently not crouching
                {
                    anime.SetBool("Crouching", true); // Set it to crouching
                    camMode = 1;
                }
            }


            if (moveFB != 0 || moveLR != 0) // If player has input any moving direction
            {
                if (GGScript.rotate == 0 && anime.GetInteger("Grapple") == 0)
                {
                    anime.SetInteger ("Speed", 1); // set moving speed to 1 (walking)
                    anime.SetFloat ("Angle", angleDifference()); // set animation angle to angle difference

                    if (Input.GetKey(KeyCode.LeftShift) && !anime.GetBool("Crouching"))// If left shift is held which will trigger running
                    {
                        anime.SetInteger ("Speed", 2); // set moving speed to 2 (running)
                        camMode = 2;
                    }
            


                    if (movingAnime()) // If it's right now moving straight
                    {
                        if (anime.GetBool("Crouching"))
                            rot = Mathf.LerpAngle (0, angleDifference() + 26f, Time.deltaTime * 10f); // Adjust angle along with frame update in consider so angle adjustment stays consistent with time
                        else
                            rot = Mathf.LerpAngle (0, angleDifference(), Time.deltaTime * 10f); // Adjust angle along with frame update in consider so angle adjustment stays consistent with time
                
                        transform.Rotate(0, rot, 0, Space.World); //Apply angle adjustment while moving
                    }
                }
            }
            
            else
            {
                anime.SetFloat ("Angle", 0f); // If not moving set angle to 0 degree
                anime.SetInteger ("Speed", 0); // If not moving set moving speed to 0 (idle)
            }
        }

        else //AIR
        {
            anime.SetBool("Air",true);
            anime.SetBool("Crouching", false);
            
            //anime.SetInteger("Speed",0);
            
            if (moveFB != 0 || moveLR != 0) // If player has input any moving direction
            {
                anime.SetInteger ("Speed", 0);
                
                if (Input.GetKey(KeyCode.LeftShift))// If left shift is held which will trigger running
                {
                    anime.SetInteger ("Speed", 2); // set moving speed to 2 (running)
                    camMode = 4;
                }
            }

            else //If player don't have any input
            {
                if (anime.GetInteger("Grapple") == 0) anime.SetInteger("Speed",0); //If it's not in grapple mode then change the speed to zero
                camMode = 3; //Set camera mode to 3 (Falling Camera Angle)
            }
            
        }

    }


}
