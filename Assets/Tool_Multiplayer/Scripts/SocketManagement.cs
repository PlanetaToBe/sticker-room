using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;
using BestHTTP;
using BestHTTP.SocketIO;
using UnityEngine.UI;
using UnityEngine.VR;

public class SocketManagement : MonoBehaviour {

	[Header("Socket Settings")]
	public string networkAddress;
	public string portNumber;
	[HideInInspector]
	public string myName = string.Empty;
	public bool isConnected = false;
	[HideInInspector]
	public int whoIamInLife;
	[HideInInspector]
	public bool isViveVR;
	public SocketManager Manager;

	[Header("UI Settings")]
	public GameObject menuCanvas;
	public GameObject worldCanvas;
	public Button sendButton;
	public InputField msgInput;

	[Header("Object Assignment")]
	public GameObject simplePlayerPrefab;
	public GameObject nameTagPrefab;
	public GameObject physicPlayerPrefab;
	public GameObject floor;
	public bool freeFromFloor = true;

	// ---------- FIREBASE -----------
	public FirebaseManager firebaseManager;

	// ---------- ACTION -----------
	public event Action<int, string> OnSticker;

	private enum ChatStates
	{
		Login,
		Chat
	}
	private ChatStates State;
	private string message = string.Empty;
	private string chatLog = string.Empty;
	private GameObject mainCamera;

	private Dictionary<int, GameObject> playerDict = new Dictionary<int, GameObject>();
	private bool updatePreviousPlayer = false;

	void Awake() {
		if (UnityEngine.XR.XRSettings.enabled)
		{
			isViveVR = true;
		}
		else
		{
			isViveVR = false;
		}
	}

	void Start () {
		if (Camera.main.gameObject != null)
		{
			mainCamera = Camera.main.gameObject;
		}
		else
		{
			mainCamera = null;
		}

		State = ChatStates.Login;

		// Socket
		SocketOptions options = new SocketOptions ();
		options.AutoConnect = false;
		Manager = new SocketManager (new Uri("http://"+networkAddress+":"+portNumber+"/socket.io/"), options);

		// Setup Events
		Manager.Socket.On ("login", OnLogin);
		Manager.Socket.On ("new message", OnNewMessage);
		Manager.Socket.On ("player joined", OnPlayerJoined);
		Manager.Socket.On ("player left", OnPlayerLeft);
		Manager.Socket.On ("update position", OnUpdatePosition);
		Manager.Socket.On ("update history", OnUpdateHistory);
		//
		Manager.Socket.On ("send sticker", OnSendSticker);

		// On Error
		Manager.Socket.On (SocketIOEventTypes.Error, (socket, packet, args) 
			=> Debug.Log(string.Format("Error: {0}", args[0].ToString()))
		);
			
		Manager.Open ();
	}

	void OnDestroy()
	{
		Manager.Close ();
	}

	public void PushSendButton()
	{
		switch (State)
		{
		case ChatStates.Login:
				SetUserName ();
				menuCanvas.SetActive (false);
				break;

			case ChatStates.Chat:
				SendNewMessage ();
				break;
		}
	}

	void SetUserName()
	{
		string u_n = msgInput.textComponent.text;
		if (string.IsNullOrEmpty (u_n))
			u_n = "Anonymous";
		myName = u_n;
		State = ChatStates.Chat;

		Manager.Socket.Emit ("new player", u_n);
	}

	void SendNewMessage()
	{
		string msg = msgInput.textComponent.text;
		if (string.IsNullOrEmpty (msg))
			return;

		Manager.Socket.Emit ("new message", msg);
	}

	#region SocketIO Evenets
	void OnLogin(Socket socket, Packet packet, params object[] args) //Socket socket, Packet packet, params object[] args
	{
		isConnected = true;

		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		whoIamInLife = GetInt (data["index"]);
		int n_p = GetInt (data["numPlayers"]);
		Debug.Log("Connected to Socket Server! I'm #" + whoIamInLife + " in total " + n_p + " players");

		// disable main camera, if any
		if (mainCamera) {
			Destroy(mainCamera.GetComponent<GvrAudioListener>());
			mainCamera.SetActive (false);
		}

		// create player SELF
		GameObject selfPlayer = CreatePlayer( true, null, null, null, whoIamInLife, myName );

		// send new player info back to server
		Dictionary<string, object> msg = new Dictionary<string, object>();
		msg.Add ("index", whoIamInLife);
		msg.Add ("startX", selfPlayer.transform.position.x);
		msg.Add ("startY", selfPlayer.transform.position.y);
		msg.Add ("startZ", selfPlayer.transform.position.z);
		//msg.Add ("color", selfPlayerMgmt.color);
		msg.Add ("username", myName);

		Manager.Socket.Emit("create new player", msg);
	}

	void OnNewMessage(Socket socket, Packet packet, params object[] args) //Socket socket, Packet packet, params object[] args
	{
		if (!isConnected)
			return;
		
		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		var username = data ["username"] as string;
		var msg = data["message"] as string;
		Debug.Log (username + " sends msg: " + msg);
	}

