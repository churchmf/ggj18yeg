using UnityEngine;

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
    public GameObject menuCanvas;

    public MusicSheet loadedSheet;
    public float levelTimer;

    public float topHeight = 4;
    public float noteDistance = 0.75f;

    void Start () {
        state = GameState.MainMenu;
    }
	
	void FixedUpdate () {
		switch(state)
        {
            case GameState.MainMenu:
                menuCanvas.SetActive(true);
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

    public void PlayingLevel(MusicSheet sheet)
    {
        levelTimer += Time.deltaTime;
    }

    public MusicSheet LoadSinglePlayer()
    {
        menuCanvas.SetActive(false);

        GameObject.Instantiate(playerPrefab);

        MusicSheet sheet = LevelLoader.Load("example.json");
        for(int i = 0; i < sheet.key.Length; ++i)
        {
            Key key = sheet.key[i];
            var noteGameObject = GameObject.Instantiate(noteTriggerPrefab, new Vector3(0, topHeight - (noteDistance * i)), Quaternion.identity);
            noteGameObject.GetComponent<NoteTriggerController>().frequency = key.frequency;
            noteGameObject.GetComponent<NoteTriggerController>().note = key.note;
        }

        return sheet;
    }

    public void StartSinglePlayer()
    {
        state = GameState.InitSinglePlayer;
    }
}
