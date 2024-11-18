using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventLogManager : MonoBehaviour
{
    /*
     * VARIABLES
     *
     * TextMeshPro
     * (GameObject für den Text)
     * Animator / DoTweenAnim
     * Zeit fürs ausblenden
     *
     * ________
     * LOGIC
     *
     * SetEventLog Method
     * - Text einblenden und setzen / Event anzeigen (Text)
     * - Text ausblenden
     *
     * ________
     *
     * Wir interagieren mit einem Objekt und wenn ein Event Log ausgegeben werden soll,
     * soll A) der Text gesetzt werden, und B) eingeblendet werden. Nach einer Zeit X wird der Text wieder ausgeblendet.
     *
     *
     */

    public Animator anim;
    public TextMeshProUGUI eventLog_Text;

    public float visibleTime = 2f;
    public float animationTime = .5f;
    private Coroutine _coroutine;
    
    public void SetEventLogText(string textValue)
    {
        if (_coroutine == null)
        {
            eventLog_Text.SetText(textValue);

            _coroutine = StartCoroutine(InitiateEventLog());
        }
    }

    IEnumerator InitiateEventLog()
    {
        if (!anim.enabled) anim.enabled = true;
        
        anim.Play("EventLogFadeIn");
        yield return new WaitForSeconds(visibleTime);
        anim.Play("EventLogFadeOut");

        yield return new WaitForSeconds(animationTime);
        _coroutine = null;
    }
}
