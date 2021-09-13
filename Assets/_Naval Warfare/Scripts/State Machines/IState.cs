using System.Collections;
using System.Collections.Generic;

public interface IState
{
    void OnEnter();

    IState Tick();

    void OnExit();
}