using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Cooking System/Ingredient")]
public class MealIngredients : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public MealStat MealStatModifiers;
    public int FailureRateModifier = 4;

}
