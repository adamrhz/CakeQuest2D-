using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MealType
{
    Appetizer,
    MainCourse,
    Dessert,
    Beverage
}
[Serializable]
public struct MealStat
{
    public int Sweetness;
    public int Bitterness;
    public int Saltiness;
    public int Sourness;

    public void AddModifiers(MealStat mealStatModifiers)
    {
        Sweetness += mealStatModifiers.Sweetness;
        Bitterness += mealStatModifiers.Bitterness;
        Saltiness += mealStatModifiers.Saltiness;
        Sourness += mealStatModifiers.Sourness;
    }

    public static MealStat operator +(MealStat x, MealStat y)
    {
        return new MealStat
        {
            Sweetness = x.Sweetness + y.Sweetness,
            Bitterness = x.Bitterness + y.Bitterness,
            Saltiness = x.Saltiness + y.Saltiness,
            Sourness = x.Sourness + y.Sourness
        };
    }
    public static MealStat operator -(MealStat x, MealStat y)
    {
        return new MealStat
        {
            Sweetness = x.Sweetness - y.Sweetness,
            Bitterness = x.Bitterness - y.Bitterness,
            Saltiness = x.Saltiness - y.Saltiness,
            Sourness = x.Sourness - y.Sourness
        };
    }
    public static MealStat operator -(MealStat x)
    {
        return new MealStat
        {
            Sweetness = -x.Sweetness,
            Bitterness = -x.Bitterness,
            Saltiness = -x.Saltiness,
            Sourness = -x.Sourness
        };
    }
    public void SetBase(MealStat mealStat)
    {
        this += mealStat;
    }

    public void GenerateRandomValues()
    {
        int[] values = new int[4];

        int total = 100;
        for (int i = 0; i < values.Length; i++)
        {
            int v = UnityEngine.Random.Range(0, total + 1);
            values[i] = v;
            total -= v;
        }

        for (int i = 0; i < values.Length; i++)
        {
            int j = UnityEngine.Random.Range(i, values.Length);
            (values[i], values[j]) = (values[j], values[i]);
        }

        Sweetness = values[0];
        Bitterness = values[1];
        Saltiness = values[2];
        Sourness = values[3];
    }

    public float GetSatisfaction(RealTimeMeal meal)
    {
        MealStat cookedMealStats = meal.CalculateMealStat();
        Vector4 Meal4 = new Vector4(Sweetness, Bitterness, Saltiness, Sourness);
        Vector4 CookedMeal4 = new Vector4(cookedMealStats.Sweetness, cookedMealStats.Bitterness, cookedMealStats.Saltiness, cookedMealStats.Sourness);
        return GetFlavorMatch(Meal4, CookedMeal4, 100);

    }

    public float GetFlavorMatch(Vector4 target, Vector4 cooked, float maxValuePerAxis)
    {
        float maxDistance = Mathf.Sqrt(4 * maxValuePerAxis * maxValuePerAxis);
        float dist = Vector4.Distance(target, cooked);
        float alignmentValue = Mathf.Clamp01(1f - (dist / maxDistance));
        if(alignmentValue > .9f){
            Debug.Log("Craving fullfillment! Critical MEAL SATISFACTION!");
            return alignmentValue * UnityEngine.Random.Range(1.75f, 3);
        }
        return alignmentValue;
    }

}
[CreateAssetMenu(fileName = "New Meal", menuName = "Cooking System/Meal")]
public class Meal : ScriptableObject
{
    [Header("Recipe Attributes")]
    public string Name;
    public MealType Type;



    [Header("Recipe Components")]
    public List<MealIngredients> Recipe;
    public List<MealIngredients> BonusIngredients;
    public List<MealIngredients> FailureIngredients;
    public List<Meal> BranchingMeals;
    [SerializeReference]
    public List<MealEffect> MealEffect;

    [Header("Recipe Stats")]
    [Range(10, 50)]
    public int FailureRate = 50;
    [Range(10, 50)]
    public int MealWeight = 20;
    [Range(-1,1)]
    public int HeatValue = 20; //
    public MealStat MealStat;

    public List<MealIngredients> Roster => new List<MealIngredients>(Recipe).Concat(BonusIngredients).Concat(FailureIngredients).ToList();
    public void Consume(Entity consumer)
    {
        foreach (MealEffect effect in MealEffect)
        {
            effect.OnConsume(consumer);
        }
    }

    public int GetMaxIngredientCount()
    {
        return 2 * Recipe.Count;
    }


    [ContextMenu("Generate Random Values")]
    public void GenerateRandomValues()
    {
        MealStat.GenerateRandomValues();
    }
}
