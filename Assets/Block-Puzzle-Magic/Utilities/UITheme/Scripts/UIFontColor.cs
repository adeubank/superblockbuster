using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]

public class UIFontColor : MonoBehaviour 
{
	Text currentText;
	[SerializeField] private string UIColorTag;

	void Awake()
	{
		currentText = GetComponent<Text> ();
	}

	void OnEnable()
	{
		UIThemeManager.OnUIThemeChangedEvent += OnUIThemeChangedEvent;	
		Invoke ("UpdateFontUI", 0.1F);
	}

	void OnDisable()
	{
		UIThemeManager.OnUIThemeChangedEvent -= OnUIThemeChangedEvent;	
	}

	void OnUIThemeChangedEvent (bool isDarkThemeEnabled)
	{
		UpdateFontUI ();
	}

	void UpdateFontUI() 
	{
		if (currentText != null) {
			UIThemeTag tag = UIThemeManager.Instance.currentUITheme.UIStyle.Find (o => o.tagName == UIColorTag);
			if (tag != null) {
				currentText.color = tag.UIColor;
			}
		}
	}
}
