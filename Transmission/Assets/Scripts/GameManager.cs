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
        PlayingStage
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
    private Dictionary<string, float> noteSpawnPositions;

    private float timer;
    private int stageIndex;

    public float keyStartHeight = 4.5f;
    public float keyDistanceApart = 0.75f;

    public float noteTargetXStartOffset = 5;
    public float noteTargetSpeed = -100;
    public float noteTargetXScale = 1;

    public GameObject playerGameObject;

    private AudioSource audioSource;
    void Start ()
    {
        state = GameState.MainMenu;
        audioSource = GetComponent<AudioSource>();
    }

    void Update () {
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
                break;
            case GameState.InitLevel:
                LoadLevel(levelName.text);
                stageIndex = 0;
                state = GameState.InitStage;
                break;
            case GameState.InitStage:
                timer = 0;
                InitStage(loadedLevel.stages[stageIndex], loadedLevel.beatsPerMinute, loadedLevel.beatsPerMeasure);
                state = GameState.PlayingStage;
                break;
            case GameState.PlayingStage:
                PlayingStage(loadedLevel.stages[stageIndex]);
                break;
        }
	}

    public void StartLevel()
    {
        state = GameState.InitLevel;
    }

    private void CleanUpLevel()
    {
        audioSource.Stop();

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
        sortedStageNotes = GetTimedNotes(stage.notes, tempo, beatsPerMeasure);
        foreach (string track in stage.tracks)
        {
            AudioClip audioClip = Resources.Load<AudioClip>(Path.Combine("Audio", track));
            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private Stack<TimedNote> GetTimedNotes(Note[] notes, int tempo, int beatsPerMeasure)
    {
        float secondsPerBeat = 60f / tempo;
        float secondsPerMeasure = (beatsPerMeasure * secondsPerBeat);

        var timedNotes = new List<TimedNote>();

        var measureNotes = notes.GroupBy(n => n.measure).OrderByDescending(m => m.Key);
        foreach(IGrouping<int, Note> measure in measureNotes)
        {
            float secondsIntoMeasure = 0;
            foreach (Note note in measure)
            {
                timedNotes.Add(new TimedNote()
                {
                    note = note.note,
                    beat = note.beat,
                    time = secondsIntoMeasure + (secondsPerMeasure * (note.measure - 1))
                });
                secondsIntoMeasure += (note.beat * secondsPerBeat);
            }
        }

        return new Stack<TimedNote>(timedNotes.OrderByDescending(t => t.time));
    }

    public bool completedStage = false;
    private void PlayingStage(Stage stage)
    {
        timer += Time.deltaTime;
        timeLabel.GetComponent<Text>().text = timer.ToString();

        if (sortedStageNotes.Any())
        {
            if (sortedStageNotes.Peek().time <= timer)
            {
                TimedNote note = sortedStageNotes.Pop();
                if(!string.IsNullOrEmpty(note.note) && noteSpawnPositions.Any())
                {
                    var noteTarget = Instantiate(noteTargetPrefab, new Vector3(noteTargetXStartOffset, noteSpawnPositions[note.note]), Quaternion.identity);
                    noteTarget.transform.localScale += new Vector3(noteTargetXScale * note.beat, 0);
                    noteTarget.GetComponent<Rigidbody2D>().AddForce(new Vector2(noteTargetSpeed, 0));
                }
            }
        }
        else if(!completedStage)
        {
            state = GameState.InitStage;
        }
    }

    private void LoadLevel(string name)
    {
        uiMenu.SetActive(false);
        uiHud.SetActive(true);

        playerGameObject = Instantiate(playerPrefab);

        noteSpawnPositions = new Dictionary<string, float>();

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
    }
}
