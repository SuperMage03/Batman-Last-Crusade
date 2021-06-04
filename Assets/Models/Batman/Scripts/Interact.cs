using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

//Hitbox Detection for Interactable objects
public class Interact : MonoBehaviour

{
    private InteractionChooser InteractionChooserScript;
    private GameObject player;


    private GameObject trigger;
    public bool InRange;
    public GameObject IfRange;
    public bool interactable;
    // Start is called before the first frame update
    void Start()
    {

        IfRange.SetActive(false);
        player = GameObject.FindWithTag("Player");

        InteractionChooserScript = player.GetComponent<InteractionChooser>();
    }



    void Interactable()
    {
        if (InRange && !(InteractionChooserScript.InteractionMade))
        {
            IfRange.SetActive(true);
            interactable = true;
        }
        else
        {
            interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Interactable();
    }

    //checks everytime hitboxes collide
    void OnTriggerEnter(Collider colliding)
    {
        if (colliding.tag == "Player")
        {
            trigger = colliding.gameObject;
            InRange = true;
            IfRange.SetActive(true);
        }
    }

    //checks everytime hitboxes stop colliding
    void OnTriggerExit(Collider colliding)
    {
        if(colliding.tag == "Player")
        {
            IfRange.SetActive(false);
            InRange = false;
            trigger = null;

            //accessing other scripts and resetting the NPC dialogue
            InteractionChooserScript.InteractionMade = false;
            if(InteractionChooserScript.NearestInteraction.gameObject.tag == "Interactive")
            {
                InteractionChooserScript.NPCDialogueScript.DialogueActivation = false;
            }
            

            
        }
    }
}
