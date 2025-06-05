using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DigitalClock : MonoBehaviour
{
    TimeManager tm;
    TextMeshProUGUI display;

    public bool _24HourClock = true;

    // Start is called before the first frame update
    void Start()
    {
        tm = TimeManager.sharedInstanceTimeManager;
        Debug.Log(tm.Clock24Hour());
        display = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (_24HourClock)
            display.text = tm.Clock24Hour();
        else
            display.text = tm.Clock12Hour();
       
    }
    public void SwitchClockMode()
    {
        _24HourClock = !_24HourClock;
    }
}