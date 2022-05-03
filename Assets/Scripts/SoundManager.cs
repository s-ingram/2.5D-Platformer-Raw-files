using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance = null;
    private AudioSource audioSource;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance == null)
            Instance = this;
        else if (Instance != null)
            Destroy(gameObject);
    }

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.volume = .1f;
        audioSource.loop = false;
        audioSource.playOnAwake = false;

    }

    public void PlaySound(AudioClip clipToPlay)
    {
    //    audioSource.clip = clipToPlay;
        audioSource.PlayOneShot(clipToPlay);
    }
}
