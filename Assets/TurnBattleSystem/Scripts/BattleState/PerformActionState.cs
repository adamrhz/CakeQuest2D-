using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformActionState : BattleState
{
    BattleCharacter performer;

    int initialChainTimer = 5;
    public PerformActionState()
    {
        MenuName = "ChainMenu";
    }
    public override void OnEnter()
    {
        base.OnEnter();
        battleManager.SetCursor(null);

        performer = battleManager.GetActor();
        performer.Animator.Thinking(false);
        performer.SetActing(true);
        performer.currentCommand.OnExecuted += PerformanceOver;
        //battleManager.StartCoroutine(CheckFocus());



        performer.currentCommand.OnExecuted += delegate { performer.SetActing(false); };
        performer.currentCommand.OnExecuted += performer.ResetAnimatorController;
        performer.currentCommand.OnRecipeMatched += delegate{ InstantiateMenu(performer); };
        performer.currentCommand.ExecuteCommand();
    }



    public override void OnUpdate()
    {

    }

    public override void ShowControls()
    {
        if (battleManager.GetActor())
        {
            string controls = LanguageData.GetDataById(LanguageData.CONTROLS).GetValueByKey(this.GetType().ToString());

            if (battleManager.GetActor().IsPlayerTeam())
            {
                controls = LanguageData.GetDataById(LanguageData.CONTROLS).GetValueByKey(typeof(NothingState).ToString());

            }
            battleManager.SetControlText(controls);
        }
    }

    IEnumerator CheckFocus()
    {

        if (performer.currentCommand.CanFocus())
        {



            List<BattleCharacter> takeOvers = new List<BattleCharacter>();
            if (!battleManager.IsForcedTurn())
            {

                yield return new WaitForSeconds(Random.Range(.8f, 1.5f));





                if (performer.GetTeam() == TeamIndex.Player)
                {
                    foreach (BattleCharacter bc in battleManager.HeroPartyActors)
                    {
                        if (bc.CanAct() && bc.Entity.Focus >= 10 && bc != performer)
                        {
                            takeOvers.Add(bc);
                        }
                    }
                }


                if (takeOvers.Count > 0)
                {
                    InstantiateMenu(takeOvers);
                    yield return new WaitForSeconds(Random.Range(.6f, 1.9f));
                    if (choiceMenu)
                    {
                        choiceMenu.GetComponent<TakeOverMenu>().DestroyMenu();
                    }
                }



            }
            yield return null;
        }
    }



    IEnumerator OnRecipeHit()
    {

        if (performer.currentCommand.CanCombo())
        {
            if (performer.GetTeam() == TeamIndex.Player)
            {

                InstantiateMenu(performer);
            }


            performer.StartCoroutine(Utils.SlowDown(3, .1f));

            while(Time.timeScale < 1)
            {
                if(performer != BattleManager.Singleton.GetActor())
                {
                    Object.Destroy(choiceMenu);
                    Utils.ResetTimeScale();
                }
            }




          


             
            }
            yield return null;
        
    }

    public override void OnSelect()
    {
        if (choiceMenu)
        {
            if (choiceMenu.GetComponent<TakeOverMenu>())
            {
                choiceMenu.GetComponent<ChoiceMenu>().TriggerSelected();
            }
        }
        base.OnSelect();
    }
    public override void OnNavigate(Vector2 direction)
    {
        if (choiceMenu)
        {
            choiceMenu?.GetComponent<ChoiceMenu>()?.Navigate(direction);
        }
        base.OnNavigate(direction);
    }

    public override void OnBack()
    {
        if (battleManager.IsEnemyTurn())
        {
            foreach (BattleCharacter bc in battleManager.HeroPartyActors)
            {
                if (!bc.isActing && bc.IsTargetted())
                {
                    bc.Block();
                }
            }
        }
        base.OnBack();
    }

    public override void OnBackReleased()
    {
        if (battleManager.IsEnemyTurn())
        {
            foreach (BattleCharacter bc in battleManager.HeroPartyActors)
            {

                bc.StopBlock();

            }
        }
        base.OnBackReleased();
    }
    public void InstantiateMenu(List<BattleCharacter> battleCharacters)
    {
        InstantiateTakeOverMenu(battleManager?.GetActor());

        choiceMenu.GetComponent<TakeOverMenu>().GiveTakeOvers(battleCharacters, this);
    }

    private void InstantiateTakeOverMenu(BattleCharacter battleCharacter)
    {
        GameObject choiceMenuPrefab = Resources.Load<GameObject>($"{BattleMenuPath}TakeOverMenu");
        if (choiceMenuPrefab != null)
        {
            choiceMenu = GameObject.Instantiate(choiceMenuPrefab, battleCharacter.transform.position + Vector3.up, Quaternion.identity);
            performer.StartCoroutine(Utils.SlowDown(3, .01f));

        }
        else
        {
            Debug.LogError("ChoiceMenu prefab not found in Resources.");
        }

        OnMenuInstantiated();
    }

    public override void InstantiateMenu(BattleCharacter character)
    {
        GameObject choiceMenuPrefab = Resources.Load<GameObject>($"{BattleMenuPath}{MenuName}");
        if (choiceMenuPrefab != null)
        {
            choiceMenu = GameObject.Instantiate(choiceMenuPrefab, character.transform.position + Vector3.up, Quaternion.identity);
            performer.StartCoroutine(Utils.SlowDown(initialChainTimer, .01f));
            choiceMenu.GetComponent<ChainMenu>().GiveBattleCharacter(performer);
            choiceMenu.GetComponent<ChainMenu>().SetTimer(initialChainTimer);
            if(initialChainTimer > 1)
            {
                initialChainTimer--;
            }

        }
        else
        {
            Debug.LogError("ChoiceMenu prefab not found in Resources.");
        }

        OnMenuInstantiated();
    }





    public void ForceTurn(BattleCharacter battleCharacter)
    {
        if (battleManager.GetActor().currentCommand != null)
        {
            battleManager.GetActor().currentCommand.OnExecuted -= PerformanceOver;
        }
        battleManager.SetActor(battleCharacter);
        battleCharacter.Entity.AddFocus(-100);
        battleManager.StartNewTurn();
    }
    public void PerformanceOver()
    {
        battleManager.StopCoroutine(CheckFocus());
        battleManager.NextTurn();
    }


    public override void OnExit()
    {
        base.OnExit();
        battleManager.StopCoroutine(CheckFocus());
        foreach (BattleCharacter bc in battleManager.HeroPartyActors)
        {

            bc.StopBlock();
        }
        if (battleManager.GetActor().currentCommand != null)
        {
            battleManager.GetActor().currentCommand.OnExecuted -= PerformanceOver;
        }
        battleManager.GetActor().OptionManager.ResetMenus();
        battleManager.GetActor().Animator.Thinking(false);
    }
}
