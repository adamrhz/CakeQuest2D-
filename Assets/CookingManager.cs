using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public enum FailType
{
    None,
    MissingIngredient,
    TooManyWrongIngredients,
}
[System.Serializable]
public class RealTimeMeal
{
    public RealTimeMeal(Meal baseMeal)
    {
        BaseMeal = baseMeal;
        failType = FailType.None;
        AddedIngredients = new List<MealIngredients>();
        FailureRate = 0;
    }
    public Meal BaseMeal;
    public List<MealIngredients> AddedIngredients = new List<MealIngredients>();

    public bool FailedCooking => failType != FailType.None;
    public FailType failType = FailType.None;
    public int FailureRate = 0;


    public void CheckFailed()
    {
        List<MealIngredients> allIngredients = new List<MealIngredients>(AddedIngredients);
        foreach (MealIngredients ingredient in BaseMeal.Recipe)
        {
            if (allIngredients.Contains(ingredient))
            {
                allIngredients.Remove(ingredient);
            }
            else
            {
                failType = FailType.MissingIngredient;
                return;
            }
        }
        foreach (MealIngredients ingredient in allIngredients)
        {
            if (!BaseMeal.BonusIngredients.Contains(ingredient))
            {
                FailureRate += ingredient.FailureRateModifier * (BaseMeal.FailureIngredients.Contains(ingredient)?2:1);
                if (FailureRate >= BaseMeal.FailureRate)
                {
                    failType = FailType.TooManyWrongIngredients;
                    return;
                }
            }
        }
    }

    public int CalculateFinalFailureRate()
    {
        return FailureRate;
    }

    public MealStat CalculateMealStat()
    {
        MealStat mealStat = new MealStat();
        mealStat.SetBase(BaseMeal.MealStat);
        List<MealIngredients> allIngredients = new List<MealIngredients>(AddedIngredients);
        if (!FailedCooking)
        {

        }


        foreach (MealIngredients ingredient in BaseMeal.Recipe)
        {
            if (allIngredients.Contains(ingredient))
            {
                allIngredients.Remove(ingredient);
            }
        }

        foreach (MealIngredients ingredient in allIngredients)
        {
            if (BaseMeal.BonusIngredients.Contains(ingredient))
            {
                mealStat += (ingredient.MealStatModifiers);
            }
            else if (BaseMeal.FailureIngredients.Contains(ingredient))
            {
                mealStat -= ingredient.MealStatModifiers;
            }
        }

        return mealStat;
    }



}
[Serializable]
public class IngredientSpawnStats
{
    public MealIngredients Ingredient;
    public int ConsecutiveCount;
    public float TimeSinceLastSpawn;
}
public class CookingManager : MonoBehaviour
{
    public int MaxIngredientAtATime = 4;
    public float AverageSpawnTime = .6f;
    public float SpawnTimeVariation = .2f;

    public float AverageLifeTime = 1.5f;
    public float LifeTimeVariation = .45f;


    private float nextSpawnTime = 0f;

    public GameObject IngredientObjectPrefab;

    public Meal CurrentMeal;
    public RealTimeMeal RealTimeMeal;

    public Sprite Container;
    public RectTransform Zone;


    public CM_IngredientCheckList _IngredientCheckList;
    public CM_IngredientPickedList _IngredientPickedList;

    public GameObject PoofPrefab;


    public List<MealIngredients> PickedIngredient = new List<MealIngredients>();

    public List<IngredientObject> ActiveIngredients = new List<IngredientObject>();

    private float _spawnTimer = 0f;

    private Dictionary<MealIngredients, IngredientSpawnStats> _stats
    = new Dictionary<MealIngredients, IngredientSpawnStats>();

    private MealIngredients _lastSpawned;
    public void SpawnRandomIngredient()
    {
        SpawnIngredient(GetNextIngredientToSpawn());
    }

    private MealIngredients GetNextIngredientToSpawn()
    {
        const int MAX_STREAK = 3;
        const float FORCE_TIME = 12f;
        const float STARVATION_BONUS = 5f;

        var candidates = new List<(MealIngredients ingredient, float weight)>();

        foreach (var ingredient in CurrentMeal.Roster)
        {
            if (!_stats.TryGetValue(ingredient, out var stat))
            {
                stat = new IngredientSpawnStats { Ingredient = ingredient };
                _stats.Add(ingredient, stat);
            }

            if (ingredient == _lastSpawned && stat.ConsecutiveCount >= MAX_STREAK)
                continue;

            float weight = 1f;

            if (stat.TimeSinceLastSpawn >= FORCE_TIME)
            {
                weight += STARVATION_BONUS;
            }

            candidates.Add((ingredient, weight));
        }

        var forced = _stats.Values
            .Where(s => s.TimeSinceLastSpawn >= FORCE_TIME * 2)
            .OrderByDescending(s => s.TimeSinceLastSpawn)
            .FirstOrDefault();

        if (forced != null)
            return forced.Ingredient;

        return PickWeightedRandom(candidates);
    }
    MealIngredients PickWeightedRandom(List<(MealIngredients ingredient, float weight)> list)
    {
        float total = list.Sum(x => x.weight);
        float roll = UnityEngine.Random.value * total;

        foreach (var item in list)
        {
            roll -= item.weight;
            if (roll <= 0f)
                return item.ingredient;
        }

        return list[0].ingredient;
    }
    void RegisterSpawn(MealIngredients ingredient)
    {
        foreach (var stat in _stats.Values)
            stat.ConsecutiveCount = 0;

        var s = _stats[ingredient];
        s.ConsecutiveCount++;
        s.TimeSinceLastSpawn = 0f;

        _lastSpawned = ingredient;
    }

    Vector2 GetValidPosition(
    RectTransform zone,
    List<IngredientObject> activeIngredients,
    float minDistance,
    int maxAttempts = 30)
    {
        Vector2 halfSize = zone.sizeDelta / 2f;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = (Vector2)zone.position + new Vector2(
                UnityEngine.Random.Range(-halfSize.x, halfSize.x),
                UnityEngine.Random.Range(-halfSize.y, halfSize.y)
            );

            bool valid = true;

            foreach (var pos in activeIngredients)
            {
                if (Vector2.Distance(candidate, pos.transform.position) < minDistance)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
                return candidate;
        }

        // fallback (rare)
        return (Vector2)zone.position;
    }



    public void SpawnIngredient(MealIngredients ingredient)
    {
        Vector2 position = GetValidPosition(Zone, ActiveIngredients, 50f);
        Quaternion rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
        IngredientObject ingredientObject = Instantiate(IngredientObjectPrefab, position, rotation, Zone).GetComponent<IngredientObject>();
        ingredientObject?.SetIngredient(ingredient);
        ingredientObject?.SetLifeTime(UnityEngine.Random.Range(AverageLifeTime - LifeTimeVariation, AverageLifeTime + LifeTimeVariation));
        ingredientObject?.SetCookingManager(this);
        ingredientObject.ApplySquashAndStretch(1.5f, .2f);
        ActiveIngredients.Add(ingredientObject);
        RegisterSpawn(ingredient);

    }

    // Start is called before the first frame update
    void Start()
    {
        _IngredientCheckList.Init(this);
        _IngredientPickedList.Init(this);
        ResetRecipe();
        ResetSpawnTimer();

    }

    private void ResetSpawnTimer()
    {
        _spawnTimer = 0f;
        nextSpawnTime = UnityEngine.Random.Range(AverageSpawnTime - SpawnTimeVariation, AverageSpawnTime + SpawnTimeVariation);
    }



    // Update is called once per frame
    void Update()
    {
        HandleIngredientSpawnTimer();


        HandleMealDone();
    }

    private void HandleMealDone()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
        {
            MealDone();
        }
    }

    private void MealDone()
    {
        StopCooking();
        FinishRecipe();
        ResetRecipe();
        ResetSpawnTimer();
    }

    private void ResetRecipe()
    {
        RealTimeMeal = new RealTimeMeal(CurrentMeal);
        _IngredientCheckList.SetMeal(CurrentMeal);
        _IngredientPickedList.SetMeal(CurrentMeal);
        _IngredientPickedList?.ClearPickedIngredientList();
        PickedIngredient = new List<MealIngredients>();
        ActiveIngredients = new List<IngredientObject>();
    }

    private void StopCooking()
    {
        foreach (IngredientObject ingredient in ActiveIngredients)
        {
            Destroy(ingredient.gameObject);
        }
        ActiveIngredients.Clear();
    }

    private void FinishRecipe()
    {
        foreach (MealIngredients ingredient in PickedIngredient)
        {
            RealTimeMeal.AddedIngredients.Add(ingredient);
        }
        RealTimeMeal.CheckFailed();
        MealStat mealStat = RealTimeMeal.CalculateMealStat();

        Debug.Log($"Cooked {RealTimeMeal.BaseMeal.Name} | Failed: {RealTimeMeal.FailedCooking} | Failure Type: {RealTimeMeal.failType} | Final Failure Rate: {RealTimeMeal.CalculateFinalFailureRate()} | Meal Stat - Sweetness: {mealStat.Sweetness}, Bitterness: {mealStat.Bitterness}, Saltiness: {mealStat.Saltiness}, Sourness: {mealStat.Sourness}");

    }

    private void HandleIngredientSpawnTimer()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= nextSpawnTime && ActiveIngredients.Count < MaxIngredientAtATime)
        {
            SpawnRandomIngredient();
            ResetSpawnTimer();
        }
    }

    public void IngredientPicked(MealIngredients pickedIngredien)
    {
        PickedIngredient.Add(pickedIngredien);
        _IngredientCheckList?.UpdateIngredients(PickedIngredient);
        _IngredientPickedList?.AddIngredientToList(pickedIngredien);
        if(PickedIngredient.Count >= CurrentMeal.GetMaxIngredientCount())
        {
            MealDone();
        }
    }





    
}
