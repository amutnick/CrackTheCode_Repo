using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayMovie : MonoBehaviour {

    private MovieTexture _video;
    private Button _buttonBegin;

	// Use this for initialization
	void Start () {
        _video = ((MovieTexture)GetComponent<Renderer>().material.mainTexture);
        _video.Play();
        if (Button.FindObjectOfType<Button>())
            _buttonBegin = Button.FindObjectOfType<Button>();
        else
            _buttonBegin = null;
	}
	
	// Update is called once per frame
	void Update () {
        if(!_video.isPlaying)
        {
            if(_buttonBegin)
            {
                _buttonBegin = Button.FindObjectOfType<Button>();
                _buttonBegin.GetComponent<Animator>().SetBool("ShowButton", true);
            }
        }

        //if (Input.GetMouseButtonUp(0))
        //{
        //    Application.LoadLevel(1);
        //}
	}

   

    
}
