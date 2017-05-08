using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

	public int lifeCount = 3;
	public int scoreCount = 0;
	public float timeDuration = 0f;
	public Text infoText;
	public Text scoreText;
	public Text lifeText;

	public Text lb_ranking;
	public Text lb_duration;
	public Text lb_score;
	public Text lb_name;

	void Start()
	{
		infoText.text = "";
	}

	public void AddScore(int score)
	{
		scoreCount += score;
		scoreText.text = "Score: " + scoreCount;
	}

	public void LostLife()
	{
		lifeCount--;
		lifeText.text = "*" + lifeCount;
	}

	public void ResetLife()
	{
		lifeCount = 3;
		lifeText.text = "*" + lifeCount;
	}

	public void WriteInfo(string newInfo)
	{
		infoText.text = newInfo;
	}

	public void UpdateLeaderBoard( Dictionary<string, object> resultData )
	{

		if (resultData==null)
			return;

		Debug.Log ("time to update leaderboard!");

		lb_ranking.text = "";
		lb_duration.text = "";
		lb_score.text = "";
		lb_name.text = "";

		int lb_index = 0;

		// v.1
		/*
		foreach( KeyValuePair<string, object> entry in resultData )
		{
			lb_index++;

			lb_ranking.text += ("#" + lb_index + "-\n");
			//lb_name.text += (entry.Key + "\n");
			lb_duration.text += (entry.Key + "\n");

			//Debug.Log("key: " + entry.Key + ", Value: " + entry.Value);
			//Debug.Log("Player: " + entry.Key);

			//Debug.Log(entry.Value);

			Dictionary<string, object> n_data = entry.Value as Dictionary<string, object>;
		
			//lb_duration.text += (n_data["time"] + "\n");
			lb_name.text += (n_data["username"] + "\n");
			lb_score.text += (n_data["score"] + "\n");
		}
		*/
		
		// v.2
		Dictionary<string, int> mySortData = new Dictionary<string, int>();

		foreach( KeyValuePair<string, object> entry in resultData )
		{
			//v.1
			/*
			lb_index++;

			lb_ranking.text += ("#" + lb_index + "-\n");

			lb_name.text += (entry.Key + "\n");
			//lb_duration.text += (entry.Key + "\n");

			//Debug.Log("key: " + entry.Key + ", Value: " + entry.Value);
			//Debug.Log("Player: " + entry.Key);

			//Debug.Log(entry.Value);

			Dictionary<string, object> n_data = entry.Value as Dictionary<string, object>;

			lb_duration.text += (n_data["time"] + "\n");
			//lb_name.text += (n_data["username"] + "\n");
			lb_score.text += (n_data["score"] + "\n");
			*/

			//v.2
			// create new Dictionary with just name and time
			Dictionary<string, object> n_data = entry.Value as Dictionary<string, object>;

			//int _tmpTime = int.Parse(n_data ["time"]as string);
			string _tmp_ = n_data ["time"]+"";
			int _tmpInt_ = int.Parse (_tmp_);
			//Debug.Log(_tmpInt_);
			mySortData.Add(entry.Key, _tmpInt_);
		}

		foreach(var item in mySortData.OrderByDescending(key => key.Value))
		{
			lb_index++;

			lb_ranking.text += ("#" + lb_index + "-\n");

			lb_name.text += (item.Key + "\n");

			Dictionary<string, object> n_data = resultData[item.Key] as Dictionary<string, object>;

			lb_duration.text += (n_data["time"] + "\n");
			//lb_name.text += (n_data["username"] + "\n");
			lb_score.text += (n_data["score"] + "\n");
		}

	}
}
