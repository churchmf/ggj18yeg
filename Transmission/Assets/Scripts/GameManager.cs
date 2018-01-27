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
                LoadSinglePlayer();
                break;
            case GameState.SinglePlayer:
                break;
        }
	}

    public void LoadSinglePlayer()
    {
        menuCanvas.SetActive(false);

        GameObject.Instantiate(playerPrefab);

        MusicSheet sheet = LevelLoader.Load("example.json");
        for(int i = 0; i < sheet.key.Length; ++i)
        {
            Key key = sheet.key[i];
            var noteGameObject = GameObject.Instantiate(noteTriggerPrefab, new Vector3(0, topHeight - (noteDistance * i)), Quaternion.identity);
            noteGameObject.GetComponent<PlayerAudioEmitter>().frequency = key.frequency;
            noteGameObject.GetComponent<PlayerAudioEmitter>().note = key.note;
        }

        state = GameState.SinglePlayer;
    }

    public void StartSinglePlayer()
    {
        state = GameState.InitSinglePlayer;
    }
}
