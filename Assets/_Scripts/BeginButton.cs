using UnityEngine.UI;
using System.Collections;
using UnityEngine;

public class BeginButton : MonoBehaviour {
    public Button beginbutton;
    public AudioClip ButtonAudio;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
    
	}

    public void PlaySound()
    {
        beginbutton.GetComponent<AudioSource>().PlayOneShot(ButtonAudio);

    }
}
