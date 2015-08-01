using UnityEngine;

public class ButtonSound : MonoBehaviour {
    public AudioClip buttonSound;
    public AudioSource audioSource;

    public void OnMouseEnter()
    {
        audioSource.PlayOneShot(buttonSound);
        Debug.Log("Mouse Entered");
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        audioSource.PlayOneShot(buttonSound);
        Debug.Log("Mouse Entered");
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        audioSource.PlayOneShot(buttonSound);
        Debug.Log("Mouse Entered");
    }

    public void OnTriggerEnter(Collider collision)
    {
        audioSource.PlayOneShot(buttonSound);
        Debug.Log("Mouse Entered");
    }

}