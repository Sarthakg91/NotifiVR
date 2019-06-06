using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallTargetScript : MonoBehaviour {
    public GameManager gm;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ball")
        {
            print("Ball entered");
            gm.trialCompleted();
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
