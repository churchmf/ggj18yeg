﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public enum GameState
    {
        MainMenu,
        InitLevel,
        InitStage,
        PlayingStage,
        PassStage,
        PassLevel
    };

    public GameState state;
    public GameObject playerPrefab;
    public GameObject noteTriggerPrefab;
    public GameObject uiHud;
    public GameObject uiMenu;
    public GameObject timeLabel;
    public GameObject noteTargetPrefab;
    public Text levelName;

    private MusicSheet loadedLevel;
    private Stack<TimedNote> sortedStageNotes;
    private Stack<TimedDialogue> sortedStageDialogue;
    private Dictionary<string, float> noteSpawnPositions;

    private float timer;
    private int stageIndex;

    public float keyStartHeight;
    public float keyDistanceApart;

    public float noteTargetXStartOffset;
    public float noteTargetSpeed;
    public float noteTargetXScale;

    private GameObject playerGameObject;

    private List<string> levelList = new List<string>(new string[] {
        "owe.json",
        "super.json"
    });
    private int levelIndex;

    public GameObject dialoguePanel;
    public List<AudioSource> audioSourceTracks;
    void Start ()
    {
        audioSourceTracks = new List<AudioSource>(GetComponents<AudioSource>());
        state = GameState.MainMenu;

        int width = 1366; 
        int height = 768;
        bool isFullScreen = false; 
        int desiredFPS = 60;
        Screen.SetResolution(width, height, isFullScreen, desiredFPS);
    }

    void Update () {
        timer += Time.deltaTime;
        if (Input.GetKey(KeyCode.Escape))
        {
            CleanUpLevel();
            state = GameState.MainMenu;
        }

        switch (state)
        {
            case GameState.MainMenu:
                uiMenu.SetActive(true);
                uiHud.SetActive(false);
                levelIndex = 0;
                break;
            case GameState.InitLevel:
                CleanUpLevel();
                LoadLevel(levelList[levelIndex]);
                stageIndex = 0;
                state = GameState.InitStage;
                break;
            case GameState.InitStage:
                if (spawnedNotes != null && spawnedNotes.Any())
                {
                    foreach(var spawn in spawnedNotes)
                    {
                        Destroy(spawn);
                    }
                }
                spawnedNotes = new List<GameObject>();

                InitStage(loadedLevel.stages[stageIndex], loadedLevel.beatsPerMinute, loadedLevel.beatsPerMeasure);
                timer = 0;
                state = GameState.PlayingStage;
                break;
            case GameState.PlayingStage:
                PlayingStage(loadedLevel.stages[stageIndex]);
                break;
            case GameState.PassStage:
                stageIndex++;
                if(stageIndex < loadedLevel.stages.Length)
                {
                    state = GameState.InitStage;
                }
                else
                {
                    timer = 0;
                    state = GameState.PassLevel;
                }
                break;
            case GameState.PassLevel:
                levelIndex++;
                if (levelIndex < levelList.Count())
                {
                    state = GameState.InitLevel;
                }
                else
                {
                    CleanUpLevel();
                    state = GameState.MainMenu;
                    uiHud.SetActive(true);
                }
                break;
        }
	}

    public void StartLevel()
    {
        state = GameState.InitLevel;
    }

    private void CleanUpLevel()
    {
        audioSourceTracks.ForEach(a => a.Stop());

        if (spawnedNotes != null && spawnedNotes.Any())
        {
            foreach (var spawn in spawnedNotes)
            {
                Destroy(spawn);
            }
        }
        spawnedNotes = null;

        if (playerGameObject != null)
        {
            Destroy(playerGameObject);
        }

        foreach (var key in GameObject.FindGameObjectsWithTag("Key"))
        {
            Destroy(key);
        }
    }

    private void InitStage(Stage stage, int tempo, int beatsPerMeasure)
    {
        sortedStageDialogue = GetTimedDialogue(stage.dialogue, tempo, beatsPerMeasure);
        sortedStageNotes = GetTimedNotes(stage.notes, tempo, beatsPerMeasure);

        // Stop all playing tracks
        audioSourceTracks.ForEach(a => a.Stop());

        // ensure we have an audio source for each track
        while (GetComponents<AudioSource>().Length > stage.tracks.Length)
        {
            Destroy(audioSourceTracks.Last());
        }
        while (GetComponents<AudioSource>().Length < stage.tracks.Length)
        {
            gameObject.AddComponent<AudioSource>();
        }
        audioSourceTracks = new List<AudioSource>(GetComponents<AudioSource>());

        // Start all new tracks
        for (int i = 0; i < stage.tracks.Length; ++i)
        {
            string path = Path.Combine("Audio", stage.tracks[i]);
            AudioClip audioClip = Resources.Load<AudioClip>(path);

            audioSourceTracks[i].clip = audioClip;
            audioSourceTracks[i].Play();
        }
    }

    private Stack<TimedDialogue> GetTimedDialogue(Dialogue[] dialogues, int tempo, int beatsPerMeasure)
    {
        float secondsPerBeat = 60f / tempo;
        float secondsPerMeasure = (beatsPerMeasure * secondsPerBeat);

        var timedNotes = new List<TimedDialogue>();

        if(dialogues != null && dialogues.Any())
        {
            var measureDialogue = dialogues.GroupBy(d => d.measure).OrderByDescending(m => m.Key);
            foreach (IGrouping<int, Dialogue> measure in measureDialogue)
            {
                float secondsIntoMeasure = 0;
                foreach (Dialogue dialogue in measure)
                {
                    Color color;
                    ColorUtility.TryParseHtmlString(dialogue.color, out color);
                    timedNotes.Add(new TimedDialogue()
                    {
                        text = dialogue.text,
                        duration = dialogue.beat * secondsPerBeat,
                        time = secondsIntoMeasure + (secondsPerMeasure * (dialogue.measure - 1)),
                        color = color
                });
                    secondsIntoMeasure += (dialogue.beat * secondsPerBeat);
                }
            }
        }
        return new Stack<TimedDialogue>(timedNotes.OrderByDescending(t => t.time));
    }

    private Stack<TimedNote> GetTimedNotes(Note[] notes, int tempo, int beatsPerMeasure)
    {
        float secondsPerBeat = 60f / tempo;
        float secondsPerMeasure = (beatsPerMeasure * secondsPerBeat);

        var timedNotes = new List<TimedNote>();

        if(notes != null && notes.Any())
        {
            var measureNotes = notes.GroupBy(n => n.measure).OrderByDescending(m => m.Key);
            foreach(IGrouping<int, Note> measure in measureNotes)
            {
                float secondsIntoMeasure = 0;
                foreach (Note note in measure)
                {
                    timedNotes.Add(new TimedNote()
                    {
                        note = note.note,
                        duration = note.beat * secondsPerBeat,
                        time = secondsIntoMeasure + (secondsPerMeasure * (note.measure - 1))
                    });
                    secondsIntoMeasure += (note.beat * secondsPerBeat);
                }
            }
        }

        return new Stack<TimedNote>(timedNotes.OrderByDescending(t => t.time));
    }

    private IEnumerator ActionAfterSeconds(Action action, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        action.Invoke();
    }

    private IEnumerator BackToMenuAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        CleanUpLevel();
        state = GameState.MainMenu;
    }

    private IEnumerator DeactivateAfterSeconds(GameObject gameObject, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }

    private Coroutine deactivateDialogueCoroutine;
    private List<GameObject> spawnedNotes;
    private void PlayingStage(Stage stage)
    {
        if (sortedStageDialogue.Any() && sortedStageDialogue.Peek().time <= timer)
        {
            TimedDialogue dialogue = sortedStageDialogue.Pop();

            Text textObject = dialoguePanel.GetComponentInChildren<Text>();
            textObject.text = dialogue.text;
            textObject.color = dialogue.color;

            dialoguePanel.SetActive(true);

            // Hide text after duration
            if (deactivateDialogueCoroutine != null)
            {
                StopCoroutine(deactivateDialogueCoroutine);
            }
            deactivateDialogueCoroutine = StartCoroutine(DeactivateAfterSeconds(dialoguePanel, dialogue.duration));
        }

        if (sortedStageNotes.Any() && sortedStageNotes.Peek().time <= timer)
        {
            TimedNote note = sortedStageNotes.Pop();
            if(!string.IsNullOrEmpty(note.note) && noteSpawnPositions.Any())
            {
                var noteTarget = Instantiate(noteTargetPrefab, new Vector3(noteTargetXStartOffset, noteSpawnPositions[note.note]), Quaternion.identity);
                noteTarget.GetComponent<SpriteRenderer>().size = new Vector2(noteTargetXScale * note.duration, 1);
                noteTarget.GetComponent<Rigidbody2D>().AddForce(new Vector2(noteTargetSpeed, 0));
                spawnedNotes.Add(noteTarget);
            }
        }

        // If all tracks are done playing, stage is over
        if (!sortedStageNotes.Any() && !sortedStageDialogue.Any() && audioSourceTracks.All(a => !a.isPlaying))
        {
            // Check if we won, otherwise restart
            if (spawnedNotes.All(s => s.GetComponent<NoteTargetController>().hit))
            {
                state = GameState.PassStage;
            }
            else
            {
                state = GameState.InitStage;
            }
        }
    }

    private void LoadLevel(string name)
    {
        uiMenu.SetActive(false);
        uiHud.SetActive(false);

        playerGameObject = Instantiate(playerPrefab);

        noteSpawnPositions = new Dictionary<string, float>();

        try
        {
            MusicSheet sheet = LevelLoader.Load(name);
            for(int i = 0; i < sheet.keys.Length; ++i)
            {
                Key key = sheet.keys[i];
                var y = keyStartHeight - (keyDistanceApart * i);
                var noteGameObject = Instantiate(noteTriggerPrefab, new Vector3(0, y), Quaternion.identity);
                noteGameObject.GetComponent<NoteTriggerController>().clip = Resources.Load<AudioClip>(Path.Combine("Audio", key.file));
                noteGameObject.GetComponent<NoteTriggerController>().note = key.note;

                noteSpawnPositions.Add(key.note, y);
            }
            loadedLevel = sheet;
        } catch (Exception e)
        {
            Console.WriteLine(e);
            CleanUpLevel();
            state = GameState.MainMenu;
        }

    }
}
