using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NothingBehaviour : CharacterBehaviour
{


    public static float skipTimer = 0;
    private static float skipTimerMax = 1f;

    public NothingBehaviour() : base()
    {
    }



    public override void OnEnter(Character player)
    {
        base.OnEnter(player);
        if (player.gameObject.CompareTag("Player"))
        {
            UICanvas.TurnBordersOn(false);
        }
    }



    public override void Handle()
    {
        if (Timeline.IsInCutscene)
        {

        if (character == Character.Player)
            {

                HandleSkipButton();
            }
        }
    }

    public void HandleSkipButton()
    {
        if (character.inputManager.GetButton(ButtonName.Pause))
        {
            UICanvas.SetSkipPanel(skipTimer / skipTimerMax);
            skipTimer += Time.deltaTime;
            if (skipTimer > skipTimerMax)
            {
                Timeline.SkipCurrentCutscene();
                skipTimer = 0;
            }
        }
        else
        {
            if (skipTimer > 0)
            {
                UICanvas.SetSkipPanel(skipTimer / skipTimerMax);
                skipTimer -= Time.deltaTime;
            }
            else
            {
                UICanvas.SetSkipPanel(0);
                skipTimer = 0;
            }
        }
    }

    public override void OnExit()
    {
    }







}
