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
    public Note[] notes;
    public Dialogue[] dialogue;
}

[Serializable]
public class Note
{
    public string note;
    public float time;
    public float duration;
}

[Serializable]
public class Key
{
    public string note;
    public float frequency;
}

[Serializable]
public class Dialogue
{
    public string text;
    public float time;
    public float duration;
}
