using System.Collections;
using UnityEngine;

public class CookingCommand : Command
{
    private RealTimeMeal resultMeal;

    public CookingCommand(RealTimeMeal resultMeal)
    {
        this.resultMeal = resultMeal;
    }


    public override void ExecuteCommand()
    {
        Source.StartCoroutine(Execute());
    }

    public override void ActivateCommand()
    {
        base.ActivateCommand();
    }

    public IEnumerator Execute()
    {

        startPosition = Source.transform.position;

        yield return new WaitForSeconds(.6f);
        Source.EnemyContainer.localPosition = new Vector3(0.625f, -0.25f, 0);
        foreach (BattleCharacter target in Target)
        {
            target.CookingEnjoyment.CompareTastes(resultMeal);
        }


        yield return new WaitForSeconds(.6f);
        CamManager.ResetView();
        BattleManager.Singleton.FadeBackground(false, .3f);
        foreach (BattleCharacter bc in Target)
        {
            yield return bc.StartCoroutine(GoToOriginalPosition(bc));
        }
        OnCommandOver();
        OnExecuted?.Invoke();



    }

}
