using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transitionAnime;
    public float transitionTime = 1.5f;

    // Update is called once per frame
    void Update()
    {
        if (transitionAnime.GetBool("Transition")) //Gets the transition boolean from the animator
        {
            LoadNextLevel();
            transitionAnime.SetBool("Transition", false); //Set transition back to false after transition
        }
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel((SceneManager.GetActiveScene().buildIndex + 1) % 3)); //Start the coroutine to transition
    }

    IEnumerator LoadLevel(int levelIndex) //Transition Coroutine
    {
        transitionAnime.SetTrigger("Start"); //Start Fade In
        
        yield return  new WaitForSeconds(transitionTime); // Wait time until load game.
        
        SceneManager.LoadScene(levelIndex); // Load the next scene
    }
}
