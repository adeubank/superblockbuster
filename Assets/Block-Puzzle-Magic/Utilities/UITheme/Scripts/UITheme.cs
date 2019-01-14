using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UI Theme", menuName = "BlockPuzzle/Create UI Theme", order = 1)]
public class UITheme : ScriptableObject  
{
	public List<UIThemeTag> UIStyle;
}

[System.Serializable]
public class UIThemeTag
{
	public string tagName;
	public Color UIColor;
}