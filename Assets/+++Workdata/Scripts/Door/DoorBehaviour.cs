using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    /*
     * VARIABLES
     * Audio CLips - Mehrere Audio Sounds Öffnen, Schließen, Bewegung - Zufällig anspielen
     * Audio Source
     * Animation
     * Collider der Tür
     * Einen Bool für den aktuellen Zustand der Tür!
     * Floats für zeitliche Verzögerung
     *
     * LOGIC
     * Eine Methode für die Türsteuerung
     *
     * A) Ist die Tür offen?
     * - Erst die Animation und den Bewegungssound abspielen, dann das Schließgeräusch
     *
     * B) Ist die Tür geschlossen?
     * - Erst das Öffnengeräusch und dann die Aniamtion und den Bewegungssound
     *
     * METHODS
     * Door Toggle - mit einer Coroutine
     * Evtl. Zufälligen Sound abspielen
     */
    
    public AudioSource audioSource;

    public AudioClip doorOpenSound, doorCloseSound, doorMovementSound;

    public Animator anim;

    public bool isOpen;

    public float delayTimeOpen, delayTimeClose;

    public Collider[] colliders;

    public void ToggleDoor()
    {
        if (isOpen)
        {
            StartCoroutine(InitiateClose());
        }
        else
        {
            StartCoroutine(InitiateOpen());
        }

        isOpen = !isOpen;
    }

    IEnumerator InitiateOpen()
    {
        audioSource.PlayOneShot(doorOpenSound);
        yield return new WaitForSeconds(delayTimeOpen);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
        
        if (!anim.enabled) anim.enabled = true;
        anim.Play("Open");
        audioSource.PlayOneShot(doorMovementSound);

        yield return new WaitForSeconds(delayTimeClose);
        
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }
    }
    
    IEnumerator InitiateClose()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
        
        if (!anim.enabled) anim.enabled = true;
        anim.Play("Close");
        audioSource.PlayOneShot(doorMovementSound);
        
        yield return new WaitForSeconds(delayTimeClose);

        audioSource.PlayOneShot(doorCloseSound);
        
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }
    }
    
}
