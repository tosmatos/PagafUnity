using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MakeExplosion : MonoBehaviour
{
    private Keyboard keyboard;
    private HashSet<GameObject> playedEffects = new HashSet<GameObject>();
    private bool played = false;
    void Start()
    {
        keyboard = Keyboard.current;
        
        // Initially hide all children
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (keyboard.pKey.wasPressedThisFrame)
        {
            if (played == false)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                    
                }
                played = true;
            }
            else
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
                played = false;
            }
           
        }
    }
}