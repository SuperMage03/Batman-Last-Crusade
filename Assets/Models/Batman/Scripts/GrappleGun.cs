using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleGun : MonoBehaviour
{
    public Transform player;
    public Animator anime;
    public CharacterController CC;
    public float grappleSpeed = 2f;
    public float shootSpeed = 4f;
    

    private SkinnedMeshRenderer gunMesh;
    private Transform gunDummy;
    private Transform gunAttachPoint;
    private Transform gunShootPoint;
    private LineRenderer rope;
    private Grapple grappleScript;
    private Vector3 gunShootPointPos;
    
    private float counter = 0f;
    private float pullRopeCounter = 0f;
    private float gunDist;
    private Vector3 oldPos = Vector3.zero;
    private Vector3 grapplePoint = Vector3.zero;
    private PlayerMovement movementScript;
    private float grappleTime;

    // Start is called before the first frame update
    void Start() //Binding and referencing all the transforms/game objects/scripts
    {
        grappleScript = GameObject.Find("Batman").GetComponent<Grapple>();
        gunMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        gunMesh.enabled = false;
        gunAttachPoint = transform.Find ("Grapple");
        gunDummy = player.transform.Find ("Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_R_Clavicle/Bip01_R_UpperArm/Bip01_R_Forearm/Bip01_R_Hand/Gundummy");
        gunShootPoint = transform.Find ("Grapple/Two_Hook_Bat_Claw01");
        rope = GetComponent<LineRenderer>();
        rope.enabled = false;
        movementScript = GameObject.Find("Batman").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        int currentGrappleState = anime.GetInteger("Grapple"); //Get the current grapple animation state
        
        
        if (currentGrappleState != 0) //If it's in the state of grappling
        {
            gunShootPointPos = gunShootPoint.position; //Align the grapple gun with batman's hand
            gunAttachPoint.position = gunDummy.position;
            gunAttachPoint.rotation = gunDummy.rotation;
            if (!gunMesh.enabled) gunMesh.enabled = true; //make grapple gun appear

            if (currentGrappleState == 1) //If player is shooting the grapple or grappling
            {
                if (anime.GetCurrentAnimatorStateInfo(0).IsName("Grappling.Grapple_Fire")) //Fire grapple on the ground
                {
                    movementScript.camMode = 5; //Set the camera mode
                    shootRope(); //Shoot the grapple rope
                }

                if (anime.GetCurrentAnimatorStateInfo(0).IsName("Grappling.Grapple_Hold_Forward")) //Rope is attached to the building
                {
                    movementScript.camMode = 5; //Set the camera mode
                    pullRope(); //Pull Batman to the ledge
                }
                    
                
                if (anime.GetCurrentAnimatorStateInfo(0).IsName("In Air.Fall")) //If Batman is falling but it's grappling, set it to the glide mode
                    anime.SetInteger("Speed",2);
                
                if (anime.GetCurrentAnimatorStateInfo(0).IsName("Grappling.Glide_Grapple_Aim")) //If Batman shooting grapple while gliding
                {
                    movementScript.camMode = 6; //Set the camera mode
                    shootRope(); //Shoot the rope
                }
            }

            if (currentGrappleState == 2) //If Batman finished grappling, time to get up the ledge
            {
                if (Time.time - grappleTime <= 0.4f && Time.time - grappleTime >= 0.2f) //During the time window of 0.2 - 0.4 of a second when Batman is in the going up the ledge animation
                {
                    player.position = grapplePoint + new Vector3(player.forward.x, 0f, player.forward.y) * 0.1f; //Teleport Batman to the destination with little bit adjustment forward
                    rope.enabled = false; //Make the rope disappear
                    CC.enabled = true; //Turn the character controller back on
                }
                
                
                    
                
                else if (Time.time - grappleTime > 1.733f) //At the end of the animation
                {
                    anime.SetInteger("Grapple",0); //Set Grapple back to 0
                    oldPos = Vector3.zero; //Reset the grapple point
                    counter = 0f; //Set Counter back to 0
                    pullRopeCounter = 0f; //Set pullRopeCounter back to 0
                }
            }
            
            
        }
        
        else //If player is not grappling
        {
            if (gunMesh.enabled) gunMesh.enabled = false; //Set Batman's grapple gun to invisible
            if (rope.enabled) rope.enabled = false; //Set the grapple rope to disappear
        }
    }




    
    void shootRope()
    {
        rope.SetPosition(0,gunShootPointPos); //Set one set of the rope to grapple gun's nozzle
        grapplePoint = grappleScript.finalGrapple; //Get the final grapple point in the grapple detection script
        gunDist = Vector3.Distance(gunShootPointPos, grapplePoint); //Find how long the grapple rope is
        
        if (!rope.enabled) rope.enabled = true; //Enable the rope to appear
        
        if (counter < gunDist) //If the distance counter of the rope going toward the point is less than the full distance of the rope
        {
            counter += shootSpeed * Time.deltaTime; //Add shootSpeed per second to the counter
            Vector3 newPos = gunShootPointPos + counter * (grapplePoint - gunShootPointPos).normalized; //Find the new position of the end of the rope
            rope.SetPosition(1,newPos); //Set the other end of the rope to the new position
        }
        
    }

    void pullRope()
    {
        if (CC.enabled) CC.enabled = false; //If character controller is still active, set it to inactive
        if (oldPos == Vector3.zero) oldPos = gunShootPointPos; //If oldPos still have a place holder value, set it to the position of the grapple gun nozzle
        anime.SetBool("Air",false); //Set Batman in air to false
        anime.SetInteger("Speed",0); //Set Batman's speed to 0
        rope.SetPosition(0,gunShootPointPos); //Set the start of grapple rope position to the gun nozzle
        rope.SetPosition(1,grapplePoint); // Set the end of grapple rope to the grapple point
        
        //float rotx = Mathf.LerpAngle(0, Mathf.Asin((grapplePoint.y - player.position.y) / (grapplePoint.z - player.position.z)) * Mathf.Rad2Deg, Time.deltaTime * 10f);
        //if (Mathf.Abs(0f - rotx) > 0.001f) player.Rotate(rotx, 0f, 0f);
        
        if (Vector3.Distance(gunShootPointPos,grapplePoint) > 1f) //If Batman is not close enough to the grapple point
        {
            pullRopeCounter += grappleSpeed * Time.deltaTime; //Add grappleSpeed per second to how much Batman has grapped
            Vector3 newPos = oldPos + pullRopeCounter * (grapplePoint - oldPos).normalized; //Find the position along the line of which Batman is going at with the pull distance
            player.position = newPos; //Set the player position to the new position
        }
        else //If Batman is close enough to the grappling point
        {
            grappleTime = Time.time; //Record the time for time window check
            anime.SetInteger("Grapple",2); //Set Grapple Mode to 2
        }
    }
    
}
