using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class receiverInteraction : MonoBehaviour {

    // Use this for initialization
    public AudioSource speaker;
    public AudioClip phoneVoice;
    public GameObject phoneBooth;
    bool isPicked;
    float timeTarget;
	void Start () {
        isPicked = false;
        timeTarget = 1.5f;
        

	}
	
	// Update is called once per frame
	void Update () {
		if(isPicked && !speaker.isPlaying)
        {
            timeTarget -= Time.deltaTime;
            if(timeTarget<0.0f )
            {
                speaker.Play();
            }
        }
	}

    public void muteAudio()
    {
        speaker.mute = true;
    }

    public void unMuteAudio()
    {
        speaker.mute = false;
    }
    public void playRingtone()
    {
        speaker.Play();

    }

    public void onGrab()
    {
        Debug.Log("SHIT HAS BEEN FUCKING GRABBED!");
        
        speaker.Pause();
        speaker.clip = phoneVoice;
        speaker.mute = false;
        isPicked = true;
        phoneBooth.GetComponent<phoneBoothInteraction>().setInteracted(isPicked);
    }

    public void onUnGrab()
    {
        Debug.Log("SHIT HAS BEEN FUCKING UNGRABBED! THOUGH THAT IS NOT A WORD");
        speaker.Pause();
        this.transform.GetComponent<Animator>().SetBool("ringing", false);
        isPicked = false;
        timeTarget = 1.5f;
        phoneBooth.GetComponent<phoneBoothInteraction>().doneInteracting();
    }



}
