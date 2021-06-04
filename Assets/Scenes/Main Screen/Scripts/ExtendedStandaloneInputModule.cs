using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

//Get Full Access to PointerEventData Using A Class To Retrieve The Data
public class ExtendedStandaloneInputModule : StandaloneInputModule
{
    public static PointerEventData GetPointerEventData(int pointerId = -1)
    {
        PointerEventData eventData;
        _instance.GetPointerData(pointerId, out eventData, true);
        return eventData;
    }
 
    private static ExtendedStandaloneInputModule _instance;
 
    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }
}