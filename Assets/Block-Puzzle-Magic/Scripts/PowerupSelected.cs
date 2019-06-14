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
            PowerupSelectMenu.Instance.availablePowerups.powerupBlockSpawns.Find(powerup =>
                powerup.BlockID == equippedPowerupId);
        powerupImage.sprite = _equippedPowerup.powerupBlockIcon.GetComponent<Image>().sprite;
        powerupButton.enabled = true;
        powerupButton.onClick.AddListener(() =>
        {
            PowerupSelectMenu.Instance.RemoveEquippedPowerupId(_equippedPowerup.BlockID);
            SetNoPowerup();
            powerupButton.onClick.RemoveAllListeners();
        });
    }

    public void SetNoPowerup()
    {
        powerupButton.enabled = false;
        powerupImage.sprite = noPowerupImage;
    }
}