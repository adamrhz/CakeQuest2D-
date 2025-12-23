using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public BattleCharacter lastAttacker;
    public AttackInformation lastAttack;
    BattleCharacter character;
    public int Speed = 50;
    public int Health = 50;
    public int Mana = 50;
    public int Satisfaction = 0;
    public int Focus = 0;
    public int MaxFocus = 10;
    public delegate void EventHandler(int health, int maxhealth);
    public EventHandler OnHealthChange;
    public EventHandler OnManaChange;
    public EventHandler OnFocusChange;
    public delegate void DamageEventHandler(AttackInformation attackInfo);
    public delegate void DamageEvent();
    public bool isDead = false;
    public DamageEventHandler OnDamageTaken;
    public DamageEvent OnDead;

    public bool HasMaxHealth()
    {
        return Health >= character.GetReference().MaxHealth;
    }

    public bool HasMaxMana()
    {
        return Mana >= character.GetReference().MaxMana;
    }

    public bool HasMaxFocus()
    {
        return Focus >= MaxFocus;
    }

    public void AddToMana(int amount)
    {
        if (Mana + amount <= 0)
        {
            Mana = 0;
        }
        else if (Mana + amount >= character.GetReference().MaxMana)
        {

            Mana = character.GetReference().MaxMana;
        }
        else
        {
            Mana += amount;
        }
        if (character)
        {
            OnManaChange?.Invoke(Mana, character.GetReference().MaxMana);
        }
    }
    private void Awake()
    {
        character = GetComponent<BattleCharacter>();
    }
    public void ResetHealth()
    {

        Health = character.GetReference().MaxHealth;
        character.GetReference().isDead = false;
    }


    public void AddFocus(int amount)
    {
        Focus = Mathf.Clamp(Focus + amount, 0, MaxFocus);
        OnFocusChange?.Invoke(Focus, MaxFocus);
    }
    public void AddToHealth(int amount)
    {
        if(amount < 0)
        {
            character.turnsNotAttacked = 0;
        }
        if (Health + amount <= 0)
        {

            Health = 0;



            if (!isDead)
            {
                isDead = true;
                OnDead?.Invoke();
                character.Animator.Die();
                CamManager.Shake(.2f, .1f);
            }



        }
        else
        {
            if (Health + amount >= character.GetReference().MaxHealth)
            {
                Health = character.GetReference().MaxHealth;
            }
            else
            {
                Health += amount;
            }
            if (amount < 0)
            {
                if (!isDead)
                {
                    character.Animator.Hurt();
                    CamManager.Shake(.2f, .1f);
                    character.StopBlock();
                }
            }
            else if (amount > 0)
            {
                if (isDead)
                {
                    isDead = false;
                    character.Animator.Revive();
                }
            }
        }
        if (character)
        {
            OnHealthChange?.Invoke(Health, character.GetReference().MaxHealth);
        }
    }



    public void AddToHealth(AttackInformation attackInfo)
    {



        if (attackInfo.attack)
        {
            attackInfo.amount = attackInfo.element == Element.Support ? attackInfo.attack.GetAmount() : -attackInfo.attack.GetAmount();
        }







        if (attackInfo.element != Element.Support)
        {
            if (character.isBlocking)
            {
                attackInfo.amount /= 2;
            }


            if (character.isParrying && !attackInfo.command.WillKokusen())
            {
                AddFocus(Mathf.Abs(attackInfo.GetAmount()));
                attackInfo.amount = 0;
                character.StopBlock();
                character.Animator.Parry();
                character.PlaySFX(Resources.Load<AudioClip>("39_Block_03"));
                StartCoroutine(Utils.SlowDown(1.1f, .3f));
            }

            if (lastAttack != null)
            {
                if (attackInfo.ID != lastAttack.ID)
                {
                    attackInfo.HandleRecipe(character);
                }
                else
                {

                    if (lastAttack.effect == ElementEffect.RecipeBoosted || lastAttack.effect == ElementEffect.RecipeCompleted)
                    {
                        attackInfo.effect = lastAttack.effect;
                        attackInfo.RecipeIndex = lastAttack.RecipeIndex;
                        attackInfo.HandleDamage();
                        attackInfo.TriggersChain = false;
                    }
                }
            }
            else
            {
                attackInfo.HandleRecipe(character);
            }

            

        }

        if (attackInfo.effect == ElementEffect.RecipeCompleted)
        {
            GameObject obj = Instantiate(BattleManager.Singleton?.KOKUSEN, transform.position + Vector3.up, Quaternion.identity);
            GameObject obj2 = Instantiate(BattleManager.Singleton?.KOKUSENSPEEDLINES, transform.position + Vector3.up, Quaternion.identity);

            StartCoroutine(Utils.SlowDown(1f, .5f));
        }







        if (attackInfo.GetAmount() != 0)
        {

            if (attackInfo.attack?.GetHitEffect() != null)
            {
                GameObject obj = Instantiate(attackInfo.attack.GetHitEffect(), transform.position + Vector3.up, Quaternion.identity);
                Vector3 rotation = Vector3.zero;
                if (character.IsFacing() == -1)
                {
                    rotation.z = 180;
                }
                obj.transform.rotation = Quaternion.Euler(rotation);
            }
            else
            {
                if (attackInfo.source.GetReference().GetHitEffect() != null)
                {
                    GameObject obj = Instantiate(attackInfo.source.GetReference().GetHitEffect(), transform.position + Vector3.up, Quaternion.identity);
                    Vector3 rotation = Vector3.zero;
                    if (character.IsFacing() == -1)
                    {
                        rotation.z = 180;
                    }
                    obj.transform.rotation = Quaternion.Euler(rotation);
                }
            }
            if (attackInfo.attack?.GetSoundEffect() != null)
            {
                attackInfo.source.PlaySFX(attackInfo.attack.GetSoundEffect());
            }
            else
            {
                if (attackInfo.source.GetReference().GetSoundEffect() != null)
                {
                    attackInfo.source.PlaySFX(attackInfo.source.GetReference().GetSoundEffect());
                }
            }

        }
        AddToHealth(attackInfo.GetAmount());



        lastAttack = attackInfo;
        // Invoke the damage taken event
        OnDamageTaken.Invoke(attackInfo);
        if (!isDead)
        {
            if (attackInfo.effect == ElementEffect.RecipeBoosted && attackInfo.TriggersChain)
            {
                if (attackInfo.source.IsPlayerTeam())
                {
                    if (attackInfo.source == BattleManager.Singleton.GetActor())
                    {
                        attackInfo.command.OnRecipeMatched?.Invoke();
                        //attackInfo.command.nextCommand = c;
                        CamManager.PanToCharacter(attackInfo.source);
                        //attackInfo.source.CancelAttack();
                    }
                }
            }
        }
    }

    public void AddToMana(IActionData attack, BattleCharacter source = null)
    {


        int amount = attack.GetAmount();






        if (amount != 0)
        {

            if (attack?.GetHitEffect() != null)
            {
                Instantiate(attack.GetHitEffect(), transform.position + Vector3.up, Quaternion.identity);
            }
            else
            {
                if (source.GetReference().GetHitEffect() != null)
                {
                    Instantiate(source.GetReference().GetHitEffect(), transform.position + Vector3.up, Quaternion.identity);
                }
            }
            if (attack?.GetSoundEffect() != null)
            {
                source.PlaySFX(attack.GetSoundEffect());
            }
            else
            {
                if (source.GetReference().GetSoundEffect() != null)
                {
                    source.PlaySFX(source.GetReference().GetSoundEffect());
                }
            }

        }
        AddToMana(amount);
        if (amount > 0)
        {
            GetComponent<TextEffect>().SpawnTextEffect(amount, Color.cyan);
        }
    }

    public void Apply()
    {
        character.GetReference().Health = Health;
        character.GetReference().Mana = Mana;
        character.GetReference().isDead = isDead;
    }

    public void LoadReference()
    {
        Health = character.GetReference().Health;
        Mana = character.GetReference().Mana;
        isDead = character.GetReference().isDead;

        if (isDead)
        {
            character.Animator.Die();
        }



        AddToHealth(0);
        AddToMana(0);
    }

    public void LoadReferenceRefreshed()
    {
        Health = character.GetReference().MaxHealth;
        Mana = character.GetReference().MaxMana;
        isDead = false;



        AddToHealth(0);
        AddToMana(0);
    }
}
