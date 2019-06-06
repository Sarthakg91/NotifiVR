using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
static class PhoneConstants
{
    public const int CALL_COMING = 0;
    public const int CALL_COMING_INTERACT = 1;
    public const int CALL_REJECTED = 4;
    public const int CALL_ACCEPTED = 3;
    public const int CALL_IGNORED = 5;
    public const int CALL_ENDED = 6;
    public const int IDLE = 7;


}
public class phoneOverlayInteraction : MonoBehaviour {

    [SerializeField]
    public SteamVR_TrackedObject trackedObj;
    public NotificationController notif;
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
    GameObject personImage;
    public KeyCode keyPress;
    public AudioClip phoneVoice;
    public AudioClip ringTone;
    private bool waitRoutineRunning = false;
    bool vibration;
    private bool vibrating;
    bool reactionTimeNoted;
    bool noticed;

    float spawnTime;
    float interactionTime;
    float firstNoticeTime;
    float interactionDuration;
    bool notificationOn = false;

    Text text;

    private const int mMessageWidth = 200;
    private const int mMessageHeight = 64;
    private int state = -1;//to monitor the state of the system
    private readonly Vector2 mXAxis = new Vector2(1, 0);
    private readonly Vector2 mYAxis = new Vector2(0, 1);
    private bool trackingSwipe = false;
    private bool checkSwipe = false;

    //bool hapticEnabled = false;

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
    bool interactionTimeNoted = false;
    bool interactionDurationNoted = false;
    // Use this for initialization
    void Start()
    {
        device = SteamVR_Controller.Input((int)trackedObj.index);
        text = transform.Find("NameText").GetComponent<Text>();
        rejectIcon = transform.Find("PhoneRedIcon").gameObject;
                
        acceptIcon = transform.Find("PhoneGreenIcon").gameObject;
        GreenArrows = transform.Find("GreenArrows").gameObject;
        RedArrows = transform.Find("RedArrows").gameObject;
        //PhoneCanvas = transform.gameObject;
        timeText = transform.FindChild("CallTimerText").gameObject.GetComponent<Text>();
        IgnoreIcon = transform.FindChild("IgnoreIcon").gameObject;
        IgnoreText = transform.FindChild("IgnoreText").gameObject;
        keepCallIcon = transform.FindChild("KeepCallicon").gameObject;
        BackgroundCircles = transform.FindChild("BackgroundCircles").gameObject;
        Ringer = transform.FindChild("Ringer").gameObject;
        personImage = transform.FindChild("PersonImage").gameObject;
        timerTarget = 0.3f;
        startTime = Time.time;
        vibrating = false;
        interactionDurationNoted = false;

        disableEverything();
        state = PhoneConstants.IDLE;

    }

    // Update is called once per frame
    void Update()
    {
       
        if (!noticed && checkRayCollision())
        {
            noticed = true;
            firstNoticeTime = Time.time - spawnTime;
            
        }
        if (state == PhoneConstants.CALL_COMING_INTERACT)
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

        if (state == PhoneConstants.CALL_IGNORED && waitRoutineRunning == false)
        {
            disableEverything();
        }

        else if (state == PhoneConstants.CALL_REJECTED && waitRoutineRunning == false)
        {
            
            disableEverything();
        }

        else if (state == PhoneConstants.CALL_ACCEPTED && waitRoutineRunning == false)
        {

            if (countCheck == false)
            {
                startTime = Time.realtimeSinceStartup;
                countCheck = true;
                Ringer.GetComponent<AudioSource>().clip = phoneVoice;
                Ringer.GetComponent<AudioSource>().Play();
            }
            timePassed = Time.realtimeSinceStartup - startTime;
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
        else if (state == PhoneConstants.CALL_ENDED)
        {
            
            //pulsate Time Text
            if (waitRoutineRunning == false)
                disableEverything();

        }
        // Touch down, possible chance for a swipe

        else if (state == PhoneConstants.CALL_COMING_INTERACT)
        {
            Debug.Log("Checking for swipes");
            if ((int)trackedObj.index != -1 && device.GetTouchDown(Valve.VR.EVRButtonId.k_EButton_Axis0))
            {
                trackingSwipe = true;
                // Record start time and position
                mStartPosition = new Vector2(device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x,
                    device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y);
                mSwipeStartTime = Time.time;
                //Debug.Log("Inside if getTouchDown");
                if (!reactionTimeNoted)
                {
                    interactionTime = Time.time - spawnTime;
                    reactionTimeNoted = true;
                    Debug.Log("reaction time is being Noted for phoneOverlay");

                }
                Debug.Log("spawnTime is for PhoneOverlay" + spawnTime);
                Debug.Log("reaction Time  is for PhoneOverlay" + interactionTime);

            }
            // Touch up , possible chance for a swipe
            else if (device.GetTouchUp(Valve.VR.EVRButtonId.k_EButton_Axis0)&&trackingSwipe==true)
            {
                trackingSwipe = false;
               // trackingSwipe = true;
                checkSwipe = true;
                //Debug.Log("Inside if getTouchUp");
                endPosition = new Vector2(device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x,
                    device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y);
            }
            else if (trackingSwipe)
            {
                /*
                endPosition = new Vector2(device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x,
                    device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y);
                Debug.Log("Inside if trackingSwipe");*/

            }

            if (checkSwipe)
            {
                checkSwipe = false;
                float deltaTime = Time.time - mSwipeStartTime;
                Vector2 swipeVector = endPosition - mStartPosition;

                float velocity = swipeVector.magnitude / deltaTime;
                //Debug.Log(velocity);
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

    private bool checkRayCollision()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, 100.0F);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject == this.gameObject)
            {
                return true;
            }
        }
        return false;
    }



