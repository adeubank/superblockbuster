using UnityEngine;

public class SelectLanguage : MonoBehaviour
{
    [SerializeField] private GameObject LangaugeButtonTemplate;

    [SerializeField] private Transform langaugeSelectionContent;


    private void Start()
    {
        CreateLanguageList();
    }

    private void CreateLanguageList()
    {
        foreach (var lang in LocalizationManager.Instance.LanguageList)
            if (lang.isAvailable)
                CreateLanguageButton(lang);
    }

    private void CreateLanguageButton(Langauge languauge)
    {
        var languageButton = Instantiate(LangaugeButtonTemplate);
        languageButton.name = "btn-" + languauge.LanguageName;
        languageButton.GetComponent<LanguageButton>().SetLangugaeDetail(languauge);
        languageButton.transform.SetParent(langaugeSelectionContent);
        languageButton.transform.localScale = Vector3.one;
        languageButton.SetActive(true);
    }

    public void OnCloseButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            gameObject.Deactivate();
        }
    }
}