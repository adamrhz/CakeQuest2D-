using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillMenu : ChoiceMenu
{

    public GameObject ButtonPrefab;





    public void AddButtons(List<Skill> attacks)
    {
        foreach (Skill obj in attacks)
        {

            GameObject button = Instantiate(ButtonPrefab, transform);
            button.GetComponent<ChoiceMenuButton>().OnSelected.AddListener(TriggerSkill);
            button.GetComponent<SkillButton>().SetSkill(obj);
            buttons.Add(button.gameObject);
            button.GetComponent<SkillButton>().SetMenu(this);

        }
        DefaultSelect();
    }

    public void ResetGridLayour()
    {
    }
    public void TriggerSkill()
    {
        Skill attack = SelectedButton.GetComponent<SkillButton>().storedSkill;

        Command attackCommand = attack.GetCommandType();
        attackCommand.SetSource(BattleManager.Singleton.GetActor());

        if (attack.manaCost > BattleManager.Singleton.GetActor().Entity.Mana || BattleManager.Singleton.GetPossibleTarget(attackCommand).Count == 0)
        {
            SelectedButton.GetComponent<ChoiceMenuButton>().SelectFailed();
        }
        else
        {
            BattleManager.Singleton.GetActor().currentCommand = attackCommand;
            BattleManager.Singleton.Set<ChoosingTargetState>();
        }
    }
}
