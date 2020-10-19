using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMusicPlayer : MonoBehaviour
{
    private AudioSource m_audioSource;

    private void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(transform.gameObject);
        tag = "MusicPlayer";
    }

    public void PlayMusic()
    {
        if (m_audioSource.isPlaying) 
            return;

        m_audioSource.Play();
    }

    public void StopMusic()
    {
        m_audioSource.Stop();
    }
}