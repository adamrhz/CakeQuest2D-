using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleState : State
{
    protected BattleManager battleManager;
    public GameObject choiceMenuPrefab;
    public GameObject choiceMenu;
    public string MenuName = "menu";
    public string BattleMenuPath = $"Battle Menus/";



    public override void OnEnter()
    {
        ShowControls();
    }


    public override void OnExit()
    {

        BattleManager.Singleton.SetIndicationText("");
    }
    public virtual void OnSelectReleased()
    {
    }

    public virtual void OnBackReleased()
    {

    }
    public virtual void OnSelect()
    {
    }

    public virtual void OnBack()
    {

    }

    public virtual void ShowControls()
    {
        string controls = LanguageData.GetDataById(LanguageData.CONTROLS).GetValueByKey(this.GetType().ToString());

        battleManager.SetControlText(controls);
    }

    public virtual void OnNavigate(Vector2 direction)
    {
    }
    public override void OnUpdate()
    {

    }

    public virtual void InstantiateMenu(BattleCharacter character)
    {
        if(choiceMenuPrefab == null)
        {
            choiceMenuPrefab = Resources.Load<GameObject>($"{BattleMenuPath}{MenuName}");
        }
        if (choiceMenuPrefab != null)
        {
            choiceMenu = GameObject.Instantiate(choiceMenuPrefab, GameObject.Find("HUD Canvas").transform);

        }
        else
        {
            Debug.LogError("ChoiceMenu prefab not found in Resources.");
        }


        OnMenuInstantiated();
    }

    public virtual void OnMenuInstantiated()
    {

        Resources.UnloadUnusedAssets();
    }

    public void SetBattleManager(BattleManager battleManager)
    {
        this.battleManager = battleManager;
    }
}
