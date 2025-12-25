using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwappingMenu : ChoiceMenu
{



    public GameObject ButtonPrefab;
    GameObject SwappingButton = null;









    public void AddButtons(List<BattleCharacter> bcs)
    {
        foreach (BattleCharacter obj in bcs)
        {

            GameObject button = Instantiate(ButtonPrefab, transform);
            if (button)
            {
                if (obj == BattleManager.Singleton.GetActor())
                {
                    SwappingButton = button.gameObject;
                    Select(SwappingButton);
                    currentButton = bcs.IndexOf(obj);
                }
                button.GetComponent<SwapButton>().SetCharacter(obj);
                button.GetComponent<ChoiceMenuButton>().OnSelected.AddListener(TriggerSkill);
                buttons.Add(button.gameObject);
            }

        }
    }

    public override void NextButton()
    {
        currentButton++;
        if (currentButton >= buttons.Count)
        {
            currentButton = 0;
        }
        SwappingButton.transform.SetSiblingIndex(currentButton);
    }


    public override void PreviousButton()
    {
        currentButton--;
        if (currentButton < 0)
        {
            currentButton = buttons.Count - 1;
        }
        SwappingButton.transform.SetSiblingIndex(currentButton);
    }

    public void TriggerSkill()
    {

        if (BattleManager.Singleton.Actors.IndexOf(BattleManager.Singleton.GetActor()) == currentButton || !BattleManager.Singleton.Actors[currentButton].CanAct())
        {
            SelectedButton.GetComponent<ChoiceMenuButton>().SelectFailed();
        }
        else
        {
            SwapCommand swapCommand = new SwapCommand();
            swapCommand.SetSource(BattleManager.Singleton.GetActor());
            swapCommand.SetTargetIndex(currentButton);
            BattleManager.Singleton.GetActor().currentCommand = swapCommand;
            BattleManager.Singleton.Set<PerformActionState>();
        }



    }


}
