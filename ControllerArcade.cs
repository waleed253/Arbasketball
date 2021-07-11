using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (GameController))]
public class ControllerArcade : MonoBehaviour 
{	
	public int startBallsCount = 20;				//Balls count we start with
	public int ballsForVictory = 3;
	private int currentGoalCount = 0;
	private int bonusRingClearCombo = 3;				//Count of clear goals in a row to get big rinf bonus
	private int bonusAimCombo = 5;					//Count of goals in a row to get aim bonus
	private int bonusAimMinXpLevel = 5;				//Minimum xp level to be able get aim bonus
	private int bonusRingThrowsLimit = 3;			//When you get big ring bonus after this count of throws it will gone
	private int bonusAimThrowsLimit = 3;				//When you get aim bonus after this count of throws it will gone 
	private int xpScoreStep = 100;					//XP step. With help of this you can tweak the speed of getting xp level.

	public Text ballsLeft;
	public Text aimOfGame;
	public Text plusScoreTxt;
	public Text plusBallTxt;
	public GameObject ring;
	private AudioSource thisAudio;
	
	private int currentBallsCount;					
	private int score;								//Current score
	private int comboScore;							//Current amount of combo score. Increases when you have goals in a row. Resets when you fail a ball.
	private int comboGoals;							//Current quantity of usual goals got in a row. Increases when you have goals in a row. Resets when you fail a ball.
	private int comboClearGoals;					//Current quantity of clear goals got in a row. Increases when you have clear goals in a row. Resets when you fail a ball.
	private int comboGoals_bonusRing;				//Current quantity of clear goals got in a row to open ring bonus. Increases when you have clear goals in a row. Resets when you fail a ball.
	private bool bonusRingActive;					//Boolean to determine if ring bonus currently active or not
	private int bonusRingThrows;					//Current quantity of balls thrown during ring bonus active
	private int comboGoals_bonusAim;				//Current quantity of goals got in a row to open aim bonus. Increases when you have goals in a row. Resets when you fail a ball.
	private bool bonusAimActive;					//Boolean to determine if aim bonus currently active or not
	private int bonusAimThrows;						//Current quantity of balls thrown during aim bonus active or not
	private bool bonusSuperBallActive;				//Boolean to determine if superball bonus active or not
	private float superBallProgress;				//Float that keeps current superball progress value
	private int xpLevel;							//Int that defines current XP level 
	private int lastRecord;							//Int that keeps current best score 
	private bool hitRecord;							//Boolean that defines if we already hitted last best score or not
	
	
	void OnEnable()
	{
		BallControl.OnGoal += Goal;
        BallControl.OnFail += Fail;
	}
	
	void OnDisable()
	{
		BallControl.OnGoal -= Goal;
        BallControl.OnFail -= Fail;
	}
	
	void Start()
	{
		thisAudio = GetComponent<AudioSource>();
		currentBallsCount = startBallsCount;
		ResetData();
	}
	
