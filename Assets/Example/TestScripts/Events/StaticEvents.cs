using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

public class StaticEvents : MonoBehaviour
{

    public delegate void EventHandlerForThing();

    public static event EventHandlerForThing OnDoAThing;

    public static event EventHandlerForThing OnDoNothingTesterDontListen;

    public delegate void EventHandlerForThingPlusX(int x);

    public static event EventHandlerForThingPlusX OnDoAThingPlusX;

    public static void DoAThing()
    {
        // notify all listeners to event.
        if (OnDoAThing != null)
        {
            OnDoAThing();
        }
    }

    public static void DoNothingListening(int x)
    {
        // notify all listeners to event.
        if (OnDoNothingTesterDontListen != null)
        {
            OnDoNothingTesterDontListen();
        }
    }

    public static void DoAThingPlusX(int x)
    {
        // notify all listeners to event.
        if (OnDoAThingPlusX != null)
        {
            OnDoAThingPlusX(x);
        }
    }

    public void RemoveAllEventsHack()
    {
        // NOT TO BE USED, just for debuging, these should be removed properly from the object that assigns it.
        if (OnDoAThing != null)
        {
            foreach (Delegate onDoAThingDelegate in OnDoAThing.GetInvocationList())
            {
                OnDoAThing -= (EventHandlerForThing)onDoAThingDelegate;
            }
        }
        if (OnDoAThingPlusX != null)
        {
            foreach (Delegate onDoAThingDelegate in OnDoAThingPlusX.GetInvocationList())
            {
                OnDoAThingPlusX -= (EventHandlerForThingPlusX)onDoAThingDelegate;
            }
        }
    }
}

