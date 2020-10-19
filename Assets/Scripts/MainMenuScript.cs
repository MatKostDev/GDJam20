using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    void Start()
    {
        GameObject.FindGameObjectWithTag("MusicPlayer").GetComponent<MyMusicPlayer>().PlayMusic();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("PlayScene");
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene("HowToPlayScene");
    }
}
