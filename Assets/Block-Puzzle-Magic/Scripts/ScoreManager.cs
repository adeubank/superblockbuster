using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    private int bestScore = 0;
    [SerializeField] private Color lowScoreColor;
    [SerializeField] private Color mediumScoreColor;
    [SerializeField] private Color highScoreColor;
    [SerializeField] private Color minusScoreColor;

    [HideInInspector] public int Score;
    [SerializeField] private GameObject scoreTextAlertPrefab;
    [SerializeField] private Text txtBestScore;
    [SerializeField] private Text txtScore;

    [SerializeField] private GameObject comboMultiplier3XPrefab;
    [SerializeField] private GameObject comboMultiplier4XPrefab;
    [SerializeField] private GameObject comboMultiplier5XPrefab;
    [SerializeField] private GameObject comboMultiplierMegaPrefab;

    private void Start()
    {
        txtScore.text = Score.ToString();
        var bestScore = PlayerPrefs.GetInt("BestScore_" + GameController.gameMode, Score);
        txtBestScore.text = bestScore.ToString();
    }

    public void AddScore(int scoreToAdd, int comboMultiplier = 1, bool doAnimate = true)
    {
        if (scoreToAdd == 0) return;

        var oldScore = Score;
        var newScore = scoreToAdd * comboMultiplier;
        Score += newScore;

        StartCoroutine(SetScore(oldScore, Score));

        if (doAnimate)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            StartCoroutine(AddScoreAlert(newScore, comboMultiplier, mousePos));
        }
    }

    private IEnumerator AddScoreAlert(int newScore, int comboMultiplier, Vector3 position)
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

        if (newScore >= 100_000)
            scoreTextAlert.scoreText.color = highScoreColor;
        else if (newScore >= 10_000)
            scoreTextAlert.scoreText.color = mediumScoreColor;
        else if (newScore >= 1000)
            scoreTextAlert.scoreText.color = lowScoreColor;
        else if (newScore > 0)
            scoreTextAlert.scoreText.color = Color.white;
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

        StartCoroutine(SpawnComboMultiplierAlert(comboMultiplier));
    }

    private IEnumerator SpawnComboMultiplierAlert(int comboMultiplier)
    {
        var comboMultiplierPrefab = comboMultiplerPrefabFromInt(comboMultiplier);
        if (comboMultiplierPrefab == null) yield break;
        var comboMultiplierAlert = Instantiate(comboMultiplierPrefab, transform.position + new Vector3(0, 1000, 0), Quaternion.identity, transform);
        yield return new WaitForFixedUpdate(); // let unity catch up
        var outOfMovesAlertSequence = DOTween.Sequence();
        outOfMovesAlertSequence.Join(comboMultiplierAlert.transform.DOLocalMove(new Vector3(0, 254, 0), 0.8f));
        if (comboMultiplier > 5)
        {
            var backgroundImg = comboMultiplierAlert.GetComponent<Image>();
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(lowScoreColor, 0.1f));
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(mediumScoreColor, 0.1f));
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(highScoreColor, 0.1f));
            
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(lowScoreColor, 0.1f));
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(mediumScoreColor, 0.1f));
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(highScoreColor, 0.1f));
            
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(lowScoreColor, 0.1f));
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(mediumScoreColor, 0.1f));
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(highScoreColor, 0.1f));
            
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(lowScoreColor, 0.1f));
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(mediumScoreColor, 0.1f));
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(highScoreColor, 0.1f));
            
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(lowScoreColor, 0.1f));
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(mediumScoreColor, 0.1f));
            outOfMovesAlertSequence.Append(backgroundImg.DOColor(highScoreColor, 0.1f));
        }
        else
        {
            outOfMovesAlertSequence.AppendInterval(1.5f);
        }
        outOfMovesAlertSequence.Append(comboMultiplierAlert.transform.DOLocalMove(new Vector3(0, 1000, 0), 0.2f));
        outOfMovesAlertSequence.AppendCallback(() => { Destroy(comboMultiplierAlert); });
        yield return outOfMovesAlertSequence.WaitForCompletion();
    }

    private GameObject comboMultiplerPrefabFromInt(int comboMultiplier)
    {
        if (comboMultiplier <= 2) return null;
        switch (comboMultiplier)
        {
            case 3:
                // spawn 3x
                return comboMultiplier3XPrefab;

            case 4:
                // spawn 4x
                return comboMultiplier4XPrefab;

            case 5:
                // spawn 5x
                return comboMultiplier5XPrefab;

            default:
                // spawn mega
                return comboMultiplierMegaPrefab;
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
    }
}