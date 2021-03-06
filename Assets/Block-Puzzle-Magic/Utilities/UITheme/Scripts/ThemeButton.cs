using UnityEngine;
using UnityEngine.UI;

public class ThemeButton : MonoBehaviour
{
    /// The button theme.
    public Button btnTheme;

    /// The button theme image.
    public Image btnThemeImage;

    /// The night mode off sprite.
    public Sprite nightModeOffSprite;

    /// The night mode on sprite.
    public Sprite nightModeOnSprite;

    /// <summary>
    ///     Start this instance.
    /// </summary>
    private void Start()
    {
        btnTheme.onClick.AddListener(() =>
        {
            if (InputManager.Instance.canInput()) UIThemeManager.Instance.ToggleThemeStatus();
        });
    }

    /// <summary>
    ///     Raises the enable event.
    /// </summary>
    private void OnEnable()
    {
        UIThemeManager.OnUIThemeChangedEvent += OnUIThemeChangedEvent;
        initMusicStatus();
    }

    /// <summary>
    ///     Raises the disable event.
    /// </summary>
    private void OnDisable()
    {
        UIThemeManager.OnUIThemeChangedEvent -= OnUIThemeChangedEvent;
    }

    /// <summary>
    ///     Inits the music status.
    /// </summary>
    private void initMusicStatus()
    {
        btnThemeImage.sprite = AudioManager.Instance.isMusicEnabled ? nightModeOnSprite : nightModeOffSprite;
    }

    /// <summary>
    ///     Raises the user interface theme changed event event.
    /// </summary>
    /// <param name="isDarkThemeEnabled">If set to <c>true</c> is dark theme enabled.</param>
    private void OnUIThemeChangedEvent(bool isDarkThemeEnabled)
    {
        btnThemeImage.sprite = isDarkThemeEnabled ? nightModeOnSprite : nightModeOffSprite;
    }
}