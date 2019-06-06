using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class popUpInteraction : MonoBehaviour {
    public GameObject big;
    public GameObject small;
    public AudioSource smallSpeaker;
    public AudioSource bigSpeaker;
    public NotificationController notif;
    public GameObject leftController;
    public GameObject rightController;
    private SteamVR_TrackedObject trackedObjLeftController;
    private SteamVR_TrackedObject trackedObjRightController;


    bool noticed;
    float firstNoticeTime;
    bool haptic;
    bool aud;
    bool pointerTouched;

    float spawnTime;
    float noticeTime;
    float reactionTime;
    float interactionDuration;

    bool noticeTimeNoted = false;
    bool interactionTimeNoted = false;
    bool reactionTimeNoted = false;

	// Use this for initialization
	void Start () {
        haptic = false;
        aud = false;
        trackedObjLeftController = leftController.GetComponent<SteamVR_TrackedObject>();
        trackedObjRightController = rightController.GetComponent<SteamVR_TrackedObject>();
        pointerTouched = false;
    }
	
	// Update is called once per frame
	void Update () {
        
        if (!noticeTimeNoted && !noticed && checkRayCollision())
        {
            noticed = true;
            firstNoticeTime = Time.time - spawnTime;
            noticeTimeNoted = true;
            
        }



    }

    public void Show(bool h, bool a)
    {
        small.SetActive(true);
        big.SetActive(false);
        spawnTime = Time.time;
        noticed = false;
        firstNoticeTime = 0.0f;
        noticed = false;
        noticeTimeNoted = false;
        reactionTimeNoted = false;
        interactionTimeNoted = false;
        pointerTouched = false;
        haptic = h;
        aud = a;

        if (aud)
            smallSpeaker.Play();

        if(haptic)
        {
            //check which controller is nearby
            //vibrate that controller

            float distToRightController = Vector3.Distance(rightController.transform.position, small.transform.position);
            float distToLeftController = Vector3.Distance(leftController.transform.position, small.transform.position);
            print("Distance to Right: " + distToRightController);
            print("Distance to Right: " + distToLeftController);

            if(distToRightController>distToLeftController)
            {
                //vibrate left
                RumbleController(trackedObjLeftController,0.5f, 0.5f);
            }
            else
            {
                //vibrate right
                RumbleController(trackedObjRightController, 0.5f, 0.5f);
            }

        }
    }
    void RumbleController(SteamVR_TrackedObject controller,float duration, float strength)
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
    public void Expand()
    {
        big.SetActive(true);
        if (aud)
            bigSpeaker.Play();
        small.SetActive(false);
    }

    public void Close()

    {

        small.SetActive(false);
        big.SetActive(false);
        if (!interactionTimeNoted)
        {
            interactionDuration = Time.time - spawnTime;
            interactionTimeNoted = true;

        }

        if (pointerTouched && (reactionTime!=0.0f||interactionDuration!=0.0f))
        {
            Debug.Log("informationBeingLoggedFrom Pop up");
            notif.logInfo("Popup", "interaction duration", interactionDuration);
            notif.logInfo("Popup", "reaction time", reactionTime);
            notif.logInfo("Popup", "noticing time", firstNoticeTime);
            interactionDuration = 0.0f;
            reactionTime = 0.0f;
            firstNoticeTime = 0.0f;

            
            notif.onCurrentInteractionComplete();
        }



            //interaction Duration


        }

    public void pointerEntered()
    {
        
        //reaction time
        if (!pointerTouched&&!reactionTimeNoted)
        {
            pointerTouched = true;
            reactionTime = Time.time - spawnTime;
            Debug.Log("reaction recorded in popUpInteraction "+reactionTime);
            reactionTimeNoted = true;
            //get current time 
            //subtract spawn time
            //store reaction time
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
}
