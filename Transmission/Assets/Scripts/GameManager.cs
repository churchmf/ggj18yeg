using System.Collections.Generic;
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

    private MusicSheet loadedLevel;
    private Stack<TimedNote> sortedStageNotes;
    private Dictionary<string, float> noteSpawnPositions;

    private float levelTimer;
    private int stageIndex;

    public float keyStartHeight = 4;
    public float keyDistanceApart = 0.75f;

    public float noteTargetXStartOffset = 5;
    public float noteTargetSpeed = -100;
    public float noteTargetXScale = 1;

    private AudioSource audioSource;
    void Start ()
    {
        state = GameState.MainMenu;
        audioSource = GetComponent<AudioSource>();
    }

    void Update () {
		switch(state)
        {
            case GameState.MainMenu:
                uiMenu.SetActive(true);
                uiHud.SetActive(false);
                break;
            case GameState.InitLevel:
                loadedLevel = LoadLevel();

                // spawn key triggers
                noteSpawnPositions = new Dictionary<string, float>();
                foreach (GameObject keyObject in GameObject.FindGameObjectsWithTag("Key"))
                {
                    var component = keyObject.GetComponent<NoteTriggerController>();
                    noteSpawnPositions.Add(component.note, keyObject.transform.position.y);
                }

                stageIndex = 0;
                levelTimer = 0;
                state = GameState.InitStage;
                break;
            case GameState.InitStage:
                InitStage(loadedLevel.stages[stageIndex], loadedLevel.beatsPerMinute, loadedLevel.beatsPerMeasure);
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

    private void InitStage(Stage stage, int tempo, int beatsPerMeasure)
    {
        sortedStageNotes = GetTimedNotes(stage.notes, tempo, beatsPerMeasure);
        foreach (string track in stage.tracks)
        {
            AudioClip audioClip = Resources.Load<AudioClip>(track);
            audioSource.loop = true;
            audioSource.PlayOneShot(audioClip);
        }
    }

    private Stack<TimedNote> GetTimedNotes(Note[] notes, int tempo, int beatsPerMeasure)
    {
        float beatsPerSecond = tempo / 60f;
        float secondsPerBeat = 60f / tempo;
        float secondsPerMeasure = (beatsPerMeasure * secondsPerBeat);

        var timedNotes = new Stack<TimedNote>();

        var measureNotes = notes.GroupBy(n => n.measure);
        foreach(IGrouping<int, Note> measure in measureNotes) 
        {
            float secondsIntoMeasure = 0;
            foreach (Note note in measure)
            {
                secondsIntoMeasure += (note.beat * (tempo / 60));
                timedNotes.Push(new TimedNote()
                {
                    note = note.note,
                    duration = note.duration,
                    time = secondsIntoMeasure + (secondsPerMeasure * (note.measure - 1))
                });
            }
        }
           
        return timedNotes;
    }

    private void PlayingStage(Stage stage)
    {
        levelTimer += Time.deltaTime;
        timeLabel.GetComponent<Text>().text = levelTimer.ToString();

        if (sortedStageNotes.Any())
        {
            if (sortedStageNotes.Any() && sortedStageNotes.Peek().time <= levelTimer)
            {
                TimedNote note = sortedStageNotes.Pop();

                float spawnY = noteSpawnPositions[note.note];

                var noteTarget = Instantiate(noteTargetPrefab, new Vector3(noteTargetXStartOffset, spawnY), Quaternion.identity);
                noteTarget.transform.localScale = new Vector3(noteTargetXScale * note.duration, 1, 0);
                noteTarget.GetComponent<Rigidbody2D>().AddForce(new Vector2(noteTargetSpeed, 0));
            }
        }
    }

    private MusicSheet LoadLevel()
    {
        uiMenu.SetActive(false);
        uiHud.SetActive(true);

        Instantiate(playerPrefab);

        MusicSheet sheet = LevelLoader.Load("example.json");
        for(int i = 0; i < sheet.keys.Length; ++i)
        {
            Key key = sheet.keys[i];
            var noteGameObject = Instantiate(noteTriggerPrefab, new Vector3(0, keyStartHeight - (keyDistanceApart * i)), Quaternion.identity);
            noteGameObject.GetComponent<NoteTriggerController>().clip = Resources.Load<AudioClip>(key.file);
            noteGameObject.GetComponent<NoteTriggerController>().note = key.note;
        }

        return sheet;
    }
}
