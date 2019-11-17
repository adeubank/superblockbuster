using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PowerupSelectMenu : Singleton<PowerupSelectMenu>
{
    private Sequence _helpIconLoopSequence;
    private Sequence _showScrollableSequence;

    public GameObject helpIcon;
    public Scrollbar powerupMenuScrollView;
    public GameObject powerupOptionPrefab;
    public Transform powerupOptionsListTransform;
    public Transform powerupSelectedListTransform;
    public GameObject powerupSelectedPrefab;
    public GameObject powerupsEquippedTxt;
    public GameObject powerupsMenuHelpPrefab;
    public GameObject powerupsNoneEquippedTxt;

    // Start is called before the first frame update
    private void Start()
    {
        InitMenuOptions();
    }

    public void InitMenuOptions()
    {
        PowerupController.Instance.LoadSavedPurchasedPowerups();
        PowerupController.Instance.LoadSavedEquippedPowerups();
        UpdateMenu();
    }

    private void UpdateMenu()
    {
        StopHelp();
        UpdateAvailablePowerups();
        UpdateEquippedPowerups();
        SaveData();
    }

    private void StopHelp()
    {
        helpIcon.Deactivate();

        if (_showScrollableSequence != null && _showScrollableSequence.IsPlaying())
        {
            _showScrollableSequence.Kill(true);
            _showScrollableSequence = null;
        }

        if (_helpIconLoopSequence != null && _helpIconLoopSequence.IsPlaying()) _helpIconLoopSequence.Pause();
    }

    private void SaveData()
    {
        PowerupController.Instance.SaveEquippedPowerups();
        PowerupController.Instance.SavePurchasedPowerups();
    }

    private void UpdateAvailablePowerups()
    {
        // clean the list first
        foreach (Transform t in powerupOptionsListTransform)
        {
            if (t == powerupOptionsListTransform) continue;
            Destroy(t.gameObject);
        }

        PowerupOption firstPowerupOption = null;
        foreach (var powerupBlockSpawn in PowerupController.Instance.availablePowerups.powerupBlockSpawns.OrderBy(PowerupPopup.PriceForPowerup))
        {
            var powerupOption = Instantiate(powerupOptionPrefab, powerupOptionsListTransform)
                .GetComponent<PowerupOption>();

            if (firstPowerupOption == null) firstPowerupOption = powerupOption;

            powerupOption.SetPowerup(powerupBlockSpawn);
        }

        Invoke("CheckPowerupMenuHelp", 1f);
    }

    public void CheckPowerupMenuHelp()
    {
        ActivatePowerupMenuHelp(false);
    }

    public void ActivatePowerupMenuHelp(bool force = true)
    {
        _helpIconLoopSequence?.Pause();
        StartCoroutine(ShowPowerupMenuScrollableHelp(force));
    }

    private IEnumerator ShowPowerupMenuScrollableHelp(bool force = true)
    {
        if (force || !PlayerPrefs.HasKey("powerup_menu_scrollable_help") || PlayerPrefs.GetInt("powerup_menu_scrollable_help") != 1)
        {
            var helpPopup = Instantiate(powerupsMenuHelpPrefab, transform);
            yield return new WaitUntil(() => helpPopup == null);
            helpIcon.SetActive(true);
            yield return new WaitForEndOfFrame();
            // show view is scrollable
            _showScrollableSequence = DOTween.Sequence();
            var origHelpIconY = helpIcon.transform.localPosition.y;
            _showScrollableSequence.Insert(0, helpIcon.transform.DOLocalMoveY(origHelpIconY + -origHelpIconY, 1.0f));
            _showScrollableSequence.Insert(0, DOTween.To(() => powerupMenuScrollView.value, value => powerupMenuScrollView.value = value, 0.75f, 1.0f));
            _showScrollableSequence.AppendInterval(1f);
            _showScrollableSequence.Insert(2, helpIcon.transform.DOLocalMoveY(origHelpIconY, 1.0f));
            _showScrollableSequence.Insert(2, DOTween.To(() => powerupMenuScrollView.value, value => powerupMenuScrollView.value = value, 1, 1.0f));
            _showScrollableSequence.AppendCallback(() =>
            {
                helpIcon.Deactivate();
                powerupMenuScrollView.value = 1;
                StartCoroutine(ShowSelectPowerupHelp());
            });
            PlayerPrefs.SetInt("powerup_menu_scrollable_help", 1);
        }
        else if (PowerupController.Instance.equippedPowerupIds.Count == 0)
        {
            StartCoroutine(ShowSelectPowerupHelp());
        }

        yield return null;
    }

    private IEnumerator ShowSelectPowerupHelp()
    {
        if (PowerupController.Instance.equippedPowerupIds.Any()) yield break;

        // let unity catchup
        yield return new WaitForEndOfFrame();

        var firstOptionPosition = (Vector2) powerupOptionsListTransform.GetChild(1).transform.position - new Vector2(0, 40f);
        var secondOptionPosition = firstOptionPosition - new Vector2(0, 30f);
        helpIcon.transform.position = firstOptionPosition;

        helpIcon.SetActive(true);
        yield return new WaitForEndOfFrame();

        if (_helpIconLoopSequence != null)
        {
            _helpIconLoopSequence.Restart();
        }
        else
        {
            _helpIconLoopSequence = DOTween.Sequence();
            _helpIconLoopSequence.Append(helpIcon.transform.DOMove(secondOptionPosition, 0.4F).SetDelay(0.2f));
            _helpIconLoopSequence.Append(helpIcon.transform.DOMove(firstOptionPosition, 0.4F).SetDelay(0.2f));
            _helpIconLoopSequence.SetLoops(-1, LoopType.Restart);
        }
    }

    private void UpdateEquippedPowerups()
    {
        Debug.Log("Initializing equipped powerups list " + PowerupController.Instance.equippedPowerupIds);

        if (PowerupController.Instance.equippedPowerupIds.Any())
        {
            powerupsEquippedTxt.Activate();
            powerupsNoneEquippedTxt.Deactivate();
        }
        else
        {
            powerupsEquippedTxt.Deactivate();
            powerupsNoneEquippedTxt.Activate();
        }

        // clean the list first
        foreach (Transform t in powerupSelectedListTransform)
        {
            if (t == powerupSelectedListTransform) continue;
            Destroy(t.gameObject);
        }

        for (var i = 0; i < 3; i++)
        {
            var powerupSelected = Instantiate(powerupSelectedPrefab, powerupSelectedListTransform)
                .GetComponent<PowerupSelected>();
            if (i < PowerupController.Instance.equippedPowerupIds.Count)
                powerupSelected.SetPowerup(PowerupController.Instance.equippedPowerupIds[i]);
            else
                powerupSelected.SetNoPowerup();
        }
    }

    public bool AddEquippedPowerupId(PowerupBlockSpawn powerup, bool updateMenu = true)
    {
        PowerupController.Instance.AddEquippedPowerupId(powerup.BlockID);

        if (updateMenu) UpdateMenu();

        return true;
    }

    public bool RemoveEquippedPowerupId(int equippedPowerup, bool updateMenu = true)
    {
        PowerupController.Instance.RemoveEquippedPowerupId(equippedPowerup);
        if (updateMenu) UpdateMenu();
        return true;
    }

    public void OnPlayButtonClicked()
    {
        AudioManager.Instance.PlayButtonClickSound();
        GameController.gameMode = GameMode.TIMED;
        StackManager.Instance.ActivateGamePlay();
        StackManager.Instance.mainMenu.Deactivate();
        gameObject.Deactivate();
    }

    public void OnCloseButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            gameObject.Deactivate();
        }
    }


    public bool AddPurchasedPowerupId(PowerupBlockSpawn purchasedPowerup, bool updateMenu = true)
    {
        PowerupController.Instance.AddPurchasedPowerupId(purchasedPowerup.BlockID);
        if (updateMenu) UpdateMenu();

        return true;
    }
}