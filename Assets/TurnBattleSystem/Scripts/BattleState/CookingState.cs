using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookingState : BattleState
{
    public CookingManager CookingManager;
    public RealTimeMeal ResultMeal;

    public override void OnEnter()
    {
        base.OnEnter();
        battleManager.actorInfoPanel.Appear(false);
        battleManager.isObserving = false;
        CamManager.ResetView();
        battleManager.GetActor().currentCommand = null;
        battleManager.GetActor().Animator.Thinking(true);
        battleManager.SetCursor(null);
        CookingManager = Object.Instantiate(battleManager.CookingMiniGamePrefab).GetComponent<CookingManager>();
        CookingManager.SetCookingState(this);
        CookingManager.StartCooking();
    }


    public override void OnUpdate()
    {
        base.OnUpdate();
    }



    public override void OnExit()
    {
        battleManager.GetActor().Animator.Thinking(false);
        base.OnExit();
    }

    public void EndMeal(RealTimeMeal realTimeMeal)
    {
        ResultMeal = realTimeMeal;
        battleManager.StartCoroutine(EndCookingMinigame());
    }

    private IEnumerator EndCookingMinigame()
    {
        Object.Destroy(CookingManager.gameObject);
        yield return new WaitForSeconds(0.5f);
        ResultMeal.CheckFailed();
        MealStat mealStat = ResultMeal.CalculateMealStat();
        Debug.Log($"Cooked {ResultMeal.BaseMeal.Name} | Failed: {ResultMeal.FailedCooking} | Failure Type: {ResultMeal.failType} | Final Failure Rate: {ResultMeal.CalculateFinalFailureRate()} | Meal Stat - Sweetness: {mealStat.Sweetness}, Bitterness: {mealStat.Bitterness}, Saltiness: {mealStat.Saltiness}, Sourness: {mealStat.Sourness}");
        if (!ResultMeal.FailedCooking)
        {

            Command attackCommand = new CookingCommand(ResultMeal);
            attackCommand.SetSource(BattleManager.Singleton.GetActor());
            BattleManager.Singleton.GetActor().currentCommand = attackCommand;
            BattleManager.Singleton.Set<ChoosingTargetState>();
            yield break;
        }

        battleManager.GetActor().Animator.Thinking(false);
        yield return new WaitForSeconds(1f);
        battleManager.NextTurn();
    }
}
