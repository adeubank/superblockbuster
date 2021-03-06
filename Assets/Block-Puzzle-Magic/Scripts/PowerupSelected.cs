using UnityEngine;
using UnityEngine.UI;

public class PowerupSelected : MonoBehaviour
{
    private PowerupBlockSpawn _equippedPowerup;
    public Sprite noPowerupImage;
    [SerializeField] private Button powerupButton;
    public Image powerupImage;

    public void SetPowerup(int equippedPowerupId)
    {
        _equippedPowerup =
            PowerupController.Instance.availablePowerups.powerupBlockSpawns.Find(powerup =>
                powerup.BlockID == equippedPowerupId);
        powerupImage.sprite = _equippedPowerup.powerupActivationSprite.GetComponent<Image>().sprite;
        powerupButton.enabled = true;
        powerupButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayButtonClickSound();
            PowerupSelectMenu.Instance.RemoveEquippedPowerupId(_equippedPowerup.BlockID);
            SetNoPowerup();
            powerupButton.onClick.RemoveAllListeners();
        });
        gameObject.SetActive(true);
    }

    public void SetNoPowerup()
    {
        gameObject.SetActive(false);
        powerupButton.enabled = false;
        powerupImage.sprite = noPowerupImage;
    }
}