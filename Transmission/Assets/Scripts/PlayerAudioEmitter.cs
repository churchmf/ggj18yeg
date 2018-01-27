
using UnityEngine;

public class PlayerAudioEmitter : MonoBehaviour
{
    public int position = 0;
    public int samplerate = 44100;
    public float frequency;
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
            frequency = noteTrigger.frequency;
            note = noteTrigger.note;
        }
    }

    void FixedUpdate()
    {
        if(!audioSource.isPlaying && !string.IsNullOrEmpty(note))
        {
            audioSource.clip = AudioClip.Create("audioClip", samplerate, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
            audioSource.Play();
        }
    }

    void OnAudioRead(float[] data)
    {
        int count = 0;
        while (count < data.Length)
        {
            data[count] = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * frequency * position / samplerate));
            position++;
            count++;
        }
    }

    void OnAudioSetPosition(int newPosition)
    {
        position = newPosition;
    }
}
