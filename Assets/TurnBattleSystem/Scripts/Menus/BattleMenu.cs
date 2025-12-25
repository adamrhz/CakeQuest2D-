using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMenu : ChoiceMenu
{
    private void Start()
    {

        DefaultSelect();
        CheckDisabled();

    }

    private void CheckDisabled()
    {
        foreach(GameObject button in buttons)
        {
            button.GetComponent<BattleMenuButton>().disabled = BattleManager.Singleton.GetActor().OptionManager.GetDisability(buttons.IndexOf(button.gameObject));
        }
    }

    public void Guard()
    {
        Command attackCommand = new AttackCommand();
        attackCommand.SetSource(BattleManager.Singleton.GetActor());
        BattleManager.Singleton.GetActor().currentCommand = attackCommand;
        BattleManager.Singleton.Set<ChoosingTargetState>();
    }

    public void Swap()
    {
        BattleManager.Singleton.Set<SwapingState>();
    }
    public void OpenSkillMenu()
    {

        BattleManager.Singleton.Set<ChoosingSkillState>();
    }
    public void OpenItemMenu()
    {
        if (BattleManager.Singleton.GetPlayerItems().Count > 0)
        {
            BattleManager.Singleton.Set<ChoosingItemState>();
        }
    }

    public void Attack()
    {
        Command attackCommand = new AttackCommand();
        attackCommand.SetSource(BattleManager.Singleton.GetActor());
        BattleManager.Singleton.GetActor().currentCommand = attackCommand;
        BattleManager.Singleton.Set<ChoosingTargetState>();
    }


    public void Analyze()
    {
        BattleManager.Singleton.Set<AnalyzingTargetState>();
    }
}
