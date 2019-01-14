using UnityEngine;
using UnityEngine.UI;

public class MusicButton : MonoBehaviour
{
    // The button to toggle music, assigned from inspector.
    public Button btnMusic;

    // The image of the button.
    public Image btnMusicImage;

    // The off sprite for music.
    public Sprite musicOffSprite;

    // The On sprite for music.
    public Sprite musicOnSprite;

    /// <summary>
    ///     Start this instance.
    /// </summary>
    private void Start()
    {
        btnMusic.onClick.AddListener(() =>
        {
            if (InputManager.Instance.canInput())
            {
                AudioManager.Instance.PlayButtonClickSound();
                AudioManager.Instance.ToggleMusicStatus();
            }
        });
    }

    /// <summary>
    ///     Raises the enable event.
    /// </summary>
    private void OnEnable()
    {
        AudioManager.OnMusicStatusChangedEvent += OnMusicStatusChanged;
        initMusicStatus();
    }

    /// <summary>
    ///     Raises the disable event.
    /// </summary>
    private void OnDisable()
    {
        AudioManager.OnMusicStatusChangedEvent -= OnMusicStatusChanged;
    }

    /// <summary>
    ///     Inits the music status.
    /// </summary>
    private void initMusicStatus()
    {
        btnMusicImage.sprite = AudioManager.Instance.isMusicEnabled ? musicOnSprite : musicOffSprite;
    }

    /// <summary>
    ///     Raises the music status changed event.
    /// </summary>
    /// <param name="isMusicEnabled">If set to <c>true</c> is music enabled.</param>
    private void OnMusicStatusChanged(bool isMusicEnabled)
    {
        btnMusicImage.sprite = isMusicEnabled ? musicOnSprite : musicOffSprite;
    }
}