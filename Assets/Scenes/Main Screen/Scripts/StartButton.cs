using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    public Animator anime;
    public Animator TransitionAnime;
    public GameObject note;
    
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        note.SetActive(false);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && note.activeSelf)
        {
            TransitionAnime.SetBool("Transition", true); //Start the game and transition it to the Gotham Game Scene if note is been showed
        }
        
        else if (anime.GetCurrentAnimatorStateInfo(0).IsName("Highlighted") && Input.GetMouseButtonDown(0) &&
                 !note.activeSelf)
        {
            note.SetActive(true); //Shows the note if the game starts
        }
    }
}
