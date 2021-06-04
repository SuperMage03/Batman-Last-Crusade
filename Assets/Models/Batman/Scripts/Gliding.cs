using System;
using System.Collections;
using UnityEngine;

public class Gliding : MonoBehaviour
{
    public CharacterController CC;
    public Transform cam;
    public Animator anime;
    public float liftFactor = 0.07f;
    private float oldPosX, oldPosY, oldPosZ;
    private float velX, velY, velZ;
    private Vector3 moveDirection;
    private Vector3 vel = Vector3.zero;

    void Start()
    {
        oldPosX = transform.position.x;
        oldPosY = transform.position.y;
        oldPosZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {   
        
        //Apply Gravity
        moveDirection = Vector3.zero;
        moveDirection.y -= 9.81f;
        if (CC.enabled) CC.Move(moveDirection * Time.deltaTime);
        
        
        velX = (transform.position.x - oldPosX) / Time.deltaTime;
        velY = (transform.position.y - oldPosY) / Time.deltaTime;
        velZ = (transform.position.z - oldPosZ) / Time.deltaTime;
        vel = new Vector3(velX, velY, velZ);
        //Debug.Log(vel + " " + vel.magnitude);
        

        if (anime.GetBool("Air") || anime.GetCurrentAnimatorStateInfo(0).IsName("In Air.Glide_Loop"))
        {

            if (anime.GetInteger("Speed") == 2 || anime.GetCurrentAnimatorStateInfo(0).IsName("In Air.Glide_Loop"))
            {
                if (CC.enabled) gliding();
            }

            else
            {
                if (transform.eulerAngles.x != 0)
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y,0);
            }

        }
        else
        {

            
            if (transform.eulerAngles.x != 0)
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y,0);
        }


        //CC.Move(moveDirection * Time.deltaTime);
        
        oldPosX = transform.position.x;
        oldPosY = transform.position.y;
        oldPosZ = transform.position.z;
        
        
    }

    public void gliding()
    {
        transform.eulerAngles = new Vector3(cam.eulerAngles.x,cam.eulerAngles.y,0f); //Apply angle adjustment while moving
        /*
        moveDirection += transform.forward * 3f;
        float facingVel = Vector3.Dot(transform.forward, moveDirection);
        float forwardVel = transform.InverseTransformDirection(moveDirection).z;
        moveDirection = Vector3.Lerp(moveDirection, moveDirection * forwardVel, liftFactor * facingVel * forwardVel);
        
        
        Debug.Log(facingVel + " " + forwardVel+ " " + moveDirection);
        */
        
        
        //Vertical velocity turns into horizontal velocity
        Vector3 vertVel = CC.velocity - Vector3.ProjectOnPlane(CC.velocity, transform.up);
        CC.Move(-vertVel * Time.deltaTime);
        
        Vector3 Upforce = vertVel.magnitude * transform.forward * Time.deltaTime;
        CC.Move(Upforce);
        
        
        //Drag
        Vector3 forwardDrag = CC.velocity - Vector3.ProjectOnPlane(CC.velocity, transform.forward);
        Vector3 forwardD = -forwardDrag * forwardDrag.magnitude * Time.deltaTime / 1000;
        CC.Move(forwardD);
        
        
        Vector3 sideDrag = CC.velocity - Vector3.ProjectOnPlane(CC.velocity, transform.right);
        Vector3 sideD = -sideDrag * sideDrag.magnitude * Time.deltaTime;
        CC.Move(sideD);
        
    }
}
