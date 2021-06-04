using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class OptionsButton : MonoBehaviour
{
    public Animator anime;
    public GameObject Options;
    public EventSystem evt;
    private bool OptionMenu = false;
    private readonly List<string> hovering = new List<string>();
    private static PointerEventData  PED;
    private float pressedTime;

    
    
    void Start()
    {
        Options.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        hovering.Clear();
        PED = ExtendedStandaloneInputModule.GetPointerEventData(); //Store all the pointer data to PED
        foreach (GameObject GO in PED.hovered) //Check All the gameObject that the cursor is under
        {
            hovering.Add(GO.name);
            //Debug.Log(GO.name);
        }
        
        if (Input.GetMouseButtonDown(0) && !hovering.Contains("OptionsBG") && hovering.Contains(gameObject.name)) //If player clicks Options Button
        {
            OptionMenu = !OptionMenu; //Turn on or off the Option Menu
        }
        
        if (!OptionMenu) evt.SetSelectedGameObject(null); //If Option Menu Not Opened Then Selected Oject should be null
        else evt.SetSelectedGameObject(gameObject); //If Option Menu is opened then OptionButton should be the selected opject
        
        /*
        if (evt.currentSelectedGameObject == this.gameObject)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!hovering.Contains("OptionsBG"))
                {
                    evt.SetSelectedGameObject(null);
                }
                else
                {
                    evt.SetSelectedGameObject(gameObject);
                }
            }
        }
        
        else
        {
            if (Input.GetMouseButtonDown(0) && hovering.Contains(gameObject.name))
            {
                Debug.Log("Hi");
                evt.SetSelectedGameObject(gameObject);
            }
        }
        */
        //Debug.Log(!hovering.Contains("OptionsBG"));
        
        Options.SetActive(OptionMenu); //Set Option menu active base on the boolean OptionMenu
    }


}
