using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageTrigger : MonoBehaviour
{
    private const string NoTag = "Untagged";
    private const string tagname = "Enemy";

    [Tooltip("Tag of the interacting Collider to filter on.")]
    [SerializeField] private string reactOn = tagname;
    
    [SerializeField] private int damage = 1;
    
    /// <summary>
    /// Called when a value in the inspector is changed.
    /// </summary>
    private void OnValidate()
    {
        // Replaces an 'empty' reactOn field with "Untagged".
        if (string.IsNullOrWhiteSpace(reactOn))
        {
            reactOn = NoTag;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagname))
        {
            other.GetComponent<EnemyBehaviour>().GetDamage(damage);
        }
    }
}
