using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Security.Permissions;


//Finds the closest interaction when available and runs it when given input
public class InteractionChooser : MonoBehaviour
{
    //for accessing from other scripts
    public NPCDialogue NPCDialogueScript;
    public Interact InteractScript;
    public Lever LeverScript;


    public GameObject NearestInteraction;
    public int TrophiesFound;
    public GameObject ShowIfInRange;
    public bool InteractionMade;

    public AudioSource RiddlerDing;

    public GameObject FindClosest()
    {
        GameObject[] unsortedinteractive;
        GameObject[] unsortedriddler;
        GameObject[] unsortedlever;
        GameObject[] unsorted;
        Vector3 origin;
        float NearestDist;
        GameObject closest;

        closest = null;
        NearestDist = Mathf.Infinity;
        //Finds every Game Object with tags "Interactive","Riddler" and "Lever"
        unsortedinteractive = GameObject.FindGameObjectsWithTag("Interactive");
        unsortedriddler = GameObject.FindGameObjectsWithTag("Riddler");
        unsortedlever = GameObject.FindGameObjectsWithTag("Lever");
        //joins the lists into an array
        unsorted = unsortedinteractive.Concat(unsortedriddler).Concat(unsortedlever).ToArray();
        //defines player position
        origin = GameObject.FindWithTag("Player").transform.position;
        //finds the nearest item to the player
        foreach (GameObject item in unsorted)
        {
            if (NearestDist > Vector3.Distance(origin, item.transform.position))
            {
                NearestDist = Vector3.Distance(origin, item.transform.position);
                closest = item;
            }
        }
        return closest;
    }


    void Start()
    {
        TrophiesFound = 0;
        InteractionMade = false;
        RiddlerDing = GetComponent<AudioSource>();
    }


    void InteractionTypes()
    {
        NearestInteraction = FindClosest();

        //accessing other scripts
        InteractScript = NearestInteraction.GetComponent<Interact>();
        NPCDialogueScript = NearestInteraction.GetComponent<NPCDialogue>();
        LeverScript = NearestInteraction.GetComponent<Lever>();



        //interactions
        if (Input.GetKeyDown(KeyCode.E) && InteractScript.interactable)
        {

            InteractionMade = true;

            //Riddler Trophies
            if (NearestInteraction.gameObject.tag == "Riddler")
            {
                RiddlerDing.Play();
                NearestInteraction.SetActive(false);
                InteractionMade = false;
                TrophiesFound++;
            }

            //NPC's
            if (NearestInteraction.gameObject.tag == "Interactive")
            {
                NPCDialogueScript.DialogueActivation = true;

            }

            ShowIfInRange.SetActive(false);

            //Lever's
            if (NearestInteraction.gameObject.tag == "Lever")
            {
                LeverScript.WallActivation = true;

                InteractionMade = false;
                ShowIfInRange.SetActive(true);
            }



        }
    }


    // Update is called once per frame
    void Update()
    {
        InteractionTypes();
    }
}