	void Goal(float distance, float height, bool floored, bool clear, bool special)
	{
		comboGoals++;
		superBallProgress += 0.01f;

		currentGoalCount++;
		aimOfGame.text = (ballsForVictory - currentGoalCount).ToString();

		if(!bonusAimActive) 
		{
			if(xpLevel > bonusAimMinXpLevel)
				comboGoals_bonusAim += 1;
			
			if(comboGoals_bonusAim == bonusAimCombo) 
			{
				bonusAimActive = true;
				thisAudio.PlayOneShot(SoundController.data.bonusOpen);
			}
		} 
		else 
		{
			bonusAimThrows += 1;

			if(bonusAimThrows == bonusAimThrowsLimit) 
			{
				bonusAimThrows = comboGoals_bonusAim = 0;
				bonusAimActive = false;
			}	
		}
		
		if(bonusRingActive)
		{
			bonusRingThrows +=1;
			if(bonusRingThrows == bonusRingThrowsLimit) 
			{
				comboGoals_bonusRing = 0;
				StartCoroutine(ResetRing());
			}
		}
		
		if(clear) 
		{
			plusBallTxt.gameObject.SetActive(true);
			comboClearGoals += 1;
			
			if(!bonusRingActive) 
			{
				comboGoals_bonusRing +=1;
				if(comboGoals_bonusRing == bonusRingClearCombo) 
				{
					StartCoroutine(GrowRing());
				}
			}

			if(special)
				SoundController.data.playClearSpecialGoal();
			else
				SoundController.data.playClearGoal();
			
			superBallProgress += 0.01f;

		} 
		else 
		{
			SoundController.data.playGoal();
			comboClearGoals = comboGoals_bonusRing = 0;
		}
		
		comboScore += (int)distance;
		plusScoreTxt.text = "+"+comboScore.ToString("F0");
		
		if(special) 
		{
			int heightScore = (int)height;
			comboScore += heightScore;
			plusScoreTxt.text += "\n+"+heightScore.ToString("F0");
			superBallProgress += 0.01f;
		}
		
		if(floored)
		{
			int flooredScore = (int)distance*2;
			plusScoreTxt.text += "+"+flooredScore.ToString("F0");
			superBallProgress += 0.01f;
		}
		
		plusScoreTxt.gameObject.SetActive(true);
		AddScore(comboScore);
		BallCompleted();
	}
	
	void Fail()
	{
		comboGoals = comboClearGoals = comboGoals_bonusRing = comboGoals_bonusAim = comboScore = 0;
		currentBallsCount -= 1;
		
		if(bonusAimActive) 
		{
			bonusAimThrows += 1;
			if(bonusAimThrows == bonusAimThrowsLimit) 
			{
				bonusAimThrows = comboGoals_bonusAim = 0;
				bonusAimActive = false;
			}	
		}
		
		if(bonusRingActive) 
		{
			bonusRingThrows += 1;
			if(bonusRingThrows == bonusRingThrowsLimit) 
			{
				comboGoals_bonusRing = 0;
				StartCoroutine(ResetRing());
			}	
		}
			
		BallCompleted();
	}
	
	void BallCompleted()
	{
		xpLevel = score > 2 * xpScoreStep ? score/xpScoreStep : 1;

		ballsLeft.text = currentBallsCount.ToString();

		//-----------------------
		// Victory or Game Over
		//-----------------------

		if(currentGoalCount >= ballsForVictory)
		{
			GameController.data.Victory();
		}
		else if(currentBallsCount < 1) 
		{
			GameController.data.GameOver();
		}
	}
		
	IEnumerator GrowRing()
	{
		yield return new WaitForSeconds(0.5f);
		bonusRingActive = true;
		ring.SetActive(false);
		ring.transform.localPosition = new Vector3(1.9f,9.51f,0);
		ring.transform.localScale = new Vector3(2,2,2);
		ring.SetActive(true);
		thisAudio.PlayOneShot(SoundController.data.bonusOpen);
	}
	
	IEnumerator ResetRing()
	{
		yield return new WaitForSeconds(0.5f);
		bonusRingActive = false;
		ring.SetActive(false);
		ring.transform.localPosition = new Vector3(1.2f,9.51f,0);
		ring.transform.localScale = new Vector3(1,1,1);
		ring.SetActive(true);
		bonusRingThrows = 0;
	}
	
	public void AddScore(int score) 
	{
		this.score += score;

		if(this.score > PlayerPrefs.GetInt("arcadeBestScore",0))
		{
			PlayerPrefs.SetInt("arcadeBestScore",this.score);
			if(lastRecord > 0 && !hitRecord) 
			{
				HitNewRecord();
			}
		}
	}
	
	public void HitNewRecord()
	{
		SoundController.data.playNewRecord();
		hitRecord = true;
	}
	
	public void ResetData()
	{
		score = 0;
		xpLevel = 1;
		currentBallsCount = startBallsCount;
		ballsLeft.text = currentBallsCount.ToString();
		aimOfGame.text = ballsForVictory.ToString();
		lastRecord = PlayerPrefs.GetInt("arcadeBestScore",0);
	}

}