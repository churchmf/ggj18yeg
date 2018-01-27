using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public enum GameState
    {
        MainMenu,
        InitSinglePlayer,
        SinglePlayer
    };

    public GameState state;
    public GameObject playerPrefab;
    public GameObject noteTriggerPrefab;
    public GameObject uiHud;
    public GameObject uiMenu;
    public GameObject timeLabel;
    public GameObject noteTargetPrefab;

    private MusicSheet loadedSheet;
    private Stack<Note> sortedLevelNotes;
    private Dictionary<string, float> noteSpawnPositions;

    private float levelTimer;

    public float keyStartHeight = 4;
    public float keyDistanceApart = 0.75f;

    public float noteTargetXStartOffset = 5;
    public float noteTargetSpeed = -100;
    public float noteTargetXScale = 1;

    void Start () {
        state = GameState.MainMenu;
    }
	
	void Update () {
		switch(state)
        {
            case GameState.MainMenu:
                uiMenu.SetActive(true);
                uiHud.SetActive(false);
                break;
            case GameState.InitSinglePlayer:
                loadedSheet = LoadSinglePlayer();

                sortedLevelNotes = new Stack<Note>(loadedSheet.notes.OrderBy(n => n.time));

                noteSpawnPositions = new Dictionary<string, float>();
                foreach(GameObject keyObject in GameObject.FindGameObjectsWithTag("Key"))
                {
                    var component = keyObject.GetComponent<NoteTriggerController>();
                    noteSpawnPositions.Add(component.note, keyObject.transform.position.y);
                }

                levelTimer = 0;
                state = GameState.SinglePlayer;
                break;
            case GameState.SinglePlayer:
                PlayingLevel(loadedSheet);
                break;
        }
	}

    public void StartSinglePlayer()
    {
        state = GameState.InitSinglePlayer;
    }

    private void PlayingLevel(MusicSheet sheet)
    {
        levelTimer += Time.deltaTime;

        timeLabel.GetComponent<Text>().text = levelTimer.ToString();

        // generate notes for current time
        if(sortedLevelNotes.Any()) {
            if(sortedLevelNotes.Peek().time <= levelTimer)
            {
                Note note = sortedLevelNotes.Pop();

                float spawnY = noteSpawnPositions[note.note];

                var noteTarget = Instantiate(noteTargetPrefab, new Vector3(noteTargetXStartOffset, spawnY), Quaternion.identity);
                noteTarget.transform.localScale = new Vector3(noteTargetXScale * note.duration, 1, 0);
                noteTarget.GetComponent<Rigidbody2D>().AddForce(new Vector2(noteTargetSpeed, 0));
            }
        }
    }

    private MusicSheet LoadSinglePlayer()
    {
        uiMenu.SetActive(false);
        uiHud.SetActive(true);

        Instantiate(playerPrefab);

        MusicSheet sheet = LevelLoader.Load("example.json");
        for(int i = 0; i < sheet.keys.Length; ++i)
        {
            Key key = sheet.keys[i];
            var noteGameObject = Instantiate(noteTriggerPrefab, new Vector3(0, keyStartHeight - (keyDistanceApart * i)), Quaternion.identity);
            noteGameObject.GetComponent<NoteTriggerController>().frequency = key.frequency;
            noteGameObject.GetComponent<NoteTriggerController>().note = key.note;
        }

        return sheet;
    }
}
