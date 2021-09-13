using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

public class EventManager : MonoBehaviour
{
    public enum Events
    {
        OnGameManagerReady,
        OnButtonDisplayPressed,
        OnButtonClearScenarioInputPressed,
        OnInputHasText,
        OnInputIsEmpty,
        OnCardMovedToInputSlot,
        OnCardReturned,
        OnLevelStarted,
        OnEnemyDetected,
        OnEnemyDied,
    }

    private Dictionary<EventManager.Events, UnityEvent> simpleEventDictionary = new Dictionary<EventManager.Events, UnityEvent>();
    private Dictionary<EventManager.Events, UnityEvent<int>> paramIntEventDictionary = new Dictionary<EventManager.Events, UnityEvent<int>>();
    private Dictionary<EventManager.Events, UnityEvent<float>> paramFloatEventDictionary = new Dictionary<EventManager.Events, UnityEvent<float>>();
    private Dictionary<EventManager.Events, UnityEvent<string>> paramStringEventDictionary = new Dictionary<EventManager.Events, UnityEvent<string>>();
    private Dictionary<EventManager.Events, UnityEvent<GameObject>> paramGOEventDictionary = new Dictionary<EventManager.Events, UnityEvent<GameObject>>();
    private Dictionary<EventManager.Events, UnityEvent<Vector3>> paramVec3EventDictionary = new Dictionary<EventManager.Events, UnityEvent<Vector3>>();

    public static EventManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }
    }

    //========================
    public void StartListening(EventManager.Events eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;

        if (simpleEventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            simpleEventDictionary.Add(eventName, thisEvent);
        }
    }

    public void StartListeningWithGOParam(EventManager.Events eventName, UnityAction<GameObject> listener)
    {
        UnityEvent<GameObject> thisParamEvent = null;
        if (paramGOEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.AddListener(listener);
        }
        else
        {
            thisParamEvent = new UnityEvent<GameObject>();
            thisParamEvent.AddListener(listener);
            paramGOEventDictionary.Add(eventName, thisParamEvent);
        }
    }

    public void StartListeningWithIntParam(EventManager.Events eventName, UnityAction<int> listener)
    {
        UnityEvent<int> thisParamEvent = null;
        if (paramIntEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.AddListener(listener);
        }
        else
        {
            thisParamEvent = new UnityEvent<int>();
            thisParamEvent.AddListener(listener);
            paramIntEventDictionary.Add(eventName, thisParamEvent);
        }
    }
    public void StartListeningWithFloatParam(EventManager.Events eventName, UnityAction<float> listener)
    {
        UnityEvent<float> thisParamEvent = null;
        if (paramFloatEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.AddListener(listener);
        }
        else
        {
            thisParamEvent = new UnityEvent<float>();
            thisParamEvent.AddListener(listener);
            paramFloatEventDictionary.Add(eventName, thisParamEvent);
        }
    }

    public void StartListeningWithStringParam(EventManager.Events eventName, UnityAction<string> listener)
    {
        UnityEvent<string> thisParamEvent = null;
        if (paramStringEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.AddListener(listener);
        }
        else
        {
            thisParamEvent = new UnityEvent<string>();
            thisParamEvent.AddListener(listener);
            paramStringEventDictionary.Add(eventName, thisParamEvent);
        }
    }

    public void StartListeningWithVec3Param(EventManager.Events eventName, UnityAction<Vector3> listener)
    {
        UnityEvent<Vector3> thisParamEvent = null;
        if (paramVec3EventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.AddListener(listener);
        }
        else
        {
            thisParamEvent = new UnityEvent<Vector3>();
            thisParamEvent.AddListener(listener);
            paramVec3EventDictionary.Add(eventName, thisParamEvent);
        }
    }

    internal void TriggerEvent(object onHammerHitGround)
    {
        throw new NotImplementedException();
    }

    //========================
    public void StopListening(EventManager.Events eventName, UnityAction listener)
    {
        if (Instance == null) return;
        UnityEvent thisEvent = null;
        if (simpleEventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public void StopListeningWithGOParam(EventManager.Events eventName, UnityAction<GameObject> listener)
    {
        if (Instance == null) return;
        UnityEvent<GameObject> thisParamEvent = null;
        if (paramGOEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.RemoveListener(listener);
        }
    }
    public void StopListeningWithIntParam(EventManager.Events eventName, UnityAction<int> listener)
    {
        if (Instance == null) return;
        UnityEvent<int> thisParamEvent = null;
        if (paramIntEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.RemoveListener(listener);
        }
    }
    public void StopListeningWithFloatParam(EventManager.Events eventName, UnityAction<float> listener)
    {
        if (Instance == null) return;
        UnityEvent<float> thisParamEvent = null;
        if (paramFloatEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.RemoveListener(listener);
        }
    }
    public void StopListeningWithStringParam(EventManager.Events eventName, UnityAction<string> listener)
    {
        if (Instance == null) return;
        UnityEvent<string> thisParamEvent = null;
        if (paramStringEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.RemoveListener(listener);
        }
    }

    public void StopListeningWithVec3Param(EventManager.Events eventName, UnityAction<Vector3> listener)
    {
        if (Instance == null) return;
        UnityEvent<Vector3> thisParamEvent = null;
        if (paramVec3EventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.RemoveListener(listener);
        }
    }


    //========================
    public void TriggerEvent(EventManager.Events eventName)
    {
        UnityEvent thisEvent = null;
        if (simpleEventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }

    public void TriggerEventWithGOParam(EventManager.Events eventName, GameObject go)
    {
        UnityEvent<GameObject> thisParamEvent = null;
        if (paramGOEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.Invoke(go);
        }
    }

    public void TriggerEventWithIntParam(EventManager.Events eventName, int i)
    {
        UnityEvent<int> thisParamEvent = null;
        if (paramIntEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.Invoke(i);
        }
    }
    public void TriggerEventWithFloatParam(EventManager.Events eventName, float value)
    {
        UnityEvent<float> thisParamEvent = null;
        if (paramFloatEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.Invoke(value);
        }
    }
    public void TriggerEventWithStringParam(EventManager.Events eventName, string s)
    {
        UnityEvent<string> thisParamEvent = null;
        if (paramStringEventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.Invoke(s);
        }
    }

    public void TriggerEventWithVec3Param(EventManager.Events eventName, Vector3 vec3)
    {
        UnityEvent<Vector3> thisParamEvent = null;
        if (paramVec3EventDictionary.TryGetValue(eventName, out thisParamEvent))
        {
            thisParamEvent.Invoke(vec3);
        }
    }
}