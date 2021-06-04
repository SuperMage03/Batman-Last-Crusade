using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public Animator anime;

    // Update is called once per frame
    void Update()
    {
        if (anime.GetCurrentAnimatorStateInfo(0).IsName("Pressed"))
        {
            Application.Quit(); //Quit the game
        }
    }
}
