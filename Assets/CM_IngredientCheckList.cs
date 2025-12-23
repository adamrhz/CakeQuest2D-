using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CM_IngredientCheckList : MonoBehaviour
{
    public CookingManager CookingManager;
    public Meal BaseMeal;
    public RectTransform RecipeListParent;

    public Image FillBar;


    public Coroutine FillBarFillUpRoutine;

    public void Init(CookingManager cookingManager)
    {
        CookingManager = cookingManager;
    }
    public void SetMeal(Meal meal)
    {
        BaseMeal = meal;
        ClearRecipeList();
    }

    public void UpdateIngredients(List<MealIngredients> ingredients)
    {
        List<MealIngredients> currentIngredients = new List<MealIngredients>(ingredients);
        int totalIngredients = BaseMeal.Recipe.Count;
        int filledIngredients = 0;
        foreach (Transform child in RecipeListParent)
        {
            IngredientObject obj = child.gameObject.GetComponent<IngredientObject>();
            if (currentIngredients.Contains(obj.IngredientData))
            {
                filledIngredients++;
                obj.Image.color = new(1, 1, 1, 1f);
                currentIngredients.Remove(currentIngredients.Find(x => x == obj.IngredientData));
            }
            else
            {
                obj.Image.color = new(1, 1, 1, .2f);
            }
        }
        UpdateFillBar((float)filledIngredients / totalIngredients);
    }

    private void UpdateFillBar(float ratio)
    {
        if(FillBarFillUpRoutine != null)
        {
            StopCoroutine(FillBarFillUpRoutine);
            FillBarFillUpRoutine = null;
        }
        FillBarFillUpRoutine = StartCoroutine(FillBarRoutine(ratio));
    }

    public IEnumerator FillBarRoutine(float ratio)
    {

        float speed = 2f;
        float minSpeed = 1f;
        float lerpAcceleration = 5;

        while(FillBar.fillAmount != ratio)
        {
            FillBar.fillAmount = Mathf.MoveTowards(FillBar.fillAmount, ratio, speed * Time.deltaTime);
            speed = Mathf.Clamp(speed - Time.deltaTime * lerpAcceleration, minSpeed, speed);
            yield return null;
        }



        yield return null;
    }

    public void ClearRecipeList()
    {
        foreach (Transform child in RecipeListParent)
        {
            Destroy(child.gameObject);
        }
        foreach (MealIngredients ingredient in BaseMeal.Recipe)
        {
            IngredientObject obj = Instantiate(CookingManager.IngredientObjectPrefab, Vector3.zero, Quaternion.identity, RecipeListParent).GetComponent<IngredientObject>();
            obj.SetIngredient(ingredient);
            obj.Image.color = new(1, 1, 1, .2f);
            obj.Active = false;
            //obj.ApplySquashAndStretch(1.5f, .2f);
            Destroy(obj.GetComponent<EventTrigger>());
        }
        UpdateFillBar(0);
    }
}
