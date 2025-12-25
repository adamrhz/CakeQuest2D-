using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Unity.VisualScripting;

[Serializable, Inspectable]
public class Dialogue
{



    [Inspectable] public ConditionResultObject[] condition;
    public DialogueEvent[] DialogueEvents { get; set; }

    [Inspectable] public string[] dialogueLineIds;
    [Inspectable] public ChoiceDialogue[] choices;
    [Inspectable] public DSDialogueChoiceData[] DialogueChoices;

    [Inspectable] public DSDialogueType DialogueType;
    [Inspectable] public string EventIndex;
    [Inspectable] public UnityEvent OnOverEvent;
    [Inspectable] public UnityEvent OnInstantOverEvent;

    public object source = null;

    public void SetSource(object source)
    {
        this.source = source;
    }


    public Dialogue(Dialogue dialogue)
    {

        if (dialogue != null)
        {
            if (dialogue.dialogueLineIds != null)
            {
                this.dialogueLineIds = dialogue.dialogueLineIds.Length > 0 ? dialogue.dialogueLineIds : null;
            }
            if (dialogue.choices != null)
            {
                this.choices = dialogue.choices.Length > 0 ? dialogue.choices : null;
            }
            this.condition = dialogue.condition;
            this.OnOverEvent = dialogue.OnOverEvent;
            this.DialogueType = dialogue.DialogueType;
            this.OnInstantOverEvent = dialogue.OnInstantOverEvent;
            this.source = dialogue.source;
        }
    }
    public Dialogue(DSDialogueSO dialogue, DialogueEvent[] events = null, UnityAction dialogueOverCallback = null)
    {

        if (dialogue != null)
        {
            this.dialogueLineIds = dialogue.Text.ToArray();
            if (this.dialogueLineIds.Length == 0)
            {
                this.dialogueLineIds = null;
            }
            this.DialogueChoices = dialogue.Choices.ToArray();

            if (this.DialogueChoices.Length == 0)
            {
                this.DialogueChoices = null;
            }
            this.condition = dialogue.Conditions.ToArray();
            this.EventIndex = dialogue.EventIndex;
            this.DialogueType = dialogue.DialogueType;
            this.DialogueEvents = events;
            this.OnOverEvent = new UnityEvent();
            if (dialogueOverCallback != null)
            {
                this.OnOverEvent.AddListener(dialogueOverCallback);
            }
        }
    }


    public void SetNextPlayed()
    {
        if (DialogueChoices != null)
        {
            foreach (DSDialogueChoiceData choiceData in DialogueChoices)
            {
                if (choiceData.NextDialogue)
                {
                    if (choiceData.NextDialogue.BattleConditionParams != null)
                    {
                        if (choiceData.NextDialogue.BattleConditionParams[0].requiresPrevious == true)
                        {
                            choiceData.NextDialogue.BattleConditionParams[0].requiresPrevious = false;
                        }
                    }
                }
            }
        }
    }



    public bool ConditionRespected()
    {
        foreach (ConditionResultObject c in condition)
        {
            if (!c.CheckCondition())
            {
                return false;
            }
        }
        return true;
    }
    public bool isNull()
    {
        if (dialogueLineIds == null)
        {
            return true;
        }
        else if (dialogueLineIds.Length == 0 && choices.Length == 0)
        {
            return true;
        }
        else if (dialogueLineIds.Length == 0 && HasOnePossibleChoice())
        {
            return false;
        }
        else
        {
            foreach (string l in dialogueLineIds)
            {
                if (string.IsNullOrEmpty(l))
                {
                    return true;
                }
            }
        }
        return false;
    }


    public DSDialogueChoiceData[] GetUsableChoicesList()
    {
        if (DialogueType == DSDialogueType.SingleChoice || DialogueChoices == null)
        {
            return null;
        }
        List<DSDialogueChoiceData> returnChocies = new List<DSDialogueChoiceData>();
        foreach (DSDialogueChoiceData c in DialogueChoices)
        {
            if (c.NextDialogue == null)
            {
                return null;
            }
            if (c.NextDialogue.ConditionRespected())
            {
                returnChocies.Add(c);
            }
        }
        if (returnChocies.Count == 0)
        {
            return null;
        }
        return returnChocies.ToArray();
    }

    public bool HasOnePossibleChoice()
    {

        return GetUsableChoicesList()?.Length == 1;
    }


}
