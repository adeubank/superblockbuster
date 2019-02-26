using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UIFontColor : MonoBehaviour
{
    private Text currentText;
    [SerializeField] private string UIColorTag;

    private void Awake()
    {
        currentText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        UIThemeManager.OnUIThemeChangedEvent += OnUIThemeChangedEvent;
        Invoke("UpdateFontUI", 0.1F);
    }

    private void OnDisable()
    {
        UIThemeManager.OnUIThemeChangedEvent -= OnUIThemeChangedEvent;
    }

    private void OnUIThemeChangedEvent(bool isDarkThemeEnabled)
    {
        UpdateFontUI();
    }

    private void UpdateFontUI()
    {
        if (currentText != null)
        {
            var tag = UIThemeManager.Instance.currentUITheme.UIStyle.Find(o => o.tagName == UIColorTag);
            if (tag != null) currentText.color = tag.UIColor;
        }
    }
}