using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine {
    private IState currentState;

    public void Tick () {
        IState nextState = currentState?.Tick ();
        SetState (nextState);
    }

    public void SetState (IState newState) {

        if (newState == currentState || newState == null) {
            return;
        }

        currentState?.OnExit ();
        currentState = newState;
        currentState.OnEnter ();
    }
}