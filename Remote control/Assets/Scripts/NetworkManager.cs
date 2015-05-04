using System;
using System.Collections.Generic;
using System.Globalization;
using Bottle;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NetworkManager : MonoBehaviour {
	// General
#if !UNITY_ANDROID	// Only do this on PC clients
	private SyncStart _syncStart;
#endif
	public float PerformanceDuration = 60;
	private const string TypeGame = "HolmatroRescueExperience";
	private const string GameName = "RoomName";
	enum Role { Remote, Server, Client }
	Role _role;

	// GUI
	private Canvas _canvas;
	public List<Image> ProgressIndicators = new List<Image>();
	private Text _roleText;
	private Text _connectionText;
	private Text _isPlayingText;
	private Image _startButtonImage;
	private Text _startButtonText;

	// Server
	private string _serverIP;
	private int _serverPort;
	List<Client> _clients = new List<Client>();

	// Client
	private bool _clientIsConnectedToServer;
	private bool _isPlaying;
	[Serializable]
	public class Client {
		public int ID;
		public NetworkPlayer NetworkPlayer;
		//public ClientState State;
		public float PlayingSince;
	}

	private NetworkView _networkView;

	public void Awake() {
		// Cache components
		_canvas = FindObjectOfType<Canvas>();
		_networkView = GetComponent<NetworkView>();
#if !UNITY_ANDROID	// Only do this on PC clients
		_syncStart = FindObjectOfType<SyncStart>();
#endif
		_roleText = GameObject.FindGameObjectWithTag("Role text").GetComponent<Text>();
		_connectionText = GameObject.FindGameObjectWithTag("Connection text").GetComponent<Text>();
		_isPlayingText = GameObject.FindGameObjectWithTag("Is playing text").GetComponent<Text>();
		_startButtonImage = GameObject.FindGameObjectWithTag("Start button").GetComponent<Image>();
		_startButtonText = GameObject.FindGameObjectWithTag("Start button text").GetComponent<Text>();

		// Parse settings
		if (Application.platform == RuntimePlatform.Android) {
			_serverIP = "192.168.0.174";
			_serverPort = 25000;
			_role = Role.Remote;
		}
		else {
			_serverIP = ProjectPrefs.GetString("ServerIP");
			_serverPort = ProjectPrefs.GetInt("ServerPort");
			switch (ProjectPrefs.GetString("Role").ToLower(CultureInfo.InvariantCulture)) {
				case "server": _role = Role.Server; break;
				case "remote": _role = Role.Remote; break;
				case "client": _role = Role.Client; break;
				default:
					Debug.LogError("Unknown role specified in Settings.ini: " + ProjectPrefs.GetString("Role"));
					break;
			}
		}

		StartNetwork();
		InitGUI();
	}

	private void InitGUI() {
		_canvas.gameObject.SetActive(_role == Role.Remote);
	}
	private void StartNetwork() {
		switch (_role) {
			case Role.Server:
				StartServer();
				break;
			case Role.Remote:
				InvokeRepeating("JoinServer", 0, 5f);
				break;
			case Role.Client:
				InvokeRepeating("JoinServer", 0, 5f);
				break;
		}
	}

	#region - Server -
	private void StartServer() {
		Network.InitializeServer(4, _serverPort, false);
	}
	private void OnServerInitialized() {
	}
	private void OnPlayerConnected(NetworkPlayer player) {
		Client client = new Client();
		client.NetworkPlayer = player;
		client.PlayingSince = -1;

		_clients.Add(client);

		_startButtonText.text = _clients.Count.ToString();
	}
	private void OnPlayerDisconnected(NetworkPlayer player) {
	}

	#endregion - Server -

	#region - Client -
	private void JoinServer() {
		if (_clientIsConnectedToServer) return;

		Network.Connect(_serverIP, _serverPort);
	}
	private void OnConnectedToServer() {
		_clientIsConnectedToServer = true;
	}
	private void OnDisconnectedFromServer() {
		_clientIsConnectedToServer = false;
	}
	#endregion - Client -

	#region - Commands -
	public void TogglePlayOnAllClients() {
		if (_isPlaying)
			StopAllClients();
		else
			PlayOnAllClients();
	}
	public void PlayOnAllClients() { _networkView.RPC("PlayAllClientsRPC", RPCMode.All); }
	public void StopAllClients() { _networkView.RPC("StopAllClientsRPC", RPCMode.All); }
	#endregion - Commands -
    
	#region - RPC's -
	[RPC] void PlayAllClientsRPC() {
		// On server
		_clients.ForEach(c => c.PlayingSince = Time.time);
		// On client
#if !UNITY_ANDROID	// Only do this on PC clients
		_syncStart.Play();
#endif
		_isPlaying = true;
	}
	[RPC] void StopAllClientsRPC() {
		// On server
		_clients.ForEach(c => c.PlayingSince = -1);
		// On client
#if !UNITY_ANDROID	// Only do this on PC clients
		_syncStart.Stop();
#endif
		_isPlaying = false;
	}
	#endregion - RPC's -

	public void Update() {
		_roleText.text = _role.ToString();
		_connectionText.text = _clientIsConnectedToServer ? "Connected" : "Disconnected";
		_isPlayingText.text = _isPlaying ? "Playing" : "Stopped";

		switch (_role) {
			case Role.Remote:
				foreach (Image progressIndicator in ProgressIndicators)
					progressIndicator.fillAmount = 0;
				for (int n = 0; n < _clients.Count; n++) {
					if (_clients[n].PlayingSince > 0) {
						ProgressIndicators[n].fillAmount = (Time.time - _clients[n].PlayingSince) / PerformanceDuration;
					}
				}
				break;
			case Role.Client:
				break;
			case Role.Server:
				break;
		}
	}
}
