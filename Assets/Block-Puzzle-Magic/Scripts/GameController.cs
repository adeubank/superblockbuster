using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class GameController : Singleton<GameController>
{
    public static GameMode gameMode = GameMode.TIMED;
    public Canvas UICanvas;

    private void Start()
    {
        DOTween.SetTweensCapacity(1000, 500);
    }

    // Checks if interner is available or not.
    public bool isInternetAvailable()
    {
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
            Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) return true;
        return false;
    }

    /// <summary>
    ///     Quits the game.
    /// </summary>
    public void QuitGame()
    {
        Invoke("QuitGameWithDelay", 0.5F);
    }

    /// <summary>
    ///     Quits the game with delay.
    /// </summary>
    private void QuitGameWithDelay()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
		Application.Quit ();
#endif
    }

    #region Games played

    private const string PrefsGamesPlayed = "GamesPlayed";

    public static int GamesPlayed()
    {
        return PlayerPrefs.GetInt(PrefsGamesPlayed, 0);
    }

    public static void IncrementGamesPlayed()
    {
        PlayerPrefs.SetInt(PrefsGamesPlayed, GamesPlayed() + 1);
    }

    #endregion
}