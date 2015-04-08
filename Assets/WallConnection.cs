#define UNITY_ANDROID_PRO
using UnityEngine;
using System.Collections;

#if UNITY_ANDROID_PRO
using System.Net;
using System.Net.Sockets;
#endif

public class WallConnection : MonoBehaviour {

	public string wallIPAddresss = "131.193.78.206";
	public int wallPort = 13337;

	public bool connectToWall = false;
	public bool connected = false;
	public bool connecting = false;
	// TCP Connection
#if UNITY_ANDROID_PRO
	TcpClient client;
	NetworkStream streamToServer;
#endif
	public Color lastColor;
	public int currentTool = 1;
	public GameObject[] tools;

	// Use this for initialization
	void Start () {

	}

	public void Connect(string serverIP, int msgPort)
	{
#if UNITY_ANDROID_PRO
		try
		{
			// Create a TcpClient.
			Debug.Log("WallConnection: Connecting to to " + serverIP);
			client = new TcpClient(serverIP, msgPort);
			streamToServer = client.GetStream();

			// Initialization Message
			//string message = "999 999 999 999 999\n";
			//Debug.Log("Sending to wall '" + message+"'");
			//byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
			//streamToServer.Write(data, 0, data.Length);

			//Console.WriteLine("Handshake Sent: {0}", message);
			Debug.Log("WallConnection: Connected to " + serverIP);

			connected = true;
		}
		catch (SocketException e)
		{
			Debug.LogError("SocketException: " + e);
			connected = false;
			connectToWall = false;
		}
#else
		if( !connecting )
		{
			Debug.Log ("Connecting to mobile server: " + serverIP + ":" + (msgPort));
			Network.Connect(serverIP, msgPort);
		}
		connecting = true;
#endif
	}// Connect

	void SendMessageToWall(Color color, int tool)
	{
		string message =  (100+tool) + " " +(int)(100+255*color.r) + " " + (int)(100+255*color.g) + " " + (int)(100+255*color.b) + " " + (int)(100+255*color.a)+"\n";

		if( !connected )
		{
			Debug.Log("Not connected to wall: '" + message+"'");
			return;
		}

		Debug.Log("Sending to wall '" + message+"'");
		byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
		{
#if UNITY_ANDROID_PRO
		streamToServer.Write(data, 0, data.Length);
#else
			networkView.RPC("SendCommand", RPCMode.Server, message);
#endif
		}
	}

	void OnConnectedToServer() {
		Debug.Log("Connected to server");
		connected = true;
	}

	void OnFailedToConnect(NetworkConnectionError error) {
		Debug.Log("Could not connect to server: " + error);
		connectToWall = false;
	}

	[RPC]
	void SendCommand(string cmd)
	{
		Debug.Log("Forwarding to wall '" + cmd+"'");
		byte[] data = System.Text.Encoding.ASCII.GetBytes(cmd);
#if UNITY_ANDROID_PRO
		streamToServer.Write(data, 0, data.Length);
#endif
	}
	// Update is called once per frame
	void Update () {

		if ( (Input.GetKey(KeyCode.LeftAlt)||Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.F11) // Alt-F11
		    || Input.GetKeyDown(KeyCode.Menu) // Android menu button
		    || Input.GetKeyDown(KeyCode.Escape) // Android back button
		    )
			showGUI = !showGUI;

		if( connectToWall && !connected )
			Connect(wallIPAddresss, wallPort);
		else if( !connectToWall && connected )
		{
			Dispose ();
		}
		if( connecting && !connectToWall )
			connecting = false;

		if (Input.GetMouseButtonDown (0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				GameObject hitObj = hit.collider.gameObject;
				if( hitObj.name == "tool" )
				{
					if( currentTool == 1 )
					{
						tools[0].SetActive(false);
						tools[1].SetActive(true);
						currentTool = 2;
					}
					else if( currentTool == 2 )
					{
						tools[0].SetActive(true);
						tools[1].SetActive(false);
						currentTool = 1;
					}
					SendMessageToWall (lastColor, currentTool);
				}
			}
		}
	}

	void UpdateColor(Color newColor)
	{
		lastColor = newColor;
		SendMessageToWall (newColor, currentTool);
	}

	public void Dispose() 
	{
		if( connected )
		{
#if UNITY_ANDROID_PRO
			// Close TCP connection.
			streamToServer.Close();
			client.Close();
#endif		
			connected = false;
			Debug.Log("WallConnection: Closing connections.");
		}

	}

	void OnGUI() {
		if( showGUI )
		{
			mainWindow = GUI.Window(0, mainWindow, OnMainWindow, "Paint Settings");
		}
	}

	public bool showGUI = false;
	Rect mainWindow = new Rect(20, 20, 512, 300);
	float rowHeight = 25;
	Vector2 GUIOffset = new Vector2(0, 25);

	GUIStyle connectStatus = new GUIStyle();

	void OnMainWindow(int windowID) {

		GUI.DragWindow (new Rect (0, 0, 10000, 20));

		connectToWall = GUI.Toggle (new Rect (GUIOffset.x + 20, GUIOffset.y + rowHeight * 0, 250, 40), connectToWall, "Connect to Server:");
		string statusText = "NOT CONNECTED";
		connectStatus.normal.textColor = Color.white;
		if( connected )
		{
			statusText = "CONNECTED";
			connectStatus.normal.textColor = Color.green;
		}
		else if( connecting )
		{
			statusText = "CONNECTING";
		}
		GUI.Label(new Rect(GUIOffset.x + 150, GUIOffset.y + rowHeight * 0 + 3, 250, 200), statusText, connectStatus);

		GUI.Label(new Rect(GUIOffset.x + 25, GUIOffset.y + rowHeight * 2, 120, 40), "Wall Server IP:");
		wallIPAddresss = GUI.TextField(new Rect(GUIOffset.x + 150, GUIOffset.y + rowHeight * 2, 200, 40), wallIPAddresss, 25);
		
		GUI.Label(new Rect(GUIOffset.x + 25, GUIOffset.y + rowHeight * 4, 120, 40), "Wall Message Port:");
		wallPort = int.Parse(GUI.TextField(new Rect(GUIOffset.x + 150, GUIOffset.y + rowHeight * 4, 200, 40), wallPort.ToString(), 25));

#if UNITY_ANDROID_PRO
		MobileServer mobileServer = GetComponent<MobileServer> ();
		mobileServer.startServer = GUI.Toggle (new Rect (GUIOffset.x + 20, GUIOffset.y + rowHeight * 6, 250, 40), mobileServer.startServer, "Mobile Server: ");
		string mobileServerText = "NOT RUNNING";
		connectStatus.normal.textColor = Color.white;
		if( mobileServer.serverStarted )
		{
			mobileServerText = "RUNNING";
			connectStatus.normal.textColor = Color.green;
		}
		
		GUI.Label(new Rect(GUIOffset.x + 150, GUIOffset.y + rowHeight * 6 + 3, 250, 200), mobileServerText, connectStatus);

		GUI.Label(new Rect(GUIOffset.x + 25, GUIOffset.y + rowHeight * 8, 120, 20), "Mobile Server Port:");
		mobileServer.serverPort = int.Parse(GUI.TextField(new Rect(GUIOffset.x + 150, GUIOffset.y + rowHeight * 8, 200, 40), mobileServer.serverPort.ToString(), 25));
#endif
	}
}
