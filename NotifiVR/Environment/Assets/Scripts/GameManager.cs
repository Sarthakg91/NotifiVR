using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField]
    GameObject ballTarget;
    [SerializeField]
    GameObject ball;
    [SerializeField]
    List<GameObject> ballSpawns;
    [SerializeField]
    List<GameObject> targetSpawns;
    [SerializeField]
    NotificationController notif;

  
    
    public int trialNumber;
    private int currentBallSpawn;
    private int currentTargetSpawn;
    private int testNumber;
   


    System.Random rand;
    bool trialsGoingOn;
    bool testsGoingOn;
    bool currentTaskStarted;
    bool currentTaskCompleted;
    bool notificationHandled;
    
	// Use this for initialization
	void Start () {
        rand = new System.Random();
        currentBallSpawn = rand.Next(ballSpawns.Count);
        currentTargetSpawn = rand.Next(targetSpawns.Count);
        trialsGoingOn = false;
        testsGoingOn = false;
        currentTaskCompleted = false;
        notificationHandled = false;
        testNumber = 1;
	}

    public void StartTrials()
    {
        trialNumber = 0;
        currentBallSpawn = rand.Next(ballSpawns.Count);
        currentTargetSpawn = rand.Next(targetSpawns.Count);
        trialsGoingOn = true;
        testsGoingOn = false;
        startNewTrial();
    }
	
    public void setNotificationHandled()
    {
        //to be set by the notification controller
        notificationHandled = true;
    }

    public void startTestCases()
    {
        Debug.Log("inside start test cases");
        testNumber = 0;
        currentBallSpawn = rand.Next(ballSpawns.Count);
        currentTargetSpawn = rand.Next(targetSpawns.Count);
        testsGoingOn = true;
        trialsGoingOn = false;
        startNewTest();
    }

	// Update is called once per frame
	void Update () {
        
        if (trialsGoingOn && currentTaskCompleted)
        {
           // Debug.Log("Starting new trial from Update");
            startNewTrial();
        }

        if (testsGoingOn && currentTaskCompleted && notificationHandled)
        {
            //Debug.Log("Starting new test from Update");
            startNewTest();      
        }
	}
    public int getTestNumber()
    {
        return testNumber;
    }

    public delegate void NotifFormDelegate(bool haptic, bool audio);

    //Turns the string from the notification order into the appropriate method and variables.
    public void executeNotification(String notification)
    {
        //Debug.Log("Inside notificationExecution");
        if (!notification.Equals("None" ))
        {
          //  Debug.Log("Inside notification !=null");
            notificationHandled = false;
            String form = notification.Substring(0, 3);
            String type = notification.Substring(3, 2);
            int waitTime = rand.Next(5, 15);
            NotifFormDelegate del = notif.enablePhoneOverlayNotification; ;
            switch (form)
            {
                case "PHO":
                    del = notif.enablePhoneOverlayNotification;
                    break;
                case "PHB":
                    del = notif.enablePhoneBoothNotification;
                    break;
                case "WAT":
                    del = notif.enableWatchNotification;
                    break;
                case "POP":
                    del = notif.enablePopUpNotification;
                    break;
                default:
                    break;
            }
            bool haptic = false;
            bool audio = false;
            switch(type)
            {
                case "VI":
                    //visual
                    break;
                case "AV":
                    //audio + visual
                    audio = true;
                    break;
                case "HV":
                    //haptic + visual
                    haptic = true;
                    break;
                default:
                    break;
            }
           // Debug.Log("haptic is :"+haptic);
           // Debug.Log("audio is :" + audio);
            StartCoroutine(waitThenShowNotif(del, waitTime, haptic, audio));
        }
        else
        {
            //Debug.Log("Inside notification is none");
            notif.onCurrentInteractionComplete();
        }
        
    }

    public IEnumerator waitThenShowNotif(NotifFormDelegate del, int seconds, bool haptic, bool audio)
    {
        yield return new WaitForSeconds(seconds);
        del(haptic, audio);
    }

    public void trialCompleted()
    {
        //Debug.Log("inside trial completed");
        if (trialsGoingOn)
        {
            print("Trial Completed:" + trialNumber);
            trialNumber++;
        }
        else if (testsGoingOn)
        {  
            print("Test Completed:" + testNumber);
            
        }
        currentTaskStarted = false;
        currentTaskCompleted = true;

       // Debug.Log("Ball Thrown in Hell: IsItTest" + testsGoingOn + ", TrialsOn: " + trialsGoingOn + ", NotifiHandled: " + notificationHandled + ", TaskCompleted: " + currentTaskCompleted);
        //set the boolean to indicate you have completed the current task
    }

    public void startNewTrial()
    {

        print("New Trial #" + trialNumber);
        StartCoroutine(spawnNew(2));//show a ball in 2 seconds
        currentTaskStarted = true;
        currentTaskCompleted = false;

    }
    public void startNewTest()
    {
        if (testNumber < 24)
        {      
            String notification = notif.getCurrentTestNotif(testNumber);
            Debug.Log("currentTest notification is :" + notification);
            notif.currentTestCaseNumber(testNumber);
            print("New Test #" + testNumber + ", " + notification);
            StartCoroutine(spawnNew(2));
            executeNotification(notification);
            currentTaskStarted = true;
            currentTaskCompleted = false;
            testNumber++;
        }
    }

    public IEnumerator spawnNew(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        int gen = rand.Next(ballSpawns.Count);
        while (gen == currentBallSpawn)
        {
            gen = rand.Next(ballSpawns.Count);
        }
        currentBallSpawn = gen;
        gen = rand.Next(targetSpawns.Count);
        while (gen == currentTargetSpawn)
        {
            gen = rand.Next(targetSpawns.Count);
        }
        currentTargetSpawn = gen;
        Debug.Log("target being shown is number: " + gen);

        GameObject newBallTarget = Instantiate(ballTarget, targetSpawns[currentTargetSpawn].transform.position, Quaternion.identity);
        newBallTarget.GetComponent<BallTargetScript>().gm = this;
        GameObject newBall = Instantiate(ball, ballSpawns[currentBallSpawn].transform.position, Quaternion.identity);
    }
}
