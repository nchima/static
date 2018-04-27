using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventManager {

    // Singleton
    private static GameEventManager _instance;
    public static GameEventManager instance {
        get {
            if (_instance == null) { _instance = new GameEventManager(); }
            return _instance;
        }
    }

    // A dictionary which maps types (in practice, event types) to Event.Handlers.
    private Dictionary<Type, GameEvent.Handler> registeredHandlers = new Dictionary<Type, GameEvent.Handler>();


    // 'Subscribes' a given function to be triggered when an event is fired.
    public void Subscribe<EventType>(GameEvent.Handler handlerToAdd) where EventType : GameEvent {
        Type eventType = typeof(EventType);

        // If registeredHandlers already contains the given event type as a key, add it to the associated delegate.
        if (registeredHandlers.ContainsKey(eventType)) {
            registeredHandlers[eventType] += handlerToAdd;
        }

        // If it does not, then add it.
        else {
            registeredHandlers[eventType] = handlerToAdd;
        }
    }


    public void Unsubscribe<EventType>(GameEvent.Handler handlerToRemove) where EventType : GameEvent {
        Type eventType = typeof(EventType);

        GameEvent.Handler handlers;

        // Confirm that the dictionary contains this value.
        if (registeredHandlers.TryGetValue(eventType, out handlers)) {

            // Remove the given handler from the appropriate delegate.
            handlers -= handlerToRemove;

            // If the delegate as now empty, remove this entry from the dictionary.
            if (handlers == null) {
                registeredHandlers.Remove(eventType);
            }

            // Otherwise, set the dictionary value for this event type to the modified delegate.
            else {
                registeredHandlers[eventType] = handlers;
            }
        }
    }


    public void FireEvent(GameEvent eventToFire) {
        Type type = eventToFire.GetType();
        GameEvent.Handler handlers;
        if (registeredHandlers.TryGetValue(type, out handlers)) {
            handlers(eventToFire);
        }
    }
}
