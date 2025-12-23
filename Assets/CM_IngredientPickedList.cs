using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CM_IngredientPickedList : MonoBehaviour
{

    public CookingManager CookingManager;
    public Meal BaseMeal;
    public List<MealIngredients> AddedIngredients = new List<MealIngredients>();
    public RectTransform AllIngredientInParent;

    public TMP_Text AmountText;
    public void Init(CookingManager cookingManager)
    {
        CookingManager = cookingManager;
    }
    public void SetMeal(Meal meal)
    {
        BaseMeal = meal; 
        ClearPickedIngredientList();
    }

    public void AddIngredientToList(MealIngredients pickedIngredien)
    {
        IngredientObject obj = Instantiate(CookingManager.IngredientObjectPrefab, Vector3.zero, Quaternion.identity, AllIngredientInParent).GetComponent<IngredientObject>();
        obj.SetIngredient(pickedIngredien);
        obj.Image.color = new(1, 1, 1, 1);
        obj.Active = false;
        obj.ApplySquashAndStretch(1.5f, .2f);
        AddedIngredients.Add(pickedIngredien);
        UpdateText();
        Destroy(obj.GetComponent<EventTrigger>());
    }

    public void ClearPickedIngredientList()
    {
        foreach (Transform child in AllIngredientInParent)
        {
            Destroy(child.gameObject);
        }
        AddedIngredients.Clear();
        UpdateText();
    }

    private void UpdateText()
    {
        AmountText.SetText($"{AddedIngredients.Count}/{BaseMeal.GetMaxIngredientCount()}");
        AmountText.ApplySquashAndStretch(1.5f, .2f);
    }
}
