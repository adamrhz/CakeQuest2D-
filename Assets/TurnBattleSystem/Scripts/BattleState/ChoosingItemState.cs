using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoosingItemState : ChoosingSkillState
{
    public ChoosingItemState()
    {
        MenuName = "ItemMenu";
    }




    public override void InstantiateMenu(BattleCharacter character)
    {
        base.InstantiateMenu(character);
    }


    public override void OnMenuInstantiated()
    {
        if (choiceMenu)
        {
            choiceMenu.GetComponent<ItemMenu>().AddButtons(battleManager.GetPlayerItems());

        }
    }
}
