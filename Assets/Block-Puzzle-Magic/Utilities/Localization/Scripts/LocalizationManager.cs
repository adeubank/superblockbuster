using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class LocalizationManager : Singleton<LocalizationManager>
{
    private string currentLangCode = "en";
    public string defaultLangCode = "en";
    public List<Langauge> LanguageList;
    private XDocument xDocLocalizedText;
    private XElement xEleRoot;

    public static event Action<string> OnLanguageChangedEvent;

    private void Awake()
    {
        ///Check if manual langauges is selected by user, untill user selects language manually, device language will be set for app automatically.
        if (PlayerPrefs.HasKey("currentLangCode"))
            currentLangCode = PlayerPrefs.GetString("currentLangCode", defaultLangCode);
        else
            currentLangCode = GetSystemLanguageCode();

        InitLanguageXML(currentLangCode, ref xDocLocalizedText);

        if (xDocLocalizedText != null) xEleRoot = xDocLocalizedText.Element("Resources");
    }

    /// <summary>
    ///     Sets the language.
    /// </summary>
    /// <param name="langCode">Lang code.</param>
    public void SetLanguage(string langCode)
    {
        InitLanguageXML(langCode, ref xDocLocalizedText);

        if (xDocLocalizedText != null)
        {
            xEleRoot = xDocLocalizedText.Element("Resources");
            PlayerPrefs.SetString("currentLangCode", langCode);

            currentLangCode = langCode;
            if (OnLanguageChangedEvent != null) OnLanguageChangedEvent.Invoke(currentLangCode);
        }
        else
        {
            xDocLocalizedText = XDocument.Parse(Resources.Load("strings-" + currentLangCode.ToLower()).ToString());
        }
    }

    /// <summary>
    ///     Gets the current language code.
    /// </summary>
    /// <returns>The current language code.</returns>
    public string getCurrentLanguageCode()
    {
        return currentLangCode;
    }

    public string GetLocalizedTextForTag(string tag)
    {
        if (xEleRoot != null)
        {
            var ele = xEleRoot.Descendants("string").FirstOrDefault(el => el.Attribute("name").Value == tag);

            if (ele != null) return ele.Attribute("text").Value;
        }

        return null;
    }

    /// <summary>
    ///     Gets the system language code.
    /// </summary>
    /// <returns>The system language code.</returns>
    public string GetSystemLanguageCode()
    {
        var SystemLanguageCode = "EN";
        var systemlanguageName = Application.systemLanguage.ToString();

        if (systemlanguageName.Contains("English"))
            SystemLanguageCode = "EN";
        else if (systemlanguageName.Contains("French"))
            SystemLanguageCode = "FR";
        else if (systemlanguageName.Contains("German"))
            SystemLanguageCode = "DE";
        else if (systemlanguageName.Contains("Italian"))
            SystemLanguageCode = "IT";
        else if (systemlanguageName.Contains("Japanese"))
            SystemLanguageCode = "JA";
        else if (systemlanguageName.Contains("Korean"))
            SystemLanguageCode = "KO";
        else if (systemlanguageName.Contains("Portuguese"))
            SystemLanguageCode = "PT";
        else if (systemlanguageName.Contains("Russian"))
            SystemLanguageCode = "RU";
        else if (systemlanguageName.Contains("Spanish"))
            SystemLanguageCode = "ES";
        else if (systemlanguageName.Contains("Swedish"))
            SystemLanguageCode = "SV";
        else if (systemlanguageName.Contains("Turkish")) SystemLanguageCode = "TR";

        return SystemLanguageCode;
    }

    public void InitLanguageXML(string langCode, ref XDocument xDoc)
    {
        var txtFile = Resources.Load("strings-" + langCode.ToLower()) as TextAsset;

        if (txtFile == null) txtFile = Resources.Load("strings-en") as TextAsset;

        xDoc = XDocument.Parse(txtFile.text);
    }
}

public class SpecialTagReplacer
{
    public string replaceValue;
    public string specialTag;

    public SpecialTagReplacer(string _specialTag, string _replaceValue)
    {
        specialTag = _specialTag;
        replaceValue = _replaceValue;
    }
}

[Serializable]
public class Langauge
{
    public Sprite imgFlag;
    public bool isAvailable;
    public string LangaugeCode;
    public string LanguageName;
}