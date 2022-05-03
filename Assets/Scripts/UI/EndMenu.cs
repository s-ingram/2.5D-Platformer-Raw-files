using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndMenu : MonoBehaviour
{
	public void PlayAgain()
	{
		GameManager.LoadMainMenu();
	}

	public void QuitGame()
	{
		GameManager.QuitGame();
	}
}
