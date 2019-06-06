﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ViveSwipeDetector : MonoBehaviour {
	[SerializeField]
	SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;

    Text text;

	private const int mMessageWidth  = 200;
	private const int mMessageHeight = 64;

	private readonly Vector2 mXAxis = new Vector2(1, 0);
	private readonly Vector2 mYAxis = new Vector2(0, 1);
	private bool trackingSwipe = false;
	private bool checkSwipe = false;

	private readonly string [] mMessage = {
		"",
		"Swipe Left",
		"Swipe Right",
		"Swipe Top",
		"Swipe Bottom"
	};

	private int mMessageIndex = 0;

	// The angle range for detecting swipe
	private const float mAngleRange = 30;

	// To recognize as swipe user should at lease swipe for this many pixels
	private const float mMinSwipeDist = 0.2f;

	// To recognize as a swipe the velocity of the swipe
	// should be at least mMinVelocity
	// Reduce or increase to control the swipe speed
	private const float mMinVelocity  = 4.0f;

	private Vector2 mStartPosition;
	private Vector2 endPosition;

	private float mSwipeStartTime;
	void Awake()
	{

	}
	// Use this for initialization
	void Start () {
		trackedObj = GetComponent<SteamVR_TrackedObject> ();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        text = transform.Find("Canvas/Text").GetComponent<Text>();
        text.text = "Your Boss";
	}

	// Update is called once per frame
	void Update () {
		// Touch down, possible chance for a swipe
		if ((int)trackedObj.index != -1 && device.GetTouchDown (Valve.VR.EVRButtonId.k_EButton_Axis0)) {
			trackingSwipe = true;
			// Record start time and position
			mStartPosition = new Vector2 (device.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis0).x,
				device.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis0).y);
			mSwipeStartTime = Time.time;
		}
		// Touch up , possible chance for a swipe
		else if (device.GetTouchUp (Valve.VR.EVRButtonId.k_EButton_Axis0)) {
			trackingSwipe = false;
			trackingSwipe = true;
			checkSwipe = true;
		}
		else if(trackingSwipe)
		{
			endPosition= new Vector2 (device.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis0).x,
				device.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis0).y);

		}

		if (checkSwipe) {
			checkSwipe = false;
			float deltaTime = Time.time - mSwipeStartTime;
			Vector2 swipeVector = endPosition - mStartPosition;

			float velocity = swipeVector.magnitude/deltaTime;
			Debug.Log(velocity);
			if (velocity > mMinVelocity &&
				swipeVector.magnitude > mMinSwipeDist) {

				swipeVector.Normalize();

				float angleOfSwipe = Vector2.Dot(swipeVector, mXAxis);
				angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;

				// Detect left and right swipe
				if (angleOfSwipe < mAngleRange) {
					OnSwipeRight();
				} else if ((180.0f - angleOfSwipe) < mAngleRange) {
					OnSwipeLeft();
				} else {
					// Detect top and bottom swipe
					angleOfSwipe = Vector2.Dot(swipeVector, mYAxis);
					angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
					if (angleOfSwipe < mAngleRange) {
						OnSwipeTop();
					} else if ((180.0f - angleOfSwipe) < mAngleRange) {
						OnSwipeBottom();
					} else {
						mMessageIndex = 0;
					}
				}
			}
		}

	}

	void OnGUI() {
		// Display the appropriate message
		GUI.Label(new Rect((Screen.width-mMessageWidth)/2,
			(Screen.height-mMessageHeight)/2,
			mMessageWidth, mMessageHeight),
			mMessage[mMessageIndex]);
	}

	private void OnSwipeLeft() {
        text.text = "Call Ignored";
		Debug.Log ("Swipe left");
		mMessageIndex = 1;
	}

	private void OnSwipeRight() {
		Debug.Log ("Swipe right");
        text.text = "In Call";
        mMessageIndex = 2;
	}

	private void OnSwipeTop() {
		Debug.Log ("Swipe Top");
        text.text = "Swipe up";
        mMessageIndex = 3;
	}

	private void OnSwipeBottom() {
		Debug.Log ("Swipe Bottom");
        text.text = "Swipe down";
        mMessageIndex = 4;
	}
}