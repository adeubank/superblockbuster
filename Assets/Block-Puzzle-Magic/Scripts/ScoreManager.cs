using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    private int bestScore = 0;

    [HideInInspector] public int Score;
    [SerializeField] private GameObject scoreAnimator;
    [SerializeField] private Text txtAnimatedText;
    [SerializeField] private Text txtBestScore;
    [SerializeField] private Text txtScore;

    private void Start()
    {
        txtScore.text = Score.ToString();
        var bestScore = PlayerPrefs.GetInt("BestScore_" + GameController.gameMode, Score);
        txtBestScore.text = bestScore.ToString();
    }

    public void AddScore(int scoreToAdd, bool doAnimate = true)
    {
        var oldScore = Score;
        Score += scoreToAdd;

        StartCoroutine(SetScore(oldScore, Score));

        if (doAnimate)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            scoreAnimator.transform.position = mousePos;
            txtAnimatedText.text = "+" + scoreToAdd;
            scoreAnimator.SetActive(true);
        }
    }

    public int GetScore()
    {
        return Score;
    }

    private IEnumerator SetScore(int lastScore, int currentScore)
    {
        var IterationSize = (currentScore - lastScore) / 10;

        for (var index = 1; index < 10; index++)
        {
            lastScore += IterationSize;
            txtScore.text = string.Format("{0:#,#.}", lastScore);
            yield return new WaitForEndOfFrame();
        }

        txtScore.text = string.Format("{0:#,#.}", currentScore);
        yield return new WaitForSeconds(1F);
        scoreAnimator.SetActive(false);
    }
}