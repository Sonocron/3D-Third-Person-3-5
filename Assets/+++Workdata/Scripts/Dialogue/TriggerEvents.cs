using UnityEngine;
using UnityEngine.Events;

public class TriggerEvents : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string NoTag = "Untagged";
    
    #region Inspector

    [Tooltip("Invoked when OnTriggerEnter() is called.")]
    [SerializeField] private UnityEvent<Collider> onTriggerEnter;
    
    [Tooltip("Invoked when OnTriggerExit() is called.")]
    [SerializeField] private UnityEvent<Collider> onTriggerExit;

    [Tooltip("Enable to filter the interacting collider by a specified tag.")]
    [SerializeField] private bool filterOnTag = true;

    [Tooltip("Tag of the interacting collider to filter on.")]
    [SerializeField] private string reactOn = PlayerTag;

    #endregion

    #region Unity Event Functions

    private void OnValidate()
    {
       
        // Replaces an 'empty' reactOn filed with "Untagged".
        if (string.IsNullOrWhiteSpace(reactOn))
        {
            reactOn = NoTag;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (filterOnTag && !other.CompareTag(reactOn)) { return; }
        
        onTriggerEnter.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (filterOnTag && !other.CompareTag(reactOn)) { return; }
        
        onTriggerExit.Invoke(other);
    }

    #endregion
}