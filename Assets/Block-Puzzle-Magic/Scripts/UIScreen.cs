using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScreen : MonoBehaviour 
{
	void Awake()
	{
		init ();	
	}

	public virtual void init()
	{
		//StackManager.Instance.PushWindow (gameObject);
	}
}
