using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    private Text thisText;
    public string txtTag;

    private void Awake()
    {
        thisText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChangedEvent += OnLanguageChangedEvent;
        Invoke("SetLocalizedText", 0.01F);
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChangedEvent -= OnLanguageChangedEvent;
    }

    public void SetLocalizedText()
    {
        if (!string.IsNullOrEmpty(txtTag.Trim())) thisText.SetLocalizedTextForTag(txtTag);
    }

    private void OnLanguageChangedEvent(string langCode)
    {
        SetLocalizedText();
    }
}