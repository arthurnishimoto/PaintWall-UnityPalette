using UnityEngine;
using System.Collections;

public class MobileServer : MonoBehaviour {

	public bool startServer;
	public bool serverStarted;

	public int clientsConnected;

	public int serverPort = 25000;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if( startServer && !serverStarted )
		{
			LaunchServer();
		}
		else if( !startServer && serverStarted )
		{
			Network.Disconnect();
			Debug.Log ("Shutting down server");
			serverStarted = false;
		}
	}

	void LaunchServer() {
		//Network.incomingPassword = "HolyMoly";
		bool useNat = !Network.HavePublicAddress();
		Network.InitializeServer(32, serverPort, useNat);
		Debug.Log ("Starting server on port " + serverPort);
		serverStarted = true;
	}

	void OnPlayerConnected(NetworkPlayer player) {
		Debug.Log("Player connected from " + player.ipAddress + ":" + player.port);
		clientsConnected++;
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Player disconnected from " + player.ipAddress + ":" + player.port);
		clientsConnected--;
	}
}
