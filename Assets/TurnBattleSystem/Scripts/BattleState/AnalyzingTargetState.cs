using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyzingTargetState : BattleState
{
    public List<BattleCharacter> target = new List<BattleCharacter>();
    public List<BattleCharacter> possibleTarget;
    public bool WillCutscene = false;
    int targetIndex = 0;
    public override void OnEnter()
    {
        base.OnEnter();
        battleManager.SetCursor(null);
        battleManager.actorInfoPanel.Appear(true);
        possibleTarget = battleManager.Actors;
        battleManager.isObserving = true;
        WillCutscene = false;
        if (possibleTarget.Count == 0)
        {
            battleManager.Set<ChoosingActionState>();
        }
        else
        {
            foreach(BattleCharacter bc in possibleTarget)
            {
                if (bc.Entity.isDead)
                {
                    possibleTarget.Remove(bc);
                }
            }
            Select(BattleManager.Singleton?.GetActor());
        }

    }

    public void Select(BattleCharacter character)
    {
        target.Clear();
        target.Add(character);
        battleManager.SetCursor(character);
        CamManager.PanToCharacter(character);
        battleManager.actorInfoPanel.SetActor(character);
        string t = LanguageData.GetDataById(LanguageData.INDICATION).GetValueByKey("targetOne");
        BattleManager.Singleton.SetIndicationText(character.name);
        battleManager.observationTarget = character;
        if (BattleManager.Singleton.CheckCutscene())
        {
            WillCutscene = true;
            BattleManager.Singleton.timeline.StartCinematic();
        }
        }


    public override void OnUpdate()
    {
        base.OnUpdate();
    }
    public override void OnSelect()
    {
        
        if (battleManager.GetActor().Entity.HasMaxFocus() && target[0].GetTeam() == TeamIndex.Enemy)
        {
            target[0].RevealRecipe();
            battleManager.actorInfoPanel.SetActor(target[0]);
            battleManager.GetActor().Entity.AddFocus(-09999);
        }
        base.OnSelect();
    }

    public override void OnBack()
    {
        CamManager.ResetView();
        battleManager.Set<ChoosingActionState>();
        base.OnBack();
    }

    public override void OnNavigate(Vector2 direction)
    {
        if (direction.x > 0)
        {
            NextTarget();
        }
        else if (direction.x < 0)
        {

            PreviousTarget();
        }

        base.OnNavigate(direction);
    }
    private void NextTarget()
    {

        targetIndex++;
        if (targetIndex >= possibleTarget.Count)
        {
            targetIndex = 0;
        }
        Select(possibleTarget[targetIndex]);
    }
    private void PreviousTarget()
    {
        targetIndex--;
        if (targetIndex < 0)
        {
            targetIndex = possibleTarget.Count - 1;
        }
        Select(possibleTarget[targetIndex]);
    }

    public override void OnExit()
    {
        if (!WillCutscene)
        {
            battleManager.isObserving = false;
            battleManager.actorInfoPanel.Appear(false);
            battleManager.observationTarget = null;
        }
        base.OnExit();
    }

    public int GetEnemyTargetIndex()
    {
        if (possibleTarget[targetIndex])
        {
            if (BattleManager.Singleton.EnemyPartyActors.Contains(possibleTarget[targetIndex]))
            {
                return BattleManager.Singleton.EnemyPartyActors.IndexOf(possibleTarget[targetIndex]);
            }
        }
        return -1;

    }
}
