using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System;
using System.Text;

public class NotificationController : MonoBehaviour
{
    [SerializeField]
    GameObject phoneOverlay;
    [SerializeField]
    GameObject phoneBooth;
    [SerializeField]
    GameObject watchComplete;
    [SerializeField]
    GameObject popUp;
    [SerializeField]
    GameManager gm;
    [SerializeField]
    GameObject popUpList;
    [SerializeField]
    public Canvas controlCanvas;

    public InputField PID;

    bool phoneShowing;
    bool boothShowing;
    bool watchShowing;
    bool popUpShowing;

    bool notificationOn;
    bool participantIDEntered;

    bool trialStarted;
    bool testStarted;
   

    public ArrayList notificationOrder;
    string participantID;
    Dictionary<string, string> notificationMap;
    string[] notificationName;
    string[] notificationType;
    string fileName;
    System.Random random;
    ArrayList testCaseOrder;


    public GameObject phoneReceiver;  
    public GameObject vrtkRightController;
    public GameObject leftController;
    public GameObject rightController;

    public KeyCode keyPressForPhoneOverlay;
    public KeyCode keyPressForWatch;
    public KeyCode keyPressForPhoneBooth;
    public KeyCode keyPressForPopUp;

    

   static string interactionDurationFilePath = "Assets/UserStudy/interactionDuration.txt";
   static string noticingTimeFilePath = "Assets/UserStudy/noticingTime.txt";
   static string reactionTimeFilePath = "Assets/UserStudy/reactionTime.txt";

    public GameObject phoneBoothNew;

    int currentTestNumber = 0;
    // Use this for initialization
    void Start()
    {

        //read a file that contains participant ids and notifcation orders
        //ask the user to input participant id
        //compare and decide the order of notifications to present
        notificationOrder = new ArrayList();
        testCaseOrder = new ArrayList();
        // Build the Latin Square
        notificationName = new string[] { "WAT", "PHB", "POP", "PHO" };
        notificationType = new string[] { "VI", "AV", "HV" };
 
        notificationMap = new Dictionary<string, string>();

        notificationMap.Add("A", "WATVI");
        notificationMap.Add("B", "WATAV");
        notificationMap.Add("C", "WATHV");
        notificationMap.Add("D", "PHBVI");
        notificationMap.Add("E", "PHBAV");
        notificationMap.Add("F", "PHBHV");
        notificationMap.Add("G", "POPVI");
        notificationMap.Add("H", "POPAV");
        notificationMap.Add("I", "POPHV");
        notificationMap.Add("J", "PHOVI");
        notificationMap.Add("K", "PHOAV");
        notificationMap.Add("L", "PHOHV");

        fileName = "Assets/latinSquare.csv";
 

        trialStarted = false;
        testStarted = false;


       
        for (int i=0;i<24;i++)
        {
            testCaseOrder.Add("Blank");
        }

        popUp.GetComponent<BoxCollider>().enabled = false;
        phoneOverlay.GetComponent<SphereCollider>().enabled = false;


    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(keyPressForPhoneBooth))
        {
            enablePhoneBoothNotification(true, true);
        }

        if (Input.GetKeyDown(keyPressForPhoneOverlay))
        {
            enablePhoneOverlayNotification(true, true);
        }

        if (Input.GetKeyDown(keyPressForWatch))
        {
            enableWatchNotification(true, true);
        }

        if (Input.GetKeyDown(keyPressForPopUp))
        {
            Debug.Log("O is pressed");
            enablePopUpNotification(true, true);
        }

        if (trialStarted && participantIDEntered)
        {
            
        }

        if(testStarted)
        {
            //go through the notification order to determine which goes when
        }


    }

    public void enableWatchNotification(bool haptic, bool audio)
    {
        watchComplete.transform.GetComponent<WatchInteraction>().spawnNotification(haptic, audio);
    }

    public void enablePhoneOverlayNotification(bool haptic, bool audio)
    {
        phoneOverlay.GetComponent<SphereCollider>().enabled = true;
        phoneOverlay.transform.GetComponent<phoneOverlayInteraction>().spawnNotification(haptic, audio);
    }

    public void enablePhoneBoothNotification(bool haptic, bool audio)
    {

        
        Vector3 randomDirection3D = UnityEngine.Random.insideUnitSphere * 3;
        randomDirection3D += Camera.main.transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection3D, out hit, 10, -1);
       // phoneBooth.transform.position = hit.position;
        phoneBoothNew = Instantiate(phoneBooth,hit.position, Quaternion.identity);
        phoneBoothNew.SetActive(true);
        phoneBoothNew.GetComponent<phoneBoothInteraction>().leftController = leftController;
        phoneBoothNew.GetComponent<phoneBoothInteraction>().rightController = rightController;
        phoneBoothNew.GetComponent<phoneBoothInteraction>().vrtkRightcontroller = vrtkRightController;
        phoneBoothNew.GetComponent<phoneBoothInteraction>().notificationController = this.gameObject;
        phoneBoothNew.GetComponent<phoneBoothInteraction>().assignControllers(leftController, rightController);
        
        phoneBoothNew.GetComponent<phoneBoothInteraction>().spawnNotification(haptic, audio);

    }

    public void disablePhoneBoothNotification()
    {
        Debug.Log("inside disablephonePhone Booth");
        Destroy(phoneBoothNew.gameObject);
 

    }


    public void enablePopUpNotification(bool haptic, bool audio)
    {
       // Debug.Log("inside Popup notification");
        float minDistance = float.MaxValue;
        int minIndex = 0;
       // Debug.Log("pop up list count is : " + popUpList.transform.childCount);
        for (int i = 0; i < popUpList.transform.childCount; i++)
        {
            Transform t = popUpList.transform.GetChild(i);
            float distance = Vector3.Distance(t.position, Camera.main.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                minIndex = i;
            }
        }
        popUp.transform.position = popUpList.transform.GetChild(minIndex).position;
        popUp.transform.rotation = popUpList.transform.GetChild(minIndex).rotation;

        popUpInteraction pU = popUp.GetComponent<popUpInteraction>();
        //Debug.Log("before pU.Close");
        pU.Close();
        //Debug.Log("before pU.Show");
        popUp.GetComponent<BoxCollider>().enabled = true;
        pU.Show(haptic, audio);
        //Debug.Log("After pU.Show");

    }



    public void ParticipantIdEntered()
    {
        participantID = PID.text;
        PID.gameObject.SetActive(false);
        Debug.Log("Participant ID is : " + participantID);
        //once Pid is entered, you can generate the notificaiton oder and the order of test cases
        generateNotificationOrder();
        generateTestCaseOrder();
        participantIDEntered = true;

    }

    public void onTrialbuttonPress()
    {
        //you can only start a trial after entering participant ID
        if (participantIDEntered)
        {
            Debug.Log("inside trial buttonPress");
            trialStarted = true;
            testStarted = false;
            gm.StartTrials();
        }

    }

    public void onTestButtonPress()
    {
        //you can only start the test after you have done the trials

            Debug.Log("inside test buttonPress");
            trialStarted = false;
            testStarted = true;
            gm.startTestCases();

    }

    void generateNotificationOrder()
    {
        using (StreamReader reader = new StreamReader(fileName))
        {
            if (reader == null)
            {
                Debug.Log("couldn't read file");
            }
            else
            {
                string line;


                while ((line = reader.ReadLine()) != null)
                {

                    //Define pattern
                    Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                    //Separating columns to array
                    string[] X = CSVParser.Split(line);

                    if (X[0] == participantID)
                    {
                        for (int i = 1; i < X.Length; i++)
                        {
                            notificationOrder.Add(notificationMap[X[i]]);
                        }
                    }


                }
            }
        }
    }

    void generateTestCaseOrder()
    {
        //generate 12 random numbers from 1 to 24
        //insert blanks in those indices to create a 24 item array list
        random = new System.Random((int)Time.realtimeSinceStartup);
        for (int i=0;i<12;i++)
        {
            int randomNumber = random.Next(0, 24);//random.next gives the number <upper limit
            if (testCaseOrder[randomNumber].Equals("Blank"))
            {
                testCaseOrder[randomNumber] = "None";
            }
            else
                i--;
        }
        int j = 0;
        int n = 0;

        
        for (int i = 0; i < testCaseOrder.Count; i++)
        {
            if (testCaseOrder[i].Equals("Blank"))
            {
                
                testCaseOrder[i] = notificationOrder[j++];
            }
            if (testCaseOrder[i].Equals("None"))
                n++;

        }
        //Debug.Log("number of times Blank occurs is :"+j);
        //Debug.Log("number of times None occurs is :" + n);
        String s = "";
        //print the generated sequence
        for (int i=0;i<testCaseOrder.Count;i++)
        {
            s += testCaseOrder[i] + ",";
        }

        controlCanvas.transform.FindChild("OrderShow").gameObject.GetComponent<Text>().text = s;
    }
    public void logNoticed(float timeTaken)
    {
        var rows = new List<string>();
        rows.Add(notificationOrder[gm.trialNumber].ToString());
        rows.Add("Noticing Time");
        rows.Add(timeTaken.ToString());
        rows.Add(string.Empty);

        writeToFile("Noticing Time", rows);
    }
    public void logInfo(string notificationObject,string timeMetric, float timeTaken)
    {
        //Debug.Log("LogInfo is being called" + notificationObject);
        List<string> rows = new List<string>();
        rows.Add(participantID);
        rows.Add(testCaseOrder[gm.getTestNumber()-1].ToString());
        rows.Add(notificationObject);
        rows.Add(timeMetric);
        rows.Add(timeTaken.ToString());
        rows.Add(string.Empty);
        string filePath = "";
        if (timeMetric.Contains("reaction"))
        {
            filePath = "Assets/UserStudy/reactionTime.txt";
           // Debug.Log("reaction file is chosen");
        }
        else if (timeMetric.Contains("interaction"))
        {
            //log to the file which contains interactionTimes
            filePath = "Assets/UserStudy/interactionDuration.txt";
           // Debug.Log("interaction file is chosen");
        }
        else if (timeMetric.Contains("noticing"))
        {
            //log to the file which contains noticingTimes
            filePath = "Assets/UserStudy/noticingTime.txt";
            Debug.Log("noticing file is chosen");
        }

        // create the rows you need to append
        StringBuilder sb = new StringBuilder();
        //Debug.Log("after stringBuilder");
        foreach (string row in rows)
            sb.AppendFormat(",{0}", row);
        Debug.Log("string built" + sb); ;
        // flush all rows once time.
        File.AppendAllText(filePath, sb.ToString() + Environment.NewLine, Encoding.UTF8);


    }

    public static void logInformation(string notificationObject, string timeMetric, float timeTaken)
    {  //dont forget to add the participant ID and the notification ID
        var rows = new List<string>();
        rows.Add(notificationObject);
        rows.Add(timeMetric);
        rows.Add(timeTaken.ToString());
        rows.Add(string.Empty);

        //writeToFile(timeMetric, rows);
        //log to the file which contains reactionTimes
       // Debug.Log("notification Object : " + notificationObject);
      //  Debug.Log("timeMetric : " + timeMetric);
      //  Debug.Log("Time Taken : " + timeTaken);
    }

    public static void logInformation(string notificationObject, int lastState, string timeMetric, float timeTaken)
    {  //dont forget to add the participant ID and the notification ID

        var rows = new List<string>();
        rows.Add(notificationObject);
        rows.Add(lastState.ToString());
        rows.Add(timeMetric);
        rows.Add(timeTaken.ToString());
        rows.Add(string.Empty);

        writeToFile(timeMetric, rows);

    }

    public static void writeToFile(string timeMetric, List<string> data)
    {

        //have to add participant ID to each line. Is it good to make it a static file??

        string filePath="";
        if (timeMetric.Contains("reaction"))
        {
            filePath = reactionTimeFilePath;
        }
        else if (timeMetric.Contains("interaction"))
        {
            //log to the file which contains interactionTimes
            filePath = interactionDurationFilePath;
        }
        else if (timeMetric.Equals("notice"))
        {
            //log to the file which contains noticingTimes
            filePath = noticingTimeFilePath;
        }

        // create the rows you need to append
        StringBuilder sb = new StringBuilder();
       
        foreach (string row in data)
            sb.AppendFormat(",{0}", row);

        // flush all rows once time.
        File.AppendAllText(filePath, sb.ToString()+ Environment.NewLine, Encoding.UTF8);
    }

    public String getCurrentTestNotif(int index)

    {
        if(participantIDEntered && index>=0 && index<24)
            return (String) testCaseOrder[index];
        return "None";
    }

    public void currentTestCaseNumber(int x)
    {
        currentTestNumber = x;
        //update the UI
        controlCanvas.transform.FindChild("ShowCurrenttestCase").GetComponent<Text>().text = x.ToString();
    }

    public void onCurrentInteractionComplete()
    {
        gm.setNotificationHandled();
    }


}

