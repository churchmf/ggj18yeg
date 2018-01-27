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

    public MusicSheet loadedSheet;
    public float levelTimer;

    public float keyStartHeight = 4;
    public float keyDistanceApart = 0.75f;

    public float noteTargetXStartOffset = 5;
    public float noteTargetSpeed = -10;

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
        Note note = sheet.notes.FirstOrDefault(n  => levelTimer - n.time <= 0.5);
        if(note != null)
        {
            var keyObject = GameObject.FindGameObjectsWithTag("Key")
                .FirstOrDefault(k => k.GetComponent<NoteTriggerController>().note == note.note);
            if(keyObject != null)
            {
                var noteTarget = Instantiate(noteTargetPrefab, new Vector3(noteTargetXStartOffset, keyObject.transform.position.y), Quaternion.identity);
                noteTarget.GetComponent<Rigidbody2D>().AddForce(new Vector2(noteTargetSpeed, 0));
            }
        }
    }

    private MusicSheet LoadSinglePlayer()
    {
        uiMenu.SetActive(false);
        uiHud.SetActive(true);

        GameObject.Instantiate(playerPrefab);

        MusicSheet sheet = LevelLoader.Load("example.json");
        for(int i = 0; i < sheet.keys.Length; ++i)
        {
            Key key = sheet.keys[i];
            var noteGameObject = GameObject.Instantiate(noteTriggerPrefab, new Vector3(0, keyStartHeight - (keyDistanceApart * i)), Quaternion.identity);
            noteGameObject.GetComponent<NoteTriggerController>().frequency = key.frequency;
            noteGameObject.GetComponent<NoteTriggerController>().note = key.note;
        }

        return sheet;
    }
}
