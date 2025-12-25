using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoosingActionState : BattleState
{

    public ChoosingActionState()
    {
        MenuName = "BattleMenu";
    }

    public override void OnEnter()
    {
        battleManager.actorInfoPanel.Appear(false);
        battleManager.isObserving = false;
        CamManager.ResetView();
        battleManager.GetActor().currentCommand = null;
        battleManager.GetActor().Animator.Thinking(true);
        battleManager.SetCursor(battleManager.GetActor());
        InstantiateMenu(battleManager.GetActor());
    }


    public override void OnUpdate()
    {
        base.OnUpdate();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            battleManager.Set<CookingState>();
        }
    }

    public override void OnSelect()
    {
        choiceMenu.GetComponent<ChoiceMenu>().TriggerSelected();
        base.OnSelect();
    }

    public override void OnBack()
    {

        //battleManager.ChangeState(new ChoosingActionState());
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
        GameObject choiceMenuPrefab = Resources.Load<GameObject>($"{BattleMenuPath}{MenuName}");
        if (choiceMenuPrefab != null)
        {
            choiceMenu = GameObject.Instantiate(choiceMenuPrefab, character.transform.position + Vector3.up, Quaternion.identity);
        }
        else
        {
            Debug.LogError("ChoiceMenu prefab not found in Resources.");
        }

        OnMenuInstantiated();
    }
}
