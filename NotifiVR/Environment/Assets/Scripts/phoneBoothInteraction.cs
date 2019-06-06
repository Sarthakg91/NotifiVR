using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class phoneBoothInteraction : MonoBehaviour {
    [SerializeField]
    public GameObject phoneReceiver;
    [SerializeField]
    public GameObject leftController;
    [SerializeField]
    public GameObject rightController;
    [SerializeField]
    public GameObject vrtkRightcontroller;

    SteamVR_TrackedObject leftTrackedObj;
    SteamVR_TrackedObject rightTrackedObj;


    float firstNoticeTime;
    bool noticed;
    float spawnTime;
    bool entered;
    bool interacted;
    float reactionTime;
    float interactionDuration;

    public GameObject notificationController;

    // Use this for initialization
    void Start () {
        interacted = false;
        noticed = false;
        leftTrackedObj = leftController.GetComponent<SteamVR_TrackedObject>();
        rightTrackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
        interacted = false;
        interacted = false;
        noticed = false;

    }

    public void assignControllers(GameObject lt, GameObject rt)
    {
        leftController = lt;
        rightController = rt;
        leftTrackedObj = leftController.GetComponent<SteamVR_TrackedObject>();
        rightTrackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
    }

    void Awake()
    {
        leftTrackedObj = leftController.GetComponent<SteamVR_TrackedObject>();
        rightTrackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
        interacted = false;
        interacted = false;
        noticed = false;
    }
	
	// Update is called once per frame
	void Update () {
        //firstNoticeTime += Time.deltaTime;
        if (!noticed && checkRayCollision())
        {
            noticed = true;
            firstNoticeTime=Time.time-spawnTime;
            Debug.Log("first notice time : " + firstNoticeTime);

        }
    }

    public void spawnNotification(bool haptic, bool audio)
    {
        Debug.Log("inside phoneBooth spawnNotification");
        if(haptic)
        {
            float distToRightController = Vector3.Distance(rightController.transform.position, phoneReceiver.transform.position);
            float distToLeftController = Vector3.Distance(leftController.transform.position, phoneReceiver.transform.position);
            print("Distance to Right: " + distToRightController);
            print("Distance to Left: " + distToLeftController);

            if (distToRightController > distToLeftController)
            {
                //vibrate left
                RumbleController(leftTrackedObj, 0.5f, 0.5f);
            }
            else
            {
                //vibrate right
                RumbleController(rightTrackedObj, 0.5f, 0.5f);
            }
        }
        if (audio)
        {
            phoneReceiver.GetComponent<receiverInteraction>().unMuteAudio(); 
        }
        else if (!audio)
        {
            phoneReceiver.GetComponent<receiverInteraction>().muteAudio();
        }

        spawnTime = Time.time;
        phoneReceiver.GetComponent<Animator>().SetBool("ringing", true);
    }

    void RumbleController(SteamVR_TrackedObject controller, float duration, float strength)
    {
        StartCoroutine(RumbleControllerRoutine(controller, duration, strength));
    }

    System.Collections.IEnumerator RumbleControllerRoutine(SteamVR_TrackedObject controller, float duration, float strength)
    {
        strength = Mathf.Clamp01(strength);
        float startTime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - startTime <= duration)
        {
            int valveStrength = Mathf.RoundToInt(Mathf.Lerp(0, 3999, strength));

            SteamVR_Controller.Input((int)controller.index).TriggerHapticPulse((ushort)valveStrength);

            yield return null;
        }
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

    public void onGrabCheck()
    {
        //check what object is being grabbed
        //if it is the door, then register. 
        Debug.Log("ongrabCheck is being called");
        if(!entered)
        {
            Debug.Log("interacted with Door");
            reactionTime = Time.time - spawnTime;
            entered = true;
        }
               

    }

    public void setInteracted(bool interactDone)
    {
        Debug.Log("setInteracted is being called");
        interacted = interactDone;
        //if already inside
        //entered = true;
        
    }

    public void doneInteracting ()
    {
        //log information
        //reset everything
        //disable phoneBooth
        Debug.Log("doneInteracting is being Called");
        Debug.Log("Entered is : " + entered);
        Debug.Log(" Interacted is :" + interacted);
        if(entered&&interacted)
        {
            Debug.Log("entered and interacted");
            interactionDuration = Time.time - spawnTime;
            notificationController.GetComponent<NotificationController>().logInfo("phoneBooth", "reaction Time", reactionTime);
            notificationController.GetComponent<NotificationController>().logInfo("phoneBooth", "noticing Time", firstNoticeTime);
            notificationController.GetComponent<NotificationController>().logInfo("phoneBooth", "interaction duration", interactionDuration);
            reset();
            //reset
            notificationController.GetComponent<NotificationController>().onCurrentInteractionComplete();
            notificationController.GetComponent<NotificationController>().disablePhoneBoothNotification();
            
        }
    }

    public void reset()
    {
        firstNoticeTime = 0.0f;
        noticed = false;
        spawnTime = 0.0f;
        entered = false;
        interacted = false;
        reactionTime = 0.0f;
    }
    
}
