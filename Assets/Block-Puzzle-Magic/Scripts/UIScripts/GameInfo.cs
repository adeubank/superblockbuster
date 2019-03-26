using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    CLASSIC = 1,
    TIMED = 2,
    BLAST = 3,
    ADVANCE = 4,
    CHALLENGE = 5
}

public class GameInfo : Singleton<GameInfo>
{
    public static string ReviewURL;

    public string AboutURL;
    public List<GameModesInfo> gameModesInfo;

    [SerializeField] private string ReviewURL_Google;

    [SerializeField] private string ReviewURL_iOS;

    private void Start()
    {
#if UNITY_ANDROID
        ReviewURL = ReviewURL_Google;
#elif UNITY_IOS
        ReviewURL = ReviewURL_iOS;
#endif
    }
}

[Serializable]
public class GameModesInfo
{
    public GameMode gameMode;
    public bool isActive;
    public int modeIndex;
    public string modeName;
}