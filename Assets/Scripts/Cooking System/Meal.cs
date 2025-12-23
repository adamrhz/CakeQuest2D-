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
}
[CreateAssetMenu(fileName = "New Meal", menuName = "Cooking System/Meal")]
public class Meal : ScriptableObject
{
    public string Name;
    public MealType Type;

    public int FailureRate = 50;
    public List<MealIngredients> Recipe;
    public List<Meal> BranchingMeals;
    public List<MealEffect> MealEffect;



    public List<MealIngredients> BonusIngredients;
    public List<MealIngredients> FailureIngredients;
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

}



public abstract class MealEffect
{
    public abstract void OnConsume(Entity consumer);
}