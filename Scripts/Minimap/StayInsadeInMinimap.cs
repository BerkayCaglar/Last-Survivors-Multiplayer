using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayInsadeInMinimap : MonoBehaviour
{
    private Renderer rend;
    private bool isInside;
    private void Start() {
        rend = GetComponent<Renderer>();
    }
    private void Update() {
        isInside = rend.isVisible ? true : false;

        if(!isInside)
        {
            
        }
    }
    public bool GetIsInside()
    {
        return isInside;
    }
}
