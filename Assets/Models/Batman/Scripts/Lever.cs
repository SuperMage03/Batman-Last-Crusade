using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

//Controls the Interactions made by pulling levers
public class Lever : MonoBehaviour
{

    public bool WallActivation;

    public int SwitchCounter;
    public GameObject Switch;

    public GameObject Wall1;
    public GameObject Wall2;
    public GameObject Wall3;
    public GameObject Wall4;

    public bool IsActive1;
    public bool IsActive2;
    public bool IsActive3;
    public bool IsActive4;

    List<GameObject> Walls;
    List<bool> IsActiveCheck;

    // Start is called before the first frame update
    void Start()
    {
        
        SwitchCounter = 0;
       
        //Getting lists of walls and variables to check if a wall is active or not
        //levers can affect up to 4 objects each
        Walls = new List<GameObject>() { Wall1, Wall2, Wall3, Wall4 };
        IsActiveCheck = new List<bool>() { IsActive1, IsActive2, IsActive3, IsActive4 };

        WallActivation = false;

        //Sets each wall to inactive or active at the start of the game
        for (int i = 0; i < Walls.Count(); i++)
        {
            if (!(IsActiveCheck[i]))
            {
                Walls[i].SetActive(false);
            }
        }
       
    }

    void LeverPulled()
    {
        
        //Set to true from InteractionChooser.cs
        if (WallActivation)
        {
            //Counter to check whether the switch should be upsidedown
            SwitchCounter++;
            
            //flips the lever up and down through rotation
            if (SwitchCounter%2 == 0)
            {
                Switch.transform.eulerAngles = new Vector3(Switch.transform.eulerAngles.x - 90, Switch.transform.eulerAngles.y, Switch.transform.eulerAngles.z);
            }
            else
            {
                Switch.transform.eulerAngles = new Vector3(Switch.transform.eulerAngles.x + 90, Switch.transform.eulerAngles.y, Switch.transform.eulerAngles.z);
            }

            //removes object if object is active, activates object if object is removed
            for(int i = 0; i < Walls.Count(); i++)
            {
                if (Walls[i].activeSelf)
                {
                    IsActiveCheck[i] = false;
                }
                else
                {
                    IsActiveCheck[i] = true;
                }

                Walls[i].SetActive(IsActiveCheck[i]);
            }
                
            
            WallActivation = false;
        }

        
        


    }

    // Update is called once per frame
    void Update()
    {
        LeverPulled();
    }
}
