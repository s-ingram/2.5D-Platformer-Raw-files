using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
	public static GameManager Instance = null;

	[SerializeField] ScriptableScene sceneList;
	[SerializeField] ScriptablePlaylist playlist;
	AudioSource audioSource;

	UnityEngine.Rendering.Universal.Light2D globalLight;

	public HealthBar healthBarUI;
	public ShieldBar shieldBarUI;
	public AmmoCounter ammoCounterUI;
	public Crosshair crosshairUI;
	public ReloadAnimation reloadUI;

	static GameObject gameUI;
	static GameObject mainMenuUI;
	static GameObject pauseMenuUI;
	static GameObject endMenuUI;
	static GameObject loadingScreen;

	public GameObject charObj;
	static GameObject character;
	static LevelGenerator levelGenerator;
	static int levelIndex = 0;

	// paude code referenced from Brackeys: https://www.youtube.com/watch?v=JivuXdrIHK0&t=3s
	public static bool gamePaused = false;

	public event EventHandler OnEscapeKeyDown, OnLevelGenerated;

	void Awake()
	{
		DontDestroyOnLoad(this);

		if (Instance == null)
			Instance = this;
		else if (Instance != null)
			Destroy(gameObject);

		//	----------------------	//

		UISetup();
		MainMenuUISetup();
		MusicSetup();

		levelGenerator = gameObject.AddComponent<LevelGenerator>();
		globalLight = gameObject.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();

		SceneManager.sceneLoaded += GenerateLevel;
		SceneManager.sceneLoaded += NextTrack;
		OnEscapeKeyDown += PauseGame;
		levelGenerator.OnLevelGenerated += SpawnCharacter;
		levelGenerator.OnLevelGenerated += DeactivateLoadingScreen;

		LoadMainMenu();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			if (OnEscapeKeyDown != null) OnEscapeKeyDown(this, EventArgs.Empty);
	}

	private void UISetup()
	{
		gameUI = new GameObject("GameUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		gameUI.transform.SetParent(this.transform);
		gameUI.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
		gameUI.GetComponent<Canvas>().pixelPerfect = true;
		gameUI.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

		healthBarUI = Instantiate(Resources.Load("UI/HealthBar", typeof(HealthBar)), gameUI.transform) as HealthBar;
		shieldBarUI = Instantiate(Resources.Load("UI/ShieldBar", typeof(ShieldBar)), gameUI.transform) as ShieldBar;
		ammoCounterUI = Instantiate(Resources.Load("UI/Ammo", typeof(AmmoCounter)), gameUI.transform) as AmmoCounter;
		crosshairUI = Instantiate(Resources.Load("UI/Crosshair", typeof(Crosshair)), gameUI.transform) as Crosshair;
		reloadUI = Instantiate(Resources.Load("UI/ReloadAnim", typeof(ReloadAnimation)), gameUI.transform) as ReloadAnimation;
	}

	private void MainMenuUISetup()
	{
		mainMenuUI = Instantiate(Resources.Load("UI/MainMenu", typeof(GameObject)), this.transform) as GameObject;
		pauseMenuUI = Instantiate(Resources.Load("UI/PauseUI", typeof(GameObject)), this.transform) as GameObject;
		endMenuUI = Instantiate(Resources.Load("UI/EndMenu", typeof(GameObject)), this.transform) as GameObject;
		loadingScreen = Instantiate(Resources.Load("UI/LoadingScreen", typeof(GameObject)), this.transform) as GameObject;

		gameUI.SetActive(false);
		pauseMenuUI.SetActive(false);
		endMenuUI.SetActive(false);
		loadingScreen.SetActive(false);
	}

	private static void MusicSetup()
	{
		Instance.audioSource = Instance.gameObject.AddComponent<AudioSource>();
		Instance.audioSource.loop = true;
		Instance.audioSource.volume = 0.1f;
	}

	private static void NextTrack(Scene scene, LoadSceneMode mode)
	{
		Instance.audioSource.clip = Instance.playlist.tracks[SceneManager.GetActiveScene().buildIndex];
		Instance.audioSource.Play();
	}

	private void GenerateLevel(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "MainMenu" || scene.name == "Setup" || scene.name == "EndMenu")
			return;

		StartCoroutine(levelGenerator.GenerateLevel(sceneList.levelParams[levelIndex]));
		globalLight.intensity = sceneList.levelParams[levelIndex].roomGenParams.globalLightIntensity;

		levelIndex++;
	}

	public void SpawnCharacter(object sender, EventArgs args)
	{
		if (character == null)
			character = Instantiate(charObj, LevelGenerator.GetPlayerSpawner().transform.position, Quaternion.identity);
		else
		{
			character.transform.position = LevelGenerator.GetPlayerSpawner().transform.position;

			Instance.StartCoroutine(character.GetComponent<PlayerMovementController>().LockControls(1f));
			character.GetComponent<PlayerMovementController>().ResetMoveDirection();
			character.GetComponent<PlayerAimController>().ResetCamera();
			character.GetComponent<PlayerCharacter>().UpdateHPandShieldUI();
			Input.ResetInputAxes();
		}
	}

	public static void KillCharacter(GameObject character)
	{
		if (character.GetComponent<PlayerCharacter>() != null)
			LoadMainMenu();

		Destroy(character);
	}

	public static void StartGame()
	{
		SceneManager.LoadScene("Tutorial");
		mainMenuUI.SetActive(false);
		gameUI.SetActive(true);
		Cursor.visible = false;
	}

	public static void PauseGame(object sender, EventArgs args)
	{
		if (SceneManager.GetActiveScene().name == "MainMenu")
			return;

		if (gamePaused)
		{
			pauseMenuUI.SetActive(false);
			Time.timeScale = 1f;
			gamePaused = false;
		}
		else
		{
			pauseMenuUI.SetActive(true);
			Time.timeScale = 0f;
			gamePaused = true;
		}
	}

	public static void LoadMainMenu()
	{
		levelIndex = 0;

		Destroy(character);

		mainMenuUI.SetActive(true);
		pauseMenuUI.SetActive(false);
		endMenuUI.SetActive(false);
		Time.timeScale = 1f;
		gamePaused = false;
		Cursor.visible = true;
		SceneManager.LoadScene("MainMenu");
	}

	public static void LoadNextScene()
	{
		//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

		Instance.StartCoroutine(HandleLoadNextScene(SceneManager.GetActiveScene().buildIndex + 1));
	}

	private static IEnumerator HandleLoadNextScene(int sceneIndex)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

		while (!asyncLoad.isDone)
			yield return null;

		if (SceneManager.GetActiveScene().name != "EndMenu")
			ActivateLoadingScreen();
		else
			EndMenu();
	}

	private static void EndMenu()
	{
		Cursor.visible = true;

		endMenuUI.SetActive(true);
		pauseMenuUI.SetActive(false);
		gameUI.SetActive(false);
		Destroy(character);
	}

	private static void ActivateLoadingScreen()
	{
		loadingScreen.SetActive(true);
	}

	private static void DeactivateLoadingScreen(object sender, EventArgs args)
	{
		loadingScreen.SetActive(false);
	}

	public static void QuitGame()
	{
		Application.Quit();
	}

	public static bool IsPaused()
	{ return gamePaused; }
}
