using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WatchInteraction : MonoBehaviour
{
    [SerializeField]
    GameObject cameraRig;
    public GameObject leftController;


    SteamVR_Camera cameraScript;
    public GameObject canvasSet;
    public GameObject timeCanvas;
    Animator watchAnimator;
    SphereCollider selfCollider;
    List<Collider> canvasColliders;
    public KeyCode keyPress;
    public AudioSource speaker;
    public GameObject lightSource;
    public AudioClip NotificationPopSound;
    public GameObject notificationController;


    private SteamVR_TrackedObject trackedObj;

    float timePassed;
    int hours;
    int minutes;
    int seconds;
    Text timeText;
    float timerTarget = 4.0f;
    float fanInTimerTarget = 5.0f;
    private bool notificationCanvasOn;
    private bool isOn = false;
    float notificationSpawnTime;
    float reactionTime;
    float firstNoticeTime;
    float interactionDuration;
    bool noticed;
    bool reacted;
    bool interacted;
    bool ended;


    // Use this for initialization
    void Start()
    {
        cameraScript = cameraRig.transform.Find("Camera (eye)").GetComponent<SteamVR_Camera>();
        watchAnimator = canvasSet.GetComponent<Animator>();
        selfCollider = GetComponent<SphereCollider>();
        canvasColliders = new List<Collider>();
        foreach (Transform t in canvasSet.transform)
        {
            canvasColliders.Add(t.gameObject.GetComponent<BoxCollider>());
        }
        notificationCanvasOn = false;
        timeCanvas.SetActive(true);
        canvasSet.SetActive(false);
        trackedObj = leftController.GetComponent<SteamVR_TrackedObject>();

        noticed = false;
        interacted = false;
        ended = false;


        // disable notification canavases by default
        // enable time canvas, which shows time  by default
        // wait for keyboard trigger to enable notification canvases and disable time canvas
        // once reading is done, back to time canvas. 
        // 



    }
    private void Update()
    {
        if (Input.GetKeyDown(keyPress))
        {
            spawnNotification(true, true);



        }
        if (!notificationCanvasOn)
        {  //shows the time passed if the notification is not on
            timePassed = Time.time;
            hours = (int)timePassed / 3600;
            minutes = ((int)timePassed % 3600) / 60;
            seconds = ((int)timePassed % 3600) % 60;
            string timeString = hours.ToString() + " : " + minutes.ToString() + " : " + seconds.ToString();
            timeText = timeCanvas.transform.FindChild("TimeText").gameObject.GetComponent<Text>();
            if (timeText == null)
            {
                Debug.Log("Couldn't find text component");
            }
            timeText.text = timeString;
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

        while (Time.realtimeSinceStartup - startTime <= duration)
        {
            int valveStrength = Mathf.RoundToInt(Mathf.Lerp(0, 3999, strength));

            SteamVR_Controller.Input((int)trackedObj.index).TriggerHapticPulse((ushort)valveStrength);

            yield return null;
        }
    }

    private void TriggerHapticPulse(int v)
    {
        throw new NotImplementedException();
    }

    void FixedUpdate()
    {
        if (notificationCanvasOn)
        {
            //Debug.Log("notification canvas is on");
            RaycastHit[] hits;
            hits = Physics.RaycastAll(cameraScript.GetRay(), 10.0F).OrderBy(h => h.distance).ToArray();
            bool watching = false;
            //start timer. if watching becomes true, reset timer. once timer is over, reset notification
            bool firstCanvasFound = false;
            foreach (Collider c in canvasColliders)
            {
                c.gameObject.transform.localScale = Vector3.one;
            }
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == selfCollider && !watching)
                {
                    watching = true;
                    watchAnimator.SetBool("Watching", watching);
                    lightSource.SetActive(false);
                    if (!isOn)
                    {
                      
                        
                        isOn = true;

                        //first time it happens note down the time that is the reaction time
                        if (!reacted)
                        {
                            reactionTime = Time.realtimeSinceStartup - notificationSpawnTime;
                            reacted = true;
                            timerTarget = 4.0f;
                        }

                    }
                    timerTarget = 4.0f;//reset timer
                }
                if (!firstCanvasFound && canvasColliders.Contains(hit.collider))
                {
                    firstCanvasFound = true;
                    watching = true;
                    if (!isOn)
                    {
                        
                        isOn = true;
                        if (!reacted)
                        {
                            reactionTime = Time.realtimeSinceStartup - notificationSpawnTime;
                            reacted = true;
                        }
                    }
                    watchAnimator.SetBool("Watching", watching);
                    lightSource.SetActive(false);
                    timerTarget = 4.0f;//reset timer
                    hit.collider.gameObject.transform.localScale = Vector3.one * 3;
                }
            }
            timerTarget -= Time.deltaTime;
            // Debug.Log(timerTarget);

            if (watching == false && timerTarget < 0.0f)
            {
                notificationCanvasOn = false;
                timeCanvas.SetActive(true);
                canvasSet.SetActive(false);

                // end of interaction
                if (reacted && !ended)
                {
                    interactionDuration = Time.realtimeSinceStartup - notificationSpawnTime;
                    ended = true;
                    reacted = false;
                    notificationController.GetComponent<NotificationController>().logInfo("Watch", "reaction Time", reactionTime);
                    notificationController.GetComponent<NotificationController>().logInfo("Watch", "interaction duration", interactionDuration);

                    notificationController.GetComponent<NotificationController>().onCurrentInteractionComplete();
                    //Debug.Log("reaction Time : " + reactionTime);
                    //Debug.Log("interaction Duration : " + interactionDuration);
                    lightSource.SetActive(false);
                }

            }
            if (watching == false && timerTarget < 2.0f)
            {
                watchAnimator.SetBool("Watching", watching);
                if (isOn)
                {
                   
                    isOn = false;
                }
            }
        }


    }

    //function to be called from the manager script
    public void spawnNotification(bool haptic, bool audio)
    {
        interactionDuration = 0.0f;
        reactionTime = 0.0f;

        timeCanvas.SetActive(false);
        canvasSet.SetActive(true);
        notificationCanvasOn = true;
        //switch on the lightsource as well
        lightSource.SetActive(true);

        timerTarget = 40.0f;
        notificationSpawnTime = Time.realtimeSinceStartup;

        if (haptic)
            RumbleController(0.5f, 0.5f);

        if (audio)
        {
            speaker.clip = NotificationPopSound;
            speaker.Play();
        }

        notificationSpawnTime = Time.realtimeSinceStartup;
        ended = false;
        reacted = false;
    }


}

