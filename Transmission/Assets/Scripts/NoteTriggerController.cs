using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteTriggerController : MonoBehaviour {
    public string note;

    public AudioClip audioNote;

    AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(audioNote);
    }
}
