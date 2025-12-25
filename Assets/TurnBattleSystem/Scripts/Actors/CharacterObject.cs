
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
[System.Serializable]
public class CharacterObject : SavableObject
{


    public CharacterData characterData;
    [Space(20)]
    public int Health;
    public int MaxHealth;
    [Space(20)]
    public int Mana;
    public int MaxMana;
    [Space(20)]
    public int Speed;
    public int AttackDamage;
    public float parryWindow = .15f;
    [Space(20)]
    public List<Element> IngredientWheel = new List<Element>();

    [Space(20)]
    public bool isDead;

    [Space(20)]
    public SkillType attackType = SkillType.Physical;
    [Space(20)]
    public Element AttackElement;
    [Space(20)]

    public List<GameObject> HitEffect;
    public List<AudioClip> SoundEffect;

    [Space(40)]
    public List<Skill> Attacks;

    [Space(20)]
    public AnimatorOverrideController animationController;

    public BoolValue InParty;



    [Space(20)]
    public int recipeLength = 3;


    public MealStat PreferredTastes;
    public List<Meal> FavoriteMeals = new List<Meal>();
    public List<Meal> DespisedMeals = new List<Meal>();



    public override string GetJsonData()
    {
        var jsonObject = JObject.Parse(base.GetJsonData()); // Start with base class data

        // Include all non-ignored properties
        jsonObject["Health"] = Health;
        jsonObject["MaxHealth"] = MaxHealth;
        jsonObject["Mana"] = Mana;
        jsonObject["MaxMana"] = MaxMana;
        jsonObject["isDead"] = isDead;

        // Handle Attacks list
        jsonObject["characterAttacks"] = JArray.FromObject(GetStringifiedAttackList());

        return jsonObject.ToString();



    }

    private List<string> GetStringifiedAttackList()
    {
        // Assuming each Skill has a method/property that returns its unique ID or name
        List<string> attackIds = new List<string>();
        foreach (Skill skill in Attacks)
        {
            if(skill != null)
            {
                attackIds.Add(skill.UID); // Replace with appropriate identifier for each Skill
            }
        }
        return attackIds;
    }


    public override void ApplyJsonData(string jsonData)
    {
        base.ApplyJsonData(jsonData); // Apply base class data first

        try
        {
            // Parse the JSON data to a JObject
            JObject jsonObject = JObject.Parse(jsonData);

            // Extract the character attack skills from the JSON
            if (jsonObject.ContainsKey("characterAttacks"))
            {
                JArray partyArray = (JArray)jsonObject["characterAttacks"];
                if (partyArray != null)
                {
                    // Convert the array to a list of strings
                    List<string> loadedAttacks = partyArray.ToObject<List<string>>();

                    // Add skills to the moveset based on the loaded data
                    AddSkillsToMoveset(loadedAttacks);
                }
                else
                {
                    Debug.LogWarning("'characterAttacks' array is null.");
                }
            }
            else
            {
                Debug.LogWarning("No 'characterAttacks' key found in the JSON data.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error applying character attacks JSON data: {ex.Message}");
        }
    }



    public void AddSkillsToMoveset(List<string> loadedSkills)
    {

        Attacks = new List<Skill>();

        foreach (string item in loadedSkills)
        {
            if (ObjectLibrary.Library.TryGetValue(item, out SavableObject value))
            {
                Attacks.Add(value as Skill);
            }
        }

    }

    public IEnumerator AddLoadedSkillToMoveset(List<Skill> loadedSkills)
    {
        Attacks.Clear();
        foreach (Skill skill in loadedSkills)
        {

            ResourceRequest request = Resources.LoadAsync<Skill>($"SkillFolder/{skill.name}");
            while (!request.isDone)
            {
                yield return null;
            }
            Skill loadedSkill = request.asset as Skill;
            Attacks.Add(loadedSkill);
            yield return null;
        }
        yield return null;
    }

    public void Revitalize()
    {
        isDead = false;
        Health = MaxHealth;
        Mana = MaxMana;

    }
    public GameObject GetHitEffect()
    {
        if (HitEffect.Count > 0)
        {
            return HitEffect[UnityEngine.Random.Range(0, HitEffect.Count)];
        }
        return null;

    }

    public AudioClip GetSoundEffect()
    {
        if (SoundEffect.Count > 0)
        {
            return SoundEffect[UnityEngine.Random.Range(0, SoundEffect.Count)];
        }
        return null;

    }

    public ElementEffect GetElementEffect(Element element)
    {
        return ElementEffect.Neutral;
    }

    public float GetParryWindowTime()
    {
        return parryWindow;
    }
}
