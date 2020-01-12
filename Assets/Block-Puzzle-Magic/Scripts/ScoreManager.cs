using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    private int bestScore = 0;
    [SerializeField] private Color highScoreColor;
    [SerializeField] private Color lowScoreColor;
    [SerializeField] private Color mediumScoreColor;
    [SerializeField] private Color minusScoreColor;

    [HideInInspector] public int Score;
    [SerializeField] private GameObject scoreTextAlertPrefab;
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
        if (scoreToAdd == 0) return;

        var oldScore = Score;
        Score += scoreToAdd;
        
        StartCoroutine(SetScore(oldScore, Score));

        if (doAnimate)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            StartCoroutine(AddScoreAlert(scoreToAdd, mousePos));
        }
    }

    private IEnumerator AddScoreAlert(int newScore, Vector3 position)
    {
        var scoreTextAlertGameObject = Instantiate(scoreTextAlertPrefab, transform, false);
        var scoreTextAlert = scoreTextAlertGameObject.GetComponent<ScoreTextAlert>();
        var baseDelay = 0.1f;

        if (newScore >= 0)
        {
            scoreTextAlert.transform.position = position;
            scoreTextAlert.scoreText.text = "+" + newScore;
        }
        else
        {
            var transform1 = txtScore.transform;
            var position1 = transform1.position;
            var newPosition = new Vector3(position1.x, position1.y - 1, 0);
            scoreTextAlert.transform.position = newPosition;
            scoreTextAlert.scoreText.text = newScore.ToString();
        }

        if (newScore >= 10_000)
            scoreTextAlert.scoreText.color = highScoreColor;
        else if (newScore >= 1000)
            scoreTextAlert.scoreText.color = mediumScoreColor;
        else if (newScore > 0)
            scoreTextAlert.scoreText.color = lowScoreColor;
        else
            scoreTextAlert.scoreText.color = minusScoreColor;

        yield return new WaitForFixedUpdate();

        var scoreTextAlertSequence = DOTween.Sequence();
        if (newScore >= 0)
            scoreTextAlertSequence.AppendInterval(baseDelay);
        else
            scoreTextAlertSequence.AppendInterval(baseDelay * 8);
        scoreTextAlertSequence.Append(scoreTextAlert.transform.DOMove(new Vector3(scoreTextAlert.transform.position.x, position.y + 3, 0), baseDelay * 8));
        scoreTextAlertSequence.Join(scoreTextAlert.scoreText.DOFade(0, baseDelay * 8));
        scoreTextAlertSequence.AppendCallback(() => { Destroy(scoreTextAlert.gameObject); });
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
    }
}