using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NothingState : BattleState
{
    public override void OnEnter()
    {
        base.OnEnter();
        battleManager.SetCursor(null);
    }


    public override void OnUpdate()
    {
        base.OnUpdate();
    }



    public override void OnExit()
    {
        base.OnExit();
    }
}
