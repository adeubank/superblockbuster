using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIImageColor : MonoBehaviour
{
    private Image currentImage;
    [SerializeField] private string UIColorTag;

    private void Awake()
    {
        currentImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        UIThemeManager.OnUIThemeChangedEvent += OnUIThemeChangedEvent;
        Invoke("UpdateImageUI", 0.1F);
    }

    private void OnDisable()
    {
        UIThemeManager.OnUIThemeChangedEvent -= OnUIThemeChangedEvent;
    }

    private void OnUIThemeChangedEvent(bool isDarkThemeEnabled)
    {
        UpdateImageUI();
    }

    private void UpdateImageUI()
    {
        if (currentImage != null)
        {
            var tag = UIThemeManager.Instance.currentUITheme.UIStyle.Find(o => o.tagName == UIColorTag);
            if (tag != null) currentImage.color = tag.UIColor;
        }
    }
}