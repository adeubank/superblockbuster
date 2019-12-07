using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PowerupPopup : MonoBehaviour
{
    private const string LOCALE_POWERUP_DESC_PREFIX = "powerup-desc-";

    [SerializeField] private PowerupList _powerupList;
    [HideInInspector] public PowerupBlockSpawn powerup;
    [SerializeField] private Button powerupButton;
    public GameObject powerupCoinIcon;
    public Text powerupDescription;
    public Text powerupEquipText;
    public Transform powerupIcon;
    public Text powerupName;
    public Text powerupPriceText;

    public void Awake()
    {
        InputManager.Instance.EnableTouch();
    }

    public void SetPowerup(PowerupBlockSpawn _powerup)
    {
        powerup = _powerup;
        var purchased = PowerupController.Instance.purchasedPowerupIds.Contains(powerup.BlockID);
        var equipped = PowerupController.Instance.equippedPowerupIds.Contains(powerup.BlockID);

        #region Create example powerup block

        var powerupPrefab = _powerupList.powerupBlockSpawns.First(b => b.BlockID == powerup.BlockID).shapeBlock;
        var powerupBlock = Instantiate(powerupPrefab, powerupIcon, true);
        powerupBlock.GetComponent<Canvas>().sortingOrder = 11; // greater than popup for some reason
        var newPowerupTransform = powerupBlock.GetComponent<RectTransform>();
        newPowerupTransform.localPosition = Vector3.zero;
        newPowerupTransform.localScale = Vector2.one;
        newPowerupTransform.sizeDelta = new Vector2(100, 100);
        foreach (RectTransform childTransform in newPowerupTransform)
        {
            if (childTransform == newPowerupTransform) continue;
            childTransform.sizeDelta = new Vector2(100, 100);
        }

        #endregion

        #region update pricing

        powerupEquipText.gameObject.SetActive(purchased);
        powerupCoinIcon.SetActive(!purchased);
        powerupPriceText.gameObject.SetActive(!purchased);
        powerupPriceText.text = PriceForPowerup(_powerup).ToString();

        #endregion

        #region update powerup description

        var localizedText = LocalizationManager.Instance.GetLocalizedTextForTag(LOCALE_POWERUP_DESC_PREFIX + powerup.BlockID);
        if (string.IsNullOrEmpty(localizedText))
            powerupDescription.gameObject.Deactivate();
        else
            powerupDescription.text = localizedText;

        #endregion

        #region rebind event handlers

        powerupButton.onClick.RemoveAllListeners();
        if (equipped)
            powerupButton.onClick.AddListener(UnequipThisPowerup);
        else if (purchased)
            powerupButton.onClick.AddListener(EquipThisPowerup);
        else
            powerupButton.onClick.AddListener(BuyThisPowerup);

        #endregion

        var powerupNameSplit = _powerup.shapeBlock.name.Split('-');
        if (powerupNameSplit.Length == 3)
            powerupName.text = powerupNameSplit[2].ToUpper();
        else
            throw new Exception("Failed to parse powerup name from " + _powerup.shapeBlock.name);
    }

    public static int PriceForPowerup(PowerupBlockSpawn powerup)
    {
        switch (powerup.BlockID)
        {
            #region tier 1 powerups

            case (int) ShapeInfo.Powerups.Doubler:
                return 100;
            case (int) ShapeInfo.Powerups.ColorCoder:
                return 100;
            case (int) ShapeInfo.Powerups.Bandage:
                return 200;
            case (int) ShapeInfo.Powerups.Bomb:
                return 200;
            case (int) ShapeInfo.Powerups.SticksGalore:
                return 200;

            #endregion

            #region tier 2 powerups

            case (int) ShapeInfo.Powerups.Flood:
                return 500;
            case (int) ShapeInfo.Powerups.Dandelion:
                return 750;
            case (int) ShapeInfo.Powerups.Quake:
                return 500;

            #endregion

            #region tier 3 powerups

            case (int) ShapeInfo.Powerups.Frenzy:
                return 1500;
            case (int) ShapeInfo.Powerups.Storm:
                return 1500;
            case (int) ShapeInfo.Powerups.Lag:
                return 2000;
            case (int) ShapeInfo.Powerups.Avalanche:
                return 2000;

            #endregion

            default:
                throw new NotImplementedException("Price for _powerup is not implemented. " + powerup.BlockID);
        }
    }


    private void BuyThisPowerup()
    {
        if (InputManager.Instance.canInput())
        {
            var coinDeduced = CurrencyManager.Instance.deductBalance(PriceForPowerup(powerup));

            if (coinDeduced)
            {
                AudioManager.Instance.PlayButtonClickSound();
                PowerupSelectMenu.Instance.AddPurchasedPowerupId(powerup);
                PowerupSelectMenu.Instance.AddEquippedPowerupId(powerup);
                Destroy(gameObject);
            }
            else
            {
                AudioManager.Instance.PlayButtonCancelSound();
                var sequence = DOTween.Sequence();
                sequence.Append(transform.DOLocalMoveX(15, 0.1f));
                sequence.Append(transform.DOLocalMoveX(-15, 0.1f));
                sequence.Append(transform.DOLocalMoveX(30, 0.1f));
                sequence.Append(transform.DOLocalMoveX(0, 0.1f));
            }
        }
    }

    private void EquipThisPowerup()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            PowerupSelectMenu.Instance.AddEquippedPowerupId(powerup);
            Destroy(gameObject);
        }
    }

    private void UnequipThisPowerup()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            PowerupSelectMenu.Instance.RemoveEquippedPowerupId(powerup.BlockID);
            Destroy(gameObject);
        }
    }
}