
using UnityEngine;

public class PlayerAudioEmitter : MonoBehaviour
{
    public AudioClip clip;
    public string note;

    private bool clipChanged;

    private AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        clipChanged = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var noteTrigger = other.GetComponent<NoteTriggerController>();
        if(noteTrigger != null)
        {
            clip = noteTrigger.clip;
            note = noteTrigger.note;
            clipChanged = true;
        }
    }

    void FixedUpdate()
    {
        if(clipChanged && clip != null)
        {
            clipChanged = false;
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
