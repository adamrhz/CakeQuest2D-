using System;
using UnityEngine;

[Serializable]
public abstract class MealEffect
{
    public abstract void OnConsume(Entity consumer);
}
[Serializable]
public class HealEffect : MealEffect
{
    public int amount = 10;

    public override void OnConsume(Entity consumer)
    {
        Debug.Log("BRUH");
        consumer.AddToHealth(amount);
    }
}
[Serializable]
public class DamageEffect : MealEffect
{
    public int amount = 5;

    public override void OnConsume(Entity consumer)
    {
        consumer.AddToHealth(-amount);
    }
}
