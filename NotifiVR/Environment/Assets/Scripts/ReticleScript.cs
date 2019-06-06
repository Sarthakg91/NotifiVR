using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleScript : MonoBehaviour {
    public Canvas reticle;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        reticle.gameObject.SetActive(false);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, 100.0F);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.layer == 8 && hits[i].collider.gameObject.activeSelf)
            {
                transform.position = hits[i].point;
                reticle.gameObject.SetActive(true);
            }
        }
	}
}
