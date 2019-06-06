using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class communicates with the sensors and determines when there is a notification. Notification information is sent to the corresponding 
 * notifiers which handle how the the notification information is output into the environment.
 */
using System.Timers.Timer;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System;
using VRTK;

 
[System.Serializable]
public class NotificationManager : MonoBehaviour {
	protected Dictionary<NotificationType, List<NotificationObject>> notifierObjMap;
	const int PORT = 5000;
	long timeOutMS;
	bool isAvailable;
	Queue<string> notificationQueue = new Queue<string>();
	Thread thread;
	IPAddress localAddress;
	TcpListener listener;
	TcpClient client;
	Timer notificationTimer;

	public void Awake() {
		localAddress = IPAddress.Parse(GetLocalIPAddress());
		listener = new TcpListener(localAddress, PORT);
        VRTK_ControllerEvents vrtkControllerScript = (VRTK_ControllerEvents) FindObjectOfType(typeof(VRTK_ControllerEvents));  
	}

	public void Start() {
		// User is abailable to interact with notifications
		// by default
		isAvailable = true;

        // Sets time out to 5 seconds by default, can be changed with
        // setTimeOutNotification()
        timeOutMS = 5000;

		// Sets up Timer to fire every 30 seconds to check for notifications
		notificationTimer = new System.Timers.Timer();
		notificationTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        notificationTimer.Interval = 30000;
        notificationTimer.AutoReset = true;
        notificationTimer.Enabled = true;

		notifierObjMap = new Dictionary<NotificationType, List<NotificationObject>> ();
		NotificationObject[] notifierObjects = (NotificationObject[])FindObjectsOfType (typeof(NotificationObject));
		Debug.Log ("Notifier objects found in scene: " + notifierObjects.Length);
		for (int i = 0; i < notifierObjects.Length; i++) {
			NotificationObject n = notifierObjects [i];
			List<NotificationType> types = n.getNotificationTypes ();
			for (int j = 0; j < types.Count; j++) {
				NotificationType currType = types [j];
				if (!notifierObjMap.ContainsKey (currType)) {
					notifierObjMap [currType] = new List<NotificationObject> ();
				}

				notifierObjMap [currType].Add (n);
			}
		}
			
		if (NetworkInterface.GetIsNetworkAvailable ()) {
			thread = new Thread (new ThreadStart (ConnectPhone));
			Debug.Log("Listening for phone...");
			listener.Start();
			thread.Start ();
		}
		//ConnectPhone ();
	}

	public void Update (){
		// Listen for notifications
		// Below code moved to timer event handler

		//string data = null;
		//if (notificationQueue.Count > 0) {
		//	data = notificationQueue.Dequeue ();
		//}
		
		if (Input.GetKeyDown ("space")) {
			if (notifierObjMap.ContainsKey (NotificationType.PHONE_INCOMING_CALL)) {
				List<NotificationObject> notifierList = notifierObjMap [NotificationType.PHONE_INCOMING_CALL];
				for (int i = 0; i < notifierList.Count; i++) {
					if (notifierList [i].CanHandle (NotificationType.PHONE_INCOMING_CALL)) {
						notifierList [i].TriggerNotification (NotificationType.PHONE_INCOMING_CALL);
					}
				}
			}
		} else if (Input.GetKeyDown ("n") || data != null) {
			if (notifierObjMap.ContainsKey (NotificationType.PHONE_TEXT_MESSAGE)) {
				List<NotificationObject> notifierList = notifierObjMap [NotificationType.PHONE_TEXT_MESSAGE];
				for (int i = 0; i < notifierList.Count; i++) {
					if (notifierList [i].CanHandle (NotificationType.PHONE_TEXT_MESSAGE)) {
						notifierList [i].TriggerNotification (NotificationType.PHONE_TEXT_MESSAGE);
					}
				}
			}
		}
	}

    public void ConnectObject(NotificationObject obj)
    {
        List<NotificationType> types = obj.getNotificationTypes();
        for (int i = 0; i < types.Count; i++)
        {
            //TODO: Move this logic to a separate method.
            NotificationType currType = types[i];
            if (!notifierObjMap.ContainsKey(currType))
            {
                notifierObjMap[currType] = new List<NotificationObject>();
            }

            if (!notifierObjMap[currType].Contains(obj)) {
                notifierObjMap[currType].Add(obj);
            }
        }

        NotificationObject[] notifierObjects = (NotificationObject[])FindObjectsOfType(typeof(NotificationObject));
        Debug.Log("Notifier objects found in scene: " + notifierObjects.Length);
    }

	private void ThreadCheck() {
		while (true) {
			ConnectPhone ();
			Thread.Sleep (500);
		}
	}

	public void ConnectPhone() {
		while (true) {
			// Creates accepted client and reads in data from network stream
			client = listener.AcceptTcpClient ();

			// Sets up and starts TCP listener to listen at port 5000
			NetworkStream stream = client.GetStream ();

			// Gets byte data and converts it back to a string
			byte[] streamBuffer = new byte[client.ReceiveBufferSize];
			int bytesRead = stream.Read (streamBuffer, 0, client.ReceiveBufferSize);
			string data = Encoding.ASCII.GetString (streamBuffer, 0, bytesRead);
			notificationQueue.Enqueue (data);
			Debug.Log ("Notification Recieved: " + data);
		}
	}

    // Method to set whether the user is available
    // to interact with notifications
	public void setAvailable(bool value) {
		isAvailable = value;
	}

	private static void OnTimedEvent(object source, ElapsedEventArgs e) {
		Debug.Log("Timer Event Fired");
		string data = null;
		// If notifications are in queue and user is available,
		// pop off a notification from the queue and process it
		if (notificationQueue.Count > 0 && isAvailable) {
			data = notificationQueue.Dequeue ();
			Debug.Log("Notification Dequeued: " + data);
		}
    }

    // Call this method when changing the timer's interval for firing events
    public void setTimerRate(int seconds) {
    	// Timer uses milliseconds, convert seconds specified to milliseconds
    	// then set the timer's interval to that value
    	long ms = seconds * 1000;
        notificationTimer.Interval = ms;
    }

    public void setTimeOutNotification(int seconds) {
        long ms = seconds * 1000;
        timeOutMS = ms;
    }

	public void OnApplicationQuit() {
		// Stops the listener, later on can remove this to make it always active possibly
		if (client != null) {
			client.Close ();
		}

		notificationTimer.Stop();
		if (notificationTimer != null) {
			notificationTimer.Close();
		}
			
		listener.Stop();
	}

	public static string GetLocalIPAddress() {
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip.ToString();
			}
		}
		throw new Exception("Local IP Address Not Found!");
	}
}
