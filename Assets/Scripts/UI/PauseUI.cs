using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour
{
	public void Resume()
	{
		GameManager.PauseGame(this, EventArgs.Empty);
	}

	public void LoadMainMenu()
	{
		GameManager.LoadMainMenu();
	}

	public void QuitGame()
	{
		GameManager.QuitGame();
	}
}
