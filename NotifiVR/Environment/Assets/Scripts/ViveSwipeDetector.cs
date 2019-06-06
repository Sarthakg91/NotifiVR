using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

static class Constants
{
    public const int CALL_COMING = 0;
    public const int CALL_COMING_INTERACT = 1;
    public const int CALL_REJECTED = 4;
    public const int CALL_ACCEPTED = 3;
    public const int CALL_IGNORED = 5;
    public const int CALL_ENDED = 6;


}

public class ViveSwipeDetector : MonoBehaviour
{
    [SerializeField]
    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;
    GameObject rejectIcon;
    GameObject acceptIcon;
    GameObject keepCallIcon;
    GameObject GreenArrows;
    GameObject RedArrows;
    GameObject PhoneCanvas;
    GameObject IgnoreText;
    GameObject IgnoreIcon;
    GameObject BackgroundCircles;
    GameObject Ringer;
    public KeyCode keyPress;
    public AudioClip phoneVoice;
    private bool waitRoutineRunning = false;
    public bool vibration;
    private bool vibrating;

    float spawnTime;
    float interactionTime;
    float firstNoticeTime;
    float interactionDuration;

    Text text;

    private const int mMessageWidth = 200;
    private const int mMessageHeight = 64;
    private int state = -1;//to monitor the state of the system
    private readonly Vector2 mXAxis = new Vector2(1, 0);
    private readonly Vector2 mYAxis = new Vector2(0, 1);
    private bool trackingSwipe = false;
    private bool checkSwipe = false;


    private readonly string[] mMessage = {
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
    private const float mMinVelocity = 4.0f;

    private Vector2 mStartPosition;
    private Vector2 endPosition;

    private float mSwipeStartTime;
    private float timePassed;
    private int hours;
    private int minutes;
    private int seconds;
    private Text timeText;
    private bool countCheck = false;
    float startTime;
    private float timerTarget;
    void Awake()
    {

    }
    // Use this for initialization
    void Start()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        text = transform.Find("PhoneOverlayPrefab/NameText").GetComponent<Text>();
        rejectIcon = transform.Find("PhoneOverlayPrefab/PhoneRedIcon").gameObject;
        acceptIcon = transform.Find("PhoneOverlayPrefab/PhoneGreenIcon").gameObject;
        GreenArrows = transform.Find("PhoneOverlayPrefab/GreenArrows").gameObject;
        RedArrows = transform.Find("PhoneOverlayPrefab/RedArrows").gameObject;
        PhoneCanvas = transform.Find("PhoneOverlayPrefab").gameObject;
        timeText = PhoneCanvas.transform.FindChild("CallTimerText").gameObject.GetComponent<Text>();
        IgnoreIcon = PhoneCanvas.transform.FindChild("IgnoreIcon").gameObject;
        IgnoreText = PhoneCanvas.transform.FindChild("IgnoreText").gameObject;
        keepCallIcon = PhoneCanvas.transform.FindChild("KeepCallicon").gameObject;
        BackgroundCircles = PhoneCanvas.transform.FindChild("BackgroundCircles").gameObject;
        Ringer = PhoneCanvas.transform.FindChild("Ringer").gameObject;
        timerTarget = 0.3f;
        startTime = Time.time;

        if (PhoneCanvas != null)
        {
            PhoneCanvas.SetActive(false);
        }
        vibrating = false;

        spawnTime = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(keyPress))
        {
            //enable notification canvas
            Debug.Log("key is pressed");
            state = Constants.CALL_COMING;
            ActivateNotification();

        }

        if(state==Constants.CALL_COMING_INTERACT)
        {
            if (vibrating == false && vibration)
            {
                timerTarget -= Time.deltaTime;
                if (timerTarget < 0.0f)
                {
                    vibrating = true;
                    RumbleController(0.3f, 2.0f);
                    timerTarget = 0.7f;
                }
            }
        }

        if (state == Constants.CALL_IGNORED && waitRoutineRunning == false)
        {
            PhoneCanvas.SetActive(false);
        }

        else if (state == Constants.CALL_REJECTED && waitRoutineRunning == false)
        {
            PhoneCanvas.SetActive(false);
        }

        else if (state == Constants.CALL_ACCEPTED && waitRoutineRunning == false)
        {

            if (countCheck == false)
            {
                startTime = Time.time;
                countCheck = true;
                Ringer.GetComponent<AudioSource>().clip = phoneVoice;
                Ringer.GetComponent<AudioSource>().Play();
            }
            timePassed = Time.time - startTime;
            hours = (int)timePassed / 3600;
            minutes = ((int)timePassed % 3600) / 60;
            seconds = ((int)timePassed % 3600) % 60;
            string timeString = hours.ToString() + " : " + minutes.ToString() + " : " + seconds.ToString();

            if (timeText == null)
            {
                Debug.Log("Couldn't find text component");
            }
            timeText.text = timeString;

            if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                onTouchPadPress();

            }
        }
        else if (state == Constants.CALL_ENDED)
        {
            //pulsate Time Text
            if (waitRoutineRunning == false)
                PhoneCanvas.SetActive(false);

        }
        // Touch down, possible chance for a swipe

        else if (state == Constants.CALL_COMING_INTERACT)
        {
            if ((int)trackedObj.index != -1 && device.GetTouchDown(Valve.VR.EVRButtonId.k_EButton_Axis0))
            {
                trackingSwipe = true;
                // Record start time and position
                mStartPosition = new Vector2(device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x,
                    device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y);
                mSwipeStartTime = Time.time;
            }
            // Touch up , possible chance for a swipe
            else if (device.GetTouchUp(Valve.VR.EVRButtonId.k_EButton_Axis0))
            {
                trackingSwipe = false;
                trackingSwipe = true;
                checkSwipe = true;
            }
            else if (trackingSwipe)
            {
                endPosition = new Vector2(device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x,
                    device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y);

            }

            if (checkSwipe)
            {
                checkSwipe = false;
                float deltaTime = Time.time - mSwipeStartTime;
                Vector2 swipeVector = endPosition - mStartPosition;

                float velocity = swipeVector.magnitude / deltaTime;
                Debug.Log(velocity);
                if (velocity > mMinVelocity &&
                    swipeVector.magnitude > mMinSwipeDist)
                {

                    swipeVector.Normalize();

                    float angleOfSwipe = Vector2.Dot(swipeVector, mXAxis);
                    angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;

                    // Detect left and right swipe
                    if (angleOfSwipe < mAngleRange)
                    {
                        OnSwipeRight();
                    }
                    else if ((180.0f - angleOfSwipe) < mAngleRange)
                    {
                        OnSwipeLeft();
                    }
                    else
                    {
                        // Detect top and bottom swipe
                        angleOfSwipe = Vector2.Dot(swipeVector, mYAxis);
                        angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
                        if (angleOfSwipe < mAngleRange)
                        {
                            OnSwipeTop();
                        }
                        else if ((180.0f - angleOfSwipe) < mAngleRange)
                        {
                            OnSwipeBottom();
                        }
                        else
                        {
                            mMessageIndex = 0;
                        }
                    }
                }
            }
        }

    }

    void RumbleController(float duration, float strength)
    {
        StartCoroutine(RumbleControllerRoutine(duration, strength));
    }

    System.Collections.IEnumerator RumbleControllerRoutine(float duration, float strength)
    {
        strength = Mathf.Clamp01(strength);
        float startTime = Time.realtimeSinceStartup;
            //rumble
            while (Time.realtimeSinceStartup - startTime <= duration)
            {
                int valveStrength = Mathf.RoundToInt(Mathf.Lerp(0, 3999, strength));

                SteamVR_Controller.Input((int)trackedObj.index).TriggerHapticPulse((ushort)valveStrength);

                yield return null;
            }
        vibrating = false;

    }

    private void TriggerHapticPulse(int v)
    {
        throw new NotImplementedException();
    }

    void ActivateNotification()
    {
        PhoneCanvas.SetActive(true);
        rejectIcon.SetActive(true);
        acceptIcon.SetActive(true);
        keepCallIcon.SetActive(false);
        GreenArrows.SetActive(true);
        RedArrows.SetActive(true);
        IgnoreText.SetActive(true);
        IgnoreIcon.SetActive(true);
        BackgroundCircles.SetActive(true);
        BackgroundCircles.GetComponent<Animator>().SetBool("pulsing", true);
        text.text = "Henry Calling";
        timeText.text = "0:0:0";
        timeText.gameObject.SetActive(false);
        Ringer.GetComponent<AudioSource>().Play();
        state = Constants.CALL_COMING_INTERACT;

    }

    IEnumerator UpdateTimeText(float duration)
    {
        waitRoutineRunning = true;
        float startTime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - startTime <= duration)
        {
            Debug.Log("inside enumerator");
            yield return null;
        }

        waitRoutineRunning = false;
    }

    IEnumerator WaitToDisableEnableRoutine(float duration)
    {
        waitRoutineRunning = true;
        float startTime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - startTime <= duration)
        {
            Debug.Log("inside enumerator");

            yield return null;
        }
        waitRoutineRunning = false;
    }

    void OnGUI()
    {
        // Display the appropriate message
        GUI.Label(new Rect((Screen.width - mMessageWidth) / 2,
            (Screen.height - mMessageHeight) / 2,
            mMessageWidth, mMessageHeight),
            mMessage[mMessageIndex]);
    }
    private void disableIcons()
    {
        if (rejectIcon != null)
            rejectIcon.SetActive(false);
        if (acceptIcon != null)
            acceptIcon.SetActive(false);
    }

    private void disableArrows()
    {
        if (RedArrows != null)
            RedArrows.SetActive(false);
        if (GreenArrows != null)
            GreenArrows.SetActive(false);
    }

    private void OnSwipeLeft()
    {
        text.text = "Call Rejected";
        Debug.Log("Swipe left");
        mMessageIndex = 1;
        disableIcons();
        disableArrows();
        StartCoroutine(WaitToDisableEnableRoutine(2));
        state = Constants.CALL_REJECTED;
        BackgroundCircles.SetActive(false);
        Ringer.GetComponent<AudioSource>().Pause();
        vibration = false;

    }

    private void OnSwipeRight()
    {
        Debug.Log("Swipe right");
        text.text = "Henry In Call";
        mMessageIndex = 2;
        disableIcons();
        disableArrows();
        state = Constants.CALL_ACCEPTED;
        timeText.gameObject.SetActive(true);
        keepCallIcon.SetActive(true);
        IgnoreIcon.SetActive(false);
        IgnoreText.SetActive(false);
        BackgroundCircles.GetComponent<Animator>().SetBool("pulsing", false);
        Ringer.GetComponent<AudioSource>().Pause();
        StartCoroutine(WaitToDisableEnableRoutine(1));
        vibration = false;


    }

    private void OnSwipeTop()
    {

        Debug.Log("Swipe Top");
        text.text = "Call Ignored";
        mMessageIndex = 3;
        disableIcons();
        disableArrows();
        StartCoroutine(WaitToDisableEnableRoutine(2));
        state = Constants.CALL_IGNORED;
        IgnoreIcon.SetActive(false);
        IgnoreText.SetActive(false);
        Ringer.GetComponent<AudioSource>().Pause();
        vibration = false;

    }

    private void onTouchPadPress()
    {
        Debug.Log("Call ended");
        text.text = "Call Ended";
        mMessageIndex = 5;
        StartCoroutine(WaitToDisableEnableRoutine(2));
        state = Constants.CALL_ENDED;
        Ringer.GetComponent<AudioSource>().Pause();

    }

    private void OnSwipeBottom()
    {
        Debug.Log("Swipe Bottom");
        text.text = "Swipe down";
        mMessageIndex = 4;
        disableIcons();
        disableArrows();
    }
}