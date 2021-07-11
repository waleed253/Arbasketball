using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public static GameController data;

	public GameObject panelStart;
	public GameObject panelVictory;
	public GameObject panelGameOver;

	public bool isPlaying;
	public enum State {InGame, Paused, Complete, StartUp}
	public State gameState;

	public UnityEvent OnGameStart;
	public UnityEvent OnGameComplete;
	private float timeScale = 1.5f;
	
	void Awake () 
	{
		data = this;
		Time.timeScale = timeScale;        //this is used for slow-motion effect
		gameState = State.StartUp;           //The maximum time a frame can spend on particle updates. If the frame takes longer
											     //     than this, then updates are split into multiple smaller updates.
		ShowStartPanel();
	}

	void ShowStartPanel()
	{ 
		panelStart.SetActive(true); 
	}

	void HideStartPanel()
	{
		if(panelStart.activeInHierarchy)
		{
			panelStart.SetActive(false);
		}
	}

	public void StartPlay()
	{
		isPlaying = true;
		gameState = State.InGame;

		HideStartPanel();

		OnGameStart.Invoke();
	}
	
	void Complete()
	{
		isPlaying = false;
		gameState = State.Complete;
		SoundController.data.playGameOver();


		OnGameComplete.Invoke();
	}

	public void Victory()
	{
		if (gameState != State.Complete) 
		{
			Complete();
			panelVictory.SetActive(true);
		}
	}

	public void GameOver()
	{
		if (gameState != State.Complete) 
		{
			Complete();
			panelGameOver.SetActive(true);
		}
	}
	
	public void Restart()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