    IEnumerator UpdateTimeText(float duration)
    {
        waitRoutineRunning = true;
        float startTime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - startTime <= duration)
        {
           // Debug.Log("inside enumerator");
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
           // Debug.Log("inside enumerator");

            yield return null;
        }
        waitRoutineRunning = false;
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
        if (state == PhoneConstants.CALL_COMING_INTERACT || state == PhoneConstants.CALL_COMING)
        {
            disableIcons();
            disableArrows();
            text.text = "Call Rejected";
            //Debug.Log("Swipe left detected");
            StartCoroutine(WaitToDisableEnableRoutine(2));
            state = PhoneConstants.CALL_REJECTED;
            BackgroundCircles.SetActive(false);
            if (Ringer.GetComponent<AudioSource>().isPlaying)
                Ringer.GetComponent<AudioSource>().Pause();
            vibration = false;
            if (!interactionDurationNoted)
            {
                Debug.Log("interactionDuration being calculated from swipeLeft");
                Debug.Log("reaction time: "+interactionTime);
                Debug.Log("interaction Duration: " + (Time.time-spawnTime));
                interactionDuration = Time.time - interactionTime;
                interactionDurationNoted = true;
                notif.logInfo("phone overlay", "interaction duration", interactionDuration);
            }
        }

    }

    private void OnSwipeRight()
    {
        //Debug.Log("Swipe right detected");
        if (state == PhoneConstants.CALL_COMING_INTERACT || state == PhoneConstants.CALL_COMING)
        {
            text.text = "Henry In Call";
            state = PhoneConstants.CALL_ACCEPTED;
            disableIcons();
            disableArrows();
            timeText.gameObject.SetActive(true);
            keepCallIcon.SetActive(true);
            IgnoreIcon.SetActive(false);
            IgnoreText.SetActive(false);
            BackgroundCircles.GetComponent<Animator>().SetBool("pulsing", false);
            if (Ringer.GetComponent<AudioSource>().isPlaying)
                Ringer.GetComponent<AudioSource>().Pause();

            StartCoroutine(WaitToDisableEnableRoutine(1));
            vibration = false;
        }


    }

    private void OnSwipeTop()
    {

        //Debug.Log("Swipe Top");

        if (state == PhoneConstants.CALL_COMING_INTERACT || state == PhoneConstants.CALL_COMING)
        {
            text.text = "Call Ignored";

            disableIcons();
            disableArrows();
            StartCoroutine(WaitToDisableEnableRoutine(2));
            state = PhoneConstants.CALL_IGNORED;
            IgnoreIcon.SetActive(false);
            IgnoreText.SetActive(false);
            Ringer.GetComponent<AudioSource>().Pause();
            vibration = false;
            if (!interactionDurationNoted)
            {
                Debug.Log("interactionDuration being calculated from swipeTop");
                interactionDuration = Time.time - spawnTime;
                interactionDurationNoted = true;
                notif.logInfo("phone overlay", "interaction duration", interactionDuration);
            }
        }

    }

    private void onTouchPadPress()
    {
        // Debug.Log("Call ended");
        if (state == PhoneConstants.CALL_ACCEPTED)
        {
            text.text = "Call Ended";
            mMessageIndex = 5;
            StartCoroutine(WaitToDisableEnableRoutine(2));
            state = PhoneConstants.CALL_ENDED;
            Ringer.GetComponent<AudioSource>().Pause();
            if (!interactionDurationNoted)
            {
                Debug.Log("interactionDuration being calculated from touchPadPress");
                Debug.Log("reaction time is: " + interactionTime);
                Debug.Log("current time is: " + Time.time);
                interactionDuration = Time.time - spawnTime;
                interactionDurationNoted = true;
                notif.logInfo("phone overlay", "interaction duration", interactionDuration);
            }
            
        }

    }

    private void OnSwipeBottom()
    {
        //Debug.Log("Swipe Bottom");
        text.text = "Swipe down";
        mMessageIndex = 4;
        disableIcons();
        disableArrows();
    }

    void ActivateNotification()
    {
        // PhoneCanvas.SetActive(true);
        print(rejectIcon);
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
        text.gameObject.SetActive(true);
        timeText.text = "0:0:0";
        timeText.gameObject.SetActive(false);
        personImage.SetActive(true);
        

    }

    public void spawnNotification(bool haptic, bool audio)
    {
        //set all the required objects in the children active
        //phoneredIcon, phoneGreenIcon, BackgroundCircles, personImage, GreenArrows, RedArrows, NameText, IgnoreIcon, IgnoreText
        ActivateNotification();
        if(audio)
        {
            Ringer.GetComponent<AudioSource>().Play();
        }
        vibration = haptic;
        state = PhoneConstants.CALL_COMING_INTERACT;
        spawnTime = Time.time;

        noticed = false;
        firstNoticeTime = 0.0f;
        notificationOn = true;
        interactionDuration = 0.0f;
        interactionTime = 0.0f;
        interactionDurationNoted = false;
        reactionTimeNoted = false;
    

    }

    public void disableEverything()
    {
        if (!interactionDurationNoted && interactionTimeNoted)
        {
            Debug.Log("Calculating interaction duration. Time is:" + Time.time);
            Debug.Log("reactionTime is :" + interactionTime);

        }
        rejectIcon.SetActive(false);
        acceptIcon.SetActive(false);
        keepCallIcon.SetActive(false);
        GreenArrows.SetActive(false);
        RedArrows.SetActive(false);
        IgnoreText.SetActive(false);
        IgnoreIcon.SetActive(false);
        BackgroundCircles.SetActive(false);
        BackgroundCircles.GetComponent<Animator>().SetBool("pulsing", false);
        text.text = "Henry Calling";
        text.gameObject.SetActive(false);
        timeText.text = "0:0:0";
        timeText.gameObject.SetActive(false);
        Ringer.GetComponent<AudioSource>().clip = ringTone;
        Ringer.GetComponent<AudioSource>().Stop();        
        personImage.SetActive(false);

        if (notificationOn)
        {
           // Debug.Log("is reaction time noted? : " + reactionTimeNoted);
          //  Debug.Log("is interaction duration noted? : " + interactionDurationNoted);

           // notif.logInfo("PhoneOverlay", "reaction time", interactionTime);
          // notif.logInfo("PhoneOverlay", "noticing time", firstNoticeTime);
            //notif.logInfo("PhoneOverlay", "interaction duration", interactionDuration);

            notif.onCurrentInteractionComplete();
        }
        notificationOn = false;

        state = PhoneConstants.IDLE;
        countCheck = false;
        
    }
}
