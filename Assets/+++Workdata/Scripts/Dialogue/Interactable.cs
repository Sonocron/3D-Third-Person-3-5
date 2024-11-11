using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    #region Inspector

    [Tooltip("Invoked when the player interacts with the Interactable.")]
    [SerializeField] private UnityEvent onInteracted;

    [Tooltip("Invoked when the player selects this Interactable, and they are able to interact with it.")]
    [SerializeField] private UnityEvent onSelected;

    [Tooltip("Invoked when the player deselects this Interactable, and they stop being able to interact with it.")]
    [SerializeField] private UnityEvent onDeselected;

    #endregion

    #region Unity Event Functions

    private void Start()
    {
        List<Interaction> interactions = GetComponentsInChildren<Interaction>(true).ToList();

        if (interactions.Count > 0)
        {
            interactions[0].gameObject.SetActive(true);
        }
    }

    #endregion

    public void Interact()
    {
        Interaction interaction = FindActiveInteraction();

        if (interaction != null)
        {
            interaction.Execute();
        }
        
        onInteracted.Invoke();
    }

    public void Select()
    {
        onSelected.Invoke();
    }

    public void Deselect()
    {
        onDeselected.Invoke();
    }

    private Interaction FindActiveInteraction()
    {
        return GetComponentInChildren<Interaction>(false);
    }
}