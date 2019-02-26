using System;
using UnityEngine;

public class UIThemeManager : Singleton<UIThemeManager>
{
    /// <summary>
    ///     The current user interface theme.
    /// </summary>
    public UITheme currentUITheme;

    /// <summary>
    ///     The is dark theme enabled.
    /// </summary>
    [HideInInspector] public bool isDarkThemeEnabled = true;

    /// <summary>
    ///     Occurs when on user interface theme changed event.
    /// </summary>
    public static event Action<bool> OnUIThemeChangedEvent;

    /// <summary>
    ///     Raises the enable event.
    /// </summary>
    private void OnEnable()
    {
        initThemeStatus();
    }

    /// <summary>
    ///     Inits the theme status.
    /// </summary>
    public void initThemeStatus()
    {
        isDarkThemeEnabled = PlayerPrefs.GetInt("isDarkThemeEnabled", 0) == 0 ? true : false;

        if (isDarkThemeEnabled)
            currentUITheme = (UITheme) Resources.Load("UI Theme-1");
        else
            currentUITheme = (UITheme) Resources.Load("UI Theme-2");

        if (!isDarkThemeEnabled && OnUIThemeChangedEvent != null) OnUIThemeChangedEvent.Invoke(isDarkThemeEnabled);
    }

    /// <summary>
    ///     Toggles the theme status.
    /// </summary>
    public void ToggleThemeStatus()
    {
        isDarkThemeEnabled = isDarkThemeEnabled ? false : true;

        if (isDarkThemeEnabled)
            currentUITheme = (UITheme) Resources.Load("UI Theme-1");
        else
            currentUITheme = (UITheme) Resources.Load("UI Theme-2");

        PlayerPrefs.SetInt("isDarkThemeEnabled", isDarkThemeEnabled ? 0 : 1);

        if (OnUIThemeChangedEvent != null) OnUIThemeChangedEvent.Invoke(isDarkThemeEnabled);
    }
}