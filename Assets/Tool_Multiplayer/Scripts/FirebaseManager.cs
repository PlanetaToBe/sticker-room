using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class FirebaseManager : MonoBehaviour {

	public string databaseURL;

	public class User
	{
		public string username;
		public int score;
		public int time;

		public User()
		{
			this.username = "Anonymouse";
			this.score = 0;
			this.time = 0;
		}

		public User(string username, int score, int time)
		{
			this.username = username;
			this.score = score;
			this.time = time;
		}
	}
	//public ScoreManager scoreManager;
	[System.Serializable]
	public class Thing
	{
		public string thingName;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;

		public Thing()
		{
			this.thingName = "Anonymouse";
			this.position = Vector3.zero;
			this.rotation = Quaternion.identity;
			this.scale = Vector3.one;
		}

		public Thing(string name, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			this.thingName = name;
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
		}
	}

	private DatabaseReference mDatabaseRef;

	void Start ()
	{
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl (databaseURL);

		// root reference location of the database
		mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

		SaveNewUser ("laura", 12, 35);
		SaveNewUser ("dan", 52, 135);
		SaveNewUser ("test", 12, 5);
		SaveNewUser ("zoo", 12, 500);

		GetUserData();
	}

	public void SaveNewUser(string name, int score, int time)
	{
		User user = new User (name, score, time);
		string json = JsonUtility.ToJson (user);

		//mDatabaseRef.Child ("users").Child(""+time).SetRawJsonValueAsync (json);
		mDatabaseRef.Child ("users").Child(name).SetRawJsonValueAsync (json);
	}

	public void GetUserData()
	{
		Dictionary<string,object> dataToGUI = null;

		//v.1
		/*
		FirebaseDatabase
			.DefaultInstance
			.GetReference ("users").OrderByChild("time").LimitToFirst(15)
			.GetValueAsync ().ContinueWith (task => {
				if(task.IsFaulted)
				{
					Debug.Log("Error occures when getting data");
				}
				else if(task.IsCompleted)
				{
					DataSnapshot snapshot = task.Result;
					//Debug.Log(snapshot.Value); // => Dictionary<string,object>
					dataToGUI = snapshot.Value as Dictionary<string,object>;


					foreach( KeyValuePair<string, object> entry in dataToGUI )
					{
						//Debug.Log("key: " + entry.Key + ", Value: " + entry.Value);

						Debug.Log("Time: " + entry.Key);

						string name = entry.Key;
						Dictionary<string, object> n_data = entry.Value as Dictionary<string, object>;

						foreach( KeyValuePair<string, object> n_entry in n_data )
						{
							Debug.Log("___" + n_entry.Key + ": " + n_entry.Value);
						}
					}

					scoreManager.UpdateLeaderBoard(dataToGUI);
				}
			});
		*/

		mDatabaseRef.Child ("users").OrderByChild("time")
			.GetValueAsync ().ContinueWith (task => {
				if(task.IsFaulted)
				{
					Debug.Log("Error occures when getting data");
				}
				else if(task.IsCompleted)
				{
					DataSnapshot snapshot = task.Result;
					//Debug.Log(snapshot.Value); // => Dictionary<string,object>
					dataToGUI = snapshot.Value as Dictionary<string,object>;

					/*
					foreach( KeyValuePair<string, object> entry in dataToGUI )
					{
						//Debug.Log("key: " + entry.Key + ", Value: " + entry.Value);

						Debug.Log("Time: " + entry.Key);

						string name = entry.Key;
						Dictionary<string, object> n_data = entry.Value as Dictionary<string, object>;

						foreach( KeyValuePair<string, object> n_entry in n_data )
						{
							Debug.Log("___" + n_entry.Key + ": " + n_entry.Value);
						}
					}*/

					//scoreManager.UpdateLeaderBoard(dataToGUI);
				}
			});
	}
}
