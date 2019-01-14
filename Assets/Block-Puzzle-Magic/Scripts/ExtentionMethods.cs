using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Extention methods.
/// </summary>
public static class ExtentionMethods
{
	/// <summary>
	/// This is used for windows transition animation when new screen is entered.
	/// </summary>
	/// <param name="target">Target.</param>
	public static bool OnWindowActivateLoad (this GameObject target)
	{
		PopupAnimation transition = target.GetComponent<PopupAnimation> ();
		if (transition != null) {
			transition.OnWindowAdded ();
			return true;
		}
		return false;
	}

	/// <summary>
	/// This is used for windows transition animation when new current screen is getting removed.
	/// </summary>
	/// <param name="target">Target.</param>
	public static bool OnWindowRemove (this GameObject target)
	{
		PopupAnimation transition = target.GetComponent<PopupAnimation> ();
		if (transition != null) {
			transition.OnWindowRemove ();
			return true;
		}
		return false;
	}

	/// <summary>
	/// Shuffle the specified list.
	/// </summary>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>

	public static void Activate(this GameObject target)
	{
		target.gameObject.SetActive(true);
		target.transform.SetAsLastSibling();
		PopupAnimation transition = target.GetComponent<PopupAnimation> ();
		if (transition != null) {
			transition.OnWindowAdded();
		}

		StackManager.Instance.Push(target.name);
	}

	public static void Deactivate(this GameObject target)
	{
		PopupAnimation transition = target.GetComponent<PopupAnimation> ();
		if (transition != null) {
			transition.OnWindowRemove ();
		}
		else {
			target.SetActive(false);
		}
		StackManager.Instance.Pop(target.name);
	}
}
