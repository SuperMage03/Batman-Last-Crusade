using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Manages NPC interactions
public class NPCDialogue : MonoBehaviour
{
    public GameObject Dialogue;
    public bool DialogueActivation;
    // Start is called before the first frame update
    void Start()
    {
        DialogueActivation = false;
        Dialogue.SetActive(false);
    }

    void SetDialogue()
    {
        //turns the dialogue on and off for each NPC
        if (DialogueActivation)
        {
            Dialogue.SetActive(true);
        }

        else
        {
            Dialogue.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        SetDialogue();
    }
}
