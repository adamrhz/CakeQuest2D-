using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoosingSkillState : BattleState
{
    public ChoosingSkillState()
    {
        MenuName = "Skillmenu";
    }

    public override void OnEnter()
    {
        battleManager.GetActor().currentCommand = null;
        battleManager.SetCursor(battleManager.GetActor());
        InstantiateMenu(battleManager.GetActor());
    }


    public override void OnUpdate()
    {
        base.OnUpdate();




    }
    public override void OnSelect()
    {
        choiceMenu.GetComponent<ChoiceMenu>().TriggerSelected();
        base.OnSelect();
    }

    public override void OnBack()
    {

        battleManager.Set<ChoosingActionState>();
        base.OnBack();
    }

    public override void OnNavigate(Vector2 direction)
    {
        if (direction.y < 0)
        {
            choiceMenu.GetComponent<ChoiceMenu>().NextButton();
        }
        else if (direction.y > 0)
        {

            choiceMenu.GetComponent<ChoiceMenu>().PreviousButton();
        }

        base.OnNavigate(direction);

    }

    public override void OnExit()
    {
        base.OnExit();
        GameObject.Destroy(choiceMenu);
    }



    public override void InstantiateMenu(BattleCharacter character)
    {

        base.InstantiateMenu(character);
    }

    public override void OnMenuInstantiated()
    {
        if (choiceMenu)
        {
            choiceMenu.GetComponent<SkillMenu>().AddButtons(battleManager.GetActor().GetAttacks());
        }
    }
}
