using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInformation
{

    public IActionData attack;
    public float amount = 0;
    public ElementEffect effect;
    public Element element;
    public BattleCharacter source;
    public bool TriggersChain = true;
    public bool isFinalHit;
    public string ID;
    public bool textEffectShown = false;
    public int RecipeIndex = 0;
    public Command command;


    public AttackInformation(IActionData actionData, ElementEffect elementEffect, BattleCharacter source, Command currentCommand)
    {

        if (string.IsNullOrEmpty(this.ID))
        {
            this.ID = currentCommand.commandID;
        }
        this.attack = actionData;
        if (attack)
        {

            amount = element == Element.Support ? attack.GetAmount() : -attack.GetAmount();
            element = attack.element;
        }
        else
        {
            amount = -source.GetReference().AttackDamage;
            element = Element.None;
        }
        this.effect = elementEffect;
        this.source = source;
        this.command = currentCommand;
    }



    public void HandleRecipe(BattleCharacter target)
    {
        return;
        if (element != Element.None && element != Element.Support)
        {
            if (target.HandleRecipe(this))
            {
                if (effect == ElementEffect.RecipeCompleted)
                {
                    amount *= RecipeIndex;
                }
                else
                {
                    amount += (amount * .5f * RecipeIndex);
                }
            }
        }
    }

    public int GetAmount()
    {
        return (int)amount;
    }

    public void MatchedRecipe(int recipeIndex)
    {
        RecipeIndex = recipeIndex;
        effect = ElementEffect.RecipeBoosted;

    }

    public void HandleDamage()
    {
        if (effect == ElementEffect.RecipeCompleted)
        {
            amount *= RecipeIndex;
        }
        else
        {
            amount += (amount * .5f * RecipeIndex);
        }
    }
}
