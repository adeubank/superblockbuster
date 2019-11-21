using UnityEngine;
using UnityEngine.UI;

public class PowerupOption : MonoBehaviour
{
    [HideInInspector] public PowerupBlockSpawn powerup;
    [SerializeField] public Button powerupButton;
    public Image powerupIcon;
    [SerializeField] private GameObject powerupPopupPrefab;

    public void SetPowerup(PowerupBlockSpawn _powerup)
    {
        powerup = _powerup;
        powerupIcon.sprite = _powerup.powerupActivationSprite.GetComponent<Image>().sprite;
        //region rebind event handlers
        powerupButton.onClick.RemoveAllListeners();
        powerupButton.onClick.AddListener(OpenPowerupModal);
        //endregion
    }

    private void OpenPowerupModal()
    {
        var powerupPopupGameObject = Instantiate(powerupPopupPrefab, PowerupSelectMenu.Instance.transform);
        var powerupPopup = powerupPopupGameObject.GetComponent<PowerupPopup>();
        powerupPopup.SetPowerup(powerup);
    }
}