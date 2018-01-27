
using UnityEngine;

public class PlayerAudioEmitter : MonoBehaviour
{
    public AudioClip clip;
    public string note;

    private AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var noteTrigger = other.GetComponent<NoteTriggerController>();
        if(noteTrigger != null)
        {
            clip = noteTrigger.clip;
            note = noteTrigger.note;
        }
    }

    void FixedUpdate()
    {
        if(!audioSource.isPlaying && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
