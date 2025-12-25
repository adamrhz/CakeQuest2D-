using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMenu : ChoiceMenu
{

    public GameObject ButtonPrefab;





    public void AddButtons(List<BattleItem> items)
    {
        foreach (BattleItem obj in items)
        {
            bool isin = false;
            foreach (GameObject but in buttons)
            {
                if (but.GetComponent<ItemButton>().IsItem(obj))
                {
                    but.GetComponent<ItemButton>().Add();
                    isin = true;
                    break;
                }
            }
            if (!isin)
            {
                GameObject button = Instantiate(ButtonPrefab, transform);
                button.GetComponent<ChoiceMenuButton>().OnSelected.AddListener(TriggerSkill);
                button.GetComponent<ItemButton>().SetItem(obj);
                buttons.Add(button.gameObject);
                button.GetComponent<ItemButton>().SetMenu(this);
            }

        }
        DefaultSelect();
    }



    public void TriggerSkill()
    {
        BattleItem item = SelectedButton.GetComponent<ItemButton>().storedItem;

        Command attackCommand = new ItemCommand(item);
        attackCommand.SetSource(BattleManager.Singleton.GetActor());

        if (BattleManager.Singleton.GetPossibleTarget(attackCommand).Count == 0)
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
