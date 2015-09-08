using UnityEngine;
using System.Collections;

public class loadscence : MonoBehaviour {

	public void NextLevelButton(int index)
	{
		Application.LoadLevel(index);
	}
}
