using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleState
{
    protected BattleManager battleManager;
    public GameObject choiceMenu;
    public string MenuName = "menu";
    public string BattleMenuPath = $"Battle Menus/";




    public BattleState()
    {
    }
    public virtual void OnEnter(BattleManager _battleManager)
    {
        battleManager = _battleManager;
        ShowControls();
    }


    public virtual void OnExit()
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
    public virtual void Handle()
    {

    }

    public virtual void InstantiateMenu(BattleCharacter character)
    {

        GameObject choiceMenuPrefab = Resources.Load<GameObject>($"{BattleMenuPath}{MenuName}");
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
}
