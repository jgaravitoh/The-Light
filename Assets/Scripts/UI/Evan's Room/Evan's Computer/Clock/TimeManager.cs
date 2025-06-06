using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TimeManager : MonoBehaviour
{
    public static TimeManager sharedInstanceTimeManager { get; private set; }
    public const int hoursInDay = 24, minutesInHour = 60;

    public float dayDuration = 30f;

    float totalTime = 0;
    float currentTime = 0;
    float addedHours = 0;
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (sharedInstanceTimeManager != null && sharedInstanceTimeManager != this)
        {
            Destroy(this);
        }
        else
        {
            sharedInstanceTimeManager = this;
        }
    }
    // Update is called once per frame
    void Update()
    {
        totalTime += Time.deltaTime;
        currentTime = totalTime % dayDuration;
    }

    /*
    public float GetHour()
    {
        return currentTime * hoursInDay / dayDuration;
    }
    public float GetMinutes()
    {
        return (currentTime * hoursInDay * minutesInHour / dayDuration) % minutesInHour;
    }

     */
    public float GetHour()
    {
        return (DateTime.Now.Hour + addedHours) % hoursInDay;
    }

    public float GetMinutes()
    {
        return DateTime.Now.Minute % minutesInHour;
    }

    public string Clock24Hour()
    {
        //00:00
        return Mathf.FloorToInt(GetHour()).ToString("00") + ":" + Mathf.FloorToInt(GetMinutes()).ToString("00");
    }

    public string Clock12Hour()
    {
        int hour = Mathf.FloorToInt(GetHour());
        string abbreviation = "AM";

        if (hour >= 12)
        {
            abbreviation = "PM";
            hour -= 12;
        }

        if (hour == 0) hour = 12;

        return hour.ToString("00") + ":" + Mathf.FloorToInt(GetMinutes()).ToString("00") + " " + abbreviation;
    }

    public void AddHours(int _hours)
    {
        addedHours += _hours;
    }
}