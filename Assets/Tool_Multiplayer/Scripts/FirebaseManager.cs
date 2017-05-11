using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class FirebaseManager : MonoBehaviour {

	public string databaseURL;
	public GameObject testObject;

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

	[System.Serializable]
	public class ResultContainer
	{
		public List <Thing> items;
	}

	private DatabaseReference mDatabaseRef;
	private DatabaseReference mDatabaseThingRef;

	void Start ()
	{
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl (databaseURL);

		// root reference location of the database
		mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
		mDatabaseThingRef = FirebaseDatabase.DefaultInstance.GetReference("things");

		//SaveNewUser ("test", 12, 5);
		//GetUserData();

		SaveNewThing (testObject);
		GetThingData();
	}

	public void SaveNewThing(GameObject _thing)
	{
		Thing thing = new Thing (_thing.name, _thing.transform.position, _thing.transform.rotation, _thing.transform.localScale);
		string json = JsonUtility.ToJson (thing);

		//mDatabaseRef.Child ("things").Child(_thing.name).SetRawJsonValueAsync (json);
		mDatabaseRef.Child ("things").Child(_thing.name).SetRawJsonValueAsync (json);
	}

	public void GetThingData()
	{
		Dictionary<string,object> dataToGUI = null;

		mDatabaseRef.Child ("things")
			.GetValueAsync ().ContinueWith (task => {
				if(task.IsFaulted)
				{
					Debug.Log("Error occures when getting data");
				}
				else if(task.IsCompleted)
				{
					DataSnapshot snapshot = task.Result;
					dataToGUI = snapshot.Value as Dictionary<string,object>;

					string dataInJson = snapshot.GetRawJsonValue();
					Debug.Log(dataInJson);

					// incert "Items": [ into 2nd location, and ] into last to 2nd location
					/*
					string newData = dataInJson.Insert(1, "\"Items\":[");
					int incert_pos = newData.Length-1;
					string newData2 = newData.Insert(incert_pos, "]");
					Debug.Log(newData2);

					Thing[] thingss = JsonHelper.FromJsonArray<Thing>(newData2);
					foreach( Thing t in thingss )
					{
						Debug.Log(t);
					}
					*/

					foreach( KeyValuePair<string, object> entry in dataToGUI )
					{
						//Debug.Log("key: " + entry.Key + ", Value: " + entry.Value);
						Debug.Log(entry.Value.ToString());
						//Debug.Log( JsonHelper.FromJson<Thing>(entry.Value.ToString()) );


//						string name = entry.Key;
//						Dictionary<string, object> n_data = entry.Value as Dictionary<string, object>;
//
//						foreach( KeyValuePair<string, object> n_entry in n_data )
//						{
//							Debug.Log("___" + n_entry.Key + ": " + n_entry.Value);
//						}
					}
				}
			});
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

	// source: https://forum.unity3d.com/threads/how-to-load-an-array-with-jsonutility.375735/
	// http://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity/36244111#36244111
	public static class JsonHelper
	{
		public static T FromJson<T>(string json)
		{
			return JsonUtility.FromJson<T>(json);
		}

		public static T[] FromJsonArray<T>(string json)
		{
			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
			return wrapper.Items;
		}

		public static string ToJsonArray<T>(T[] array)
		{
			Wrapper<T> wrapper = new Wrapper<T>();
			wrapper.Items = array;
			return JsonUtility.ToJson(wrapper);
		}

		public static string ToJsonArray<T>(T[] array, bool prettyPrint)
		{
			Wrapper<T> wrapper = new Wrapper<T>();
			wrapper.Items = array;
			return JsonUtility.ToJson(wrapper, prettyPrint);
		}

		[System.Serializable]
		private class Wrapper<T>
		{
			public T[] Items;
		}
	}
}