	void OnPlayerJoined(Socket socket, Packet packet, params object[] args) //Socket socket, Packet packet, params object[] args
	{
		if (!isConnected)
			return;
		
		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		var p_index = GetInt(data["index"]);
		var username = data ["username"] as string;
		int numP = GetInt(data["numPlayers"]);
		Dictionary<string, object> transform = data["transform"] as Dictionary<string, object>;

		CreatePlayer ( false, transform["startX"], transform["startY"], transform["startZ"], p_index, username );
		Debug.Log("New player: " + username + " joined! Now total player num: " + numP);
	}
		
	void OnPlayerLeft(Socket socket, Packet packet, params object[] args) //Socket socket, Packet packet, params object[] args
	{
		if (!isConnected)
			return;
		
		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		var username = data ["username"] as string;
		int numP = GetInt(data["numPlayers"]);
		int leftIndex = GetInt(data["index"]);

		if( playerDict.ContainsKey(leftIndex) )
		{
			// destroy nameTag first
			PlayerManagement p_mgmt = playerDict[leftIndex].GetComponent<PlayerManagement>();
			Destroy (p_mgmt.nameTag);
			Destroy (playerDict[leftIndex]);
			playerDict.Remove (leftIndex);
		}
		Debug.Log("Player: " + username + " left! Now total player num: " + numP);
	}

	void OnUpdatePosition(Socket socket, Packet packet, params object[] args) //Socket socket, Packet packet, params object[] args
	{
		if (!isConnected)
			return;
		
		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		//string username = data ["username"] as string;
		string transType = data ["type"] as string;
		int index = GetInt(data ["index"]);

		// update player
		if( playerDict.ContainsKey(index) )
		{
			playerDict [index].GetComponent<PlayerManagement> ().UpdateTrans (
				transType, GetFloat(data["posX"]), GetFloat(data["posY"]), GetFloat(data["posZ"]),
				GetFloat(data["quaX"]), GetFloat(data["quaY"]), GetFloat(data["quaZ"]), GetFloat(data["quaW"])
			);
		}
		/*
		Debug.Log("Player: " + username + " position x: " + data ["posX"]
			+ ", y: " + data ["posY"]
			+ ", z: " + data ["posZ"]);
		*/
	}

	void OnUpdateHistory(Socket socket, Packet packet, params object[] args) //Socket socket, Packet packet, params object[] args
	{
		if (!isConnected)
			return;
		
		if (!updatePreviousPlayer) {
			Dictionary<string, object> data = args [0] as Dictionary<string, object>;
			List<object> allplayers = data ["allPlayers"] as List<object>;

			for (int i=0; i<allplayers.Count; i++)
			{
				Dictionary<string, object> a_p = allplayers[i] as Dictionary<string, object>;
				int p_index = GetInt (a_p ["index"]);
				string p_name = a_p ["username"] as string;

				CreatePlayer (false, a_p ["startX"], a_p ["startY"], a_p ["startZ"], p_index, p_name);
				Debug.Log("Add history player: " + p_name);
			}
			updatePreviousPlayer = true;
		}
	}

	void OnSendSticker(Socket socket, Packet packet, params object[] args)
	{
		Dictionary<string, object> data = args [0] as Dictionary<string, object>;
		string fromName = data ["username"] as string;
		int index = GetInt(data ["index"]);

		if (OnSticker != null)
			OnSticker (index, fromName);
	}
	#endregion

	GameObject CreatePlayer(bool isLocal, object _posX, object _posY, object _posZ, int _index, string _name)
	{
		GameObject player;
		if (isLocal && floor!=null)
		{
			player = Instantiate (physicPlayerPrefab);
		}
		else
		{
			player = Instantiate (simplePlayerPrefab);	
		}

		PlayerManagement playerMgmt = player.GetComponent<PlayerManagement> ();
		playerMgmt.nameTag = Instantiate(nameTagPrefab);
		playerMgmt.nameTag.transform.SetParent (worldCanvas.transform);

		if (!isLocal)
		{
			Vector3 playerStartPos = new Vector3(
				GetFloat(_posX), GetFloat(_posY), GetFloat(_posZ)
			);
			player.transform.position = playerStartPos;
			player.name = "Player #"+ _index + " " + _name;
			playerMgmt.InitPlayer( _index, _name);
			playerMgmt.nameTag.transform.position = playerStartPos;
		} 
		else
		{
			if (!freeFromFloor)
			{
				player.transform.parent = floor.transform;
			}

			player.name = "ME #"+ _index + " " + _name;
			playerMgmt.socketManagement = this;
			playerMgmt.InitPlayer( _index, _name);
			playerMgmt.OnStartLocalPlayer ();
		}
		playerDict.Add (playerMgmt.whoIam, player);

		return player;
	}

	int GetInt(object num)
	{
		int numm = Convert.ToInt32 (num);
		return numm;
	}

	float GetFloat(object num)
	{
		float numm = Convert.ToSingle (num);
		return numm;
	}
}
