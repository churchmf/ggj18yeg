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
    public string[] key;
    public Note[] notes;
}

[Serializable]
public class Note
{
    public string note;
    public float time;
    public float duration;
}
