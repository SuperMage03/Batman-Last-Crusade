using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectiveComplete : MonoBehaviour
{

    public InteractionChooser InteractionChooserScript;
    public Animator anime;
    public int TrophiesTotal;


    // Start is called before the first frame update
    void Start()
    {
        InteractionChooserScript = GameObject.FindWithTag("Player").GetComponent<InteractionChooser>();
        TrophiesTotal = GameObject.FindGameObjectsWithTag("Riddler").Length;
    }

    // Update is called once per frame
    void Update()
    {
        if(InteractionChooserScript.TrophiesFound == TrophiesTotal)
        {
            anime.SetBool("Transition",true);
        }
    }
}
