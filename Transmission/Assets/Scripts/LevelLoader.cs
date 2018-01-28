using System;
using System.IO;
using UnityEngine;

public static class LevelLoader
{
    public static MusicSheet Load(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        if(File.Exists(filePath)) {
            return JsonUtility.FromJson<MusicSheet>(File.ReadAllText(filePath));
        }

        return null;
    }
}

[Serializable]
public class MusicSheet
{
    public Key[] keys;
    public Stage[] stages;
    public int beatsPerMinute;
    public int beatsPerMeasure;
}

[Serializable]
public class Stage
{
    public string[] tracks;
    public Note[] notes;
    public Dialogue[] dialogue;
}

[Serializable]
public class Note
{
    public string note;
    public int measure;
    public float beat;
}

[Serializable]
public class Key
{
    public string note;
    public string file;
}

[Serializable]
public class Dialogue
{
    public string text;
    public float measure;
    public float beat;
    public float duration;
}

public class TimedNote
{
    public string note;
    public float time;
    public float beat;
}

public class TimedDialogue
{
    public string text;
    public float time;
    public float beat;
}
