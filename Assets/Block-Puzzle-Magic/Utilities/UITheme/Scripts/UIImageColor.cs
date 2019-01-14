using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]

public class UIImageColor : MonoBehaviour 
{
	Image currentImage;
	[SerializeField] private string UIColorTag;

	void Awake()
	{
		currentImage = GetComponent<Image> ();
	}

	void OnEnable()
	{
		UIThemeManager.OnUIThemeChangedEvent += OnUIThemeChangedEvent;	
		Invoke ("UpdateImageUI", 0.1F);
	}

	void OnDisable()
	{
		UIThemeManager.OnUIThemeChangedEvent -= OnUIThemeChangedEvent;	
	}

	void OnUIThemeChangedEvent (bool isDarkThemeEnabled)
	{
		UpdateImageUI ();
	}

	void UpdateImageUI() 
	{
		if (currentImage != null) 
		{
			UIThemeTag tag = UIThemeManager.Instance.currentUITheme.UIStyle.Find (o => o.tagName == UIColorTag);
			if (tag != null) {
				currentImage.color = tag.UIColor;
			}
		}
	}
}
