using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class CookingEnjoyment : MonoBehaviour
{

    public List<Meal> FavoriteMeals = new List<Meal>();
    public List<Meal> DespisedMeals = new List<Meal>();
    public MealStat PreferredTastes;
    [Range(0, 100)]
    public int Satisfaction = 0;


    public Queue<Meal> AteMeal = new Queue<Meal>();


    public void InitializeTastes(CharacterObject character)
    {
        PreferredTastes = character.PreferredTastes;
        FavoriteMeals = character.FavoriteMeals;
        DespisedMeals = character.DespisedMeals;
        PreferredTastes.GenerateRandomValues();
    }


    public void RegisterMeal(Meal meal)
    {
        AteMeal.Enqueue(meal);
        if(AteMeal.Count >= 5)
        {
            AteMeal.Dequeue();
        }
    }
    private int CalculateStaleFactor(Meal baseMeal)
    {
        int count = 0;
        foreach(Meal meal in AteMeal)
        {
            if(meal == baseMeal)
            {
                count++;
            }
        }
        return count + 1;
    }

    public void AddToSatisfaction(int amount)
    {
        Satisfaction = Mathf.Clamp(amount + Satisfaction, -100, 100);
    }
    public void CompareTastes(RealTimeMeal meal)
    {

        float StaleFactor = CalculateStaleFactor(meal.BaseMeal);

        float fullFillFactor = 1;
        float SatisfactoryFactor = 1;
        if (FavoriteMeals.Contains(meal.BaseMeal))
        {
            Debug.Log("Meal is a favorite, applying positive satisfaction.");
            SatisfactoryFactor = UnityEngine.Random.Range(1.2f, 2f);
        }
        else if (DespisedMeals.Contains(meal.BaseMeal))
        {
            Debug.Log("Meal is dispised, applying negative satisfaction.");
            SatisfactoryFactor = UnityEngine.Random.Range(-.3f, -.9f);
        }

        float SatisfactionScore = PreferredTastes.GetSatisfaction(meal);
        float TotalSatisfaction = (meal.BaseMeal.MealWeight * SatisfactionScore * SatisfactoryFactor * fullFillFactor)/StaleFactor;

        AddToSatisfaction(Mathf.CeilToInt(TotalSatisfaction));

    }

}
