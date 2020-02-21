using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public const string PlacementName = "GameOver";

    [SerializeField] private Button btnReplay;

    [SerializeField] private Text txtBestScore;
    [SerializeField] private Text txtCoinReward;

    [SerializeField] private Text txtScore;
    [SerializeField] private Image imgGrade;
    [SerializeField] private Text txtTitle;

    [SerializeField] private GameObject gradeFxPrefab;
    [SerializeField] private Sprite imgGradeC;
    [SerializeField] private Sprite imgGradeB;
    [SerializeField] private Sprite imgGradeA;
    [SerializeField] private Sprite imgGradeS;
    private readonly List<GameObject> _gradeFxGameObjs = new List<GameObject>();
    
    private void OnEnable()
    {
        if (_gradeFxGameObjs.Any())
        {
            _gradeFxGameObjs.ForEach(Destroy);
            _gradeFxGameObjs.Clear();
        }  
        btnReplay.gameObject.SetActive(GameController.GamesPlayed() > 1);

        AdController.Instance.ShowInterstitial();

        InputManager.Instance.EnableTouch();
    }

    public void SetLevelScore(int score)
    {
        SetLevelGrade(score);
        var coinReward = 50;

        if (score >= 100_000)
            coinReward = 75;

        if (score >= 200_000)
            coinReward = 100;

        if (score >= 300_000)
            coinReward = 150;

        if (score >= 500_000)
            coinReward = 200;

        if (score >= 1_000_000)
            coinReward = 250;

        if (score > 1_500_000)
            coinReward = 500;

        var bestScore = PlayerPrefs.GetInt("BestScore_" + GameController.gameMode, score);

        if (score >= bestScore)
        {
            PlayerPrefs.SetInt("BestScore_" + GameController.gameMode, score);
            bestScore = score;
        }

        txtScore.text = string.Format("{0:#,#.}", score.ToString("0"));
        txtBestScore.text = string.Format("{0:#,#.}", bestScore.ToString("0"));
        txtCoinReward.text = string.Format("{0:#,#.}", coinReward.ToString("0"));

        CurrencyManager.Instance.AddCoinBalance(coinReward);
    }

    private void SetLevelGrade(int score)
    {
        var grade = imgGradeC;
        txtTitle.text = "CASUAL";

        if (score >= 500_000)
        {
            grade = imgGradeB;
            txtTitle.text = "BOLD";
        }

        if (score >= 1_000_000)
        {
            grade = imgGradeA;
            txtTitle.text = "AWESOME";
        }

        if (score > 1_500_000)
        {
            grade = imgGradeS;
            var sizeDelta = imgGrade.GetComponent<RectTransform>().sizeDelta;
            var one = Instantiate(gradeFxPrefab, imgGrade.transform);
            var two = Instantiate(gradeFxPrefab, imgGrade.transform);
            var three = Instantiate(gradeFxPrefab, imgGrade.transform);
            one.transform.localPosition = new Vector3(89,95);
            two.transform.localPosition = new Vector3(-55, 35);
            three.transform.localPosition = new Vector3(66, -125);
            one.name = "One-" + one.name;
            two.name = "Two-" + two.name;
            three.name = "Three-" + three.name;
            txtTitle.text = "SUPER";
        }
        
        imgGrade.sprite = grade;
        
    }

    public void OnHomeButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            StackManager.Instance.mainMenu.Activate();
            gameObject.Deactivate();
        }
    }

    public void OnReplayButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            StackManager.Instance.ActivateGamePlay();
            gameObject.Deactivate();
        }
    }
}