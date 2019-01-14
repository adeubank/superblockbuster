﻿using UnityEngine;
using System.Collections;

public class QuitconfirmGame : MonoBehaviour
{
	/// <summary>
	/// Raises the close button pressed event.
	/// </summary>
	public void OnStayButtonPressed ()
	{
		if (InputManager.Instance.canInput ()) {
			AudioManager.Instance.PlayButtonClickSound ();
			gameObject.Deactivate();
		}
	}

	/// <summary>
	/// Raises the ok button pressed event.
	/// </summary>
	public void OnQuitButtonPressed ()
	{
		if (InputManager.Instance.canInput ()) {
			AudioManager.Instance.PlayButtonClickSound ();
			GameController.Instance.QuitGame ();
		}
	}
}
