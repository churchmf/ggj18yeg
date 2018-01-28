
using UnityEngine;
using System.Collections;

public class PlayerAudioEmitter : MonoBehaviour
{
    public AudioClip clip;
    public string note;

    private bool clipChanged;

    // private AudioSource audioSource;
    AudioSource _source0;
    AudioSource _source1;
    bool cur_is_source0 = true;
    Coroutine _curSourceFadeRoutine = null;
    Coroutine _newSourceFadeRoutine = null;

    void Start()
    {
        AudioSource[] audioSources = gameObject.GetComponents<AudioSource>();
        _source0 = audioSources[0];
        _source1 = audioSources[1];

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
			CrossFade (clip, 1.0f, 0.15f);
        }
    }






    void CrossFade(AudioClip clipToPlay, float maxVolume, float fadingTime, float delay_before_crossFade = 0)
    {
        StartCoroutine(Fade(clipToPlay, maxVolume, fadingTime, delay_before_crossFade));
    }

    IEnumerator Fade(AudioClip playMe, float maxVolume, float fadingTime, float delay_before_crossFade = 0)
    {
        if (delay_before_crossFade > 0)
        {
            yield return new WaitForSeconds(delay_before_crossFade);
        }

        AudioSource curActiveSource, newActiveSource;
        if (cur_is_source0)
        {
            curActiveSource = _source0;
            newActiveSource = _source1;
        }
        else
        {
            curActiveSource = _source1;
            newActiveSource = _source0;
        }

        newActiveSource.clip = playMe;
        newActiveSource.loop = true;
        newActiveSource.Play();
        newActiveSource.volume = 0;

        if (_curSourceFadeRoutine != null)
        {
            StopCoroutine(_curSourceFadeRoutine);
        }

        if (_newSourceFadeRoutine != null)
        {
            StopCoroutine(_newSourceFadeRoutine);
        }

        _curSourceFadeRoutine = StartCoroutine(fadeSource(curActiveSource, curActiveSource.volume, 0, fadingTime));
        _newSourceFadeRoutine = StartCoroutine(fadeSource(newActiveSource, newActiveSource.volume, maxVolume, fadingTime));

        cur_is_source0 = !cur_is_source0;

        yield break;
    }

    IEnumerator fadeSource(AudioSource sourceToFade, float startVolume, float endVolume, float duration)
    {
        float startTime = Time.time;

        while (true)
        {
            float elapsed = Time.time - startTime;
            sourceToFade.volume = Mathf.Clamp01(Mathf.Lerp(startVolume, endVolume, elapsed / duration));

            if (sourceToFade.volume == endVolume)
            {
                break;
            }
            yield return null;
        }
    }

    public bool isPlaying
    {
        get
        {
            return _source0.isPlaying || _source1.isPlaying;
        }
    }
}
