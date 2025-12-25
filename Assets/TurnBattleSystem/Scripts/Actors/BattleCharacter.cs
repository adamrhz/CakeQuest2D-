
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterState
{
    Idle,
    Choosing,
    Died
}
[RequireComponent(typeof(TeamComponent))]
public class BattleCharacter : MonoBehaviour
{

    [SerializeField] CharacterObject currentCharacter;
    public CharacterState currentState;
    public BattleCharacter _target;
    public Transform EnemyContainer;

    public bool isActing = false;
    public bool isInAttackPos = false;

    public bool isBlocking = false;
    public bool isParrying = false;

    public AudioSource sfxAudioSource;
    public AudioSource voiceAudioSource;

    public Command currentCommand;


    public AnimatorController Animator;

    public Entity Entity;
    public CookingEnjoyment CookingEnjoyment;
    public int turnsNotAttacked = 0;

    public int recipeFailedIndex = 0;
    public int recipeFailsafe = 3;
    public int recipeIndex = 0;
    public List<ElementalAttribute> recipe;

    public OptionManager OptionManager;

    void Start()
    {


        //speed = currentCharacter.Speed;
        Animator = GetComponent<AnimatorController>();
        Entity = GetComponent<Entity>();
        CookingEnjoyment = GetComponent<CookingEnjoyment>();
        CookingEnjoyment.InitializeTastes(GetReference());
        GetComponentInChildren<SpriteEvents>().SetCharacter(this);
        SetRecipe();
    }


    public void SetRecipe()
    {
        recipe = new List<ElementalAttribute>();
        int length = GetReference().recipeLength;
        length += Random.Range(-1, 2);
        length = Mathf.Clamp(length, 2, 999);
        for (int i = 0; i < length; i++)
        {
            recipe.Add(new ElementalAttribute(GetPossibleIngredient()));
        }
        recipeIndex = 0;
    }

    private Element GetPossibleIngredient()
    {
        List<Element> validElements = new List<Element>();

        foreach (Element element in GetReference().IngredientWheel)
        {
            if (element != Element.None && element != Element.Support)
            {
                validElements.Add(element);
            }

        }

        Element randomElement = validElements[UnityEngine.Random.Range(0, validElements.Count)];
        return randomElement;

    }

    public void SetRecipe(List<ElementalAttribute> attributes)
    {
        recipe = attributes;
        recipeIndex = 0;
    }
    public bool HandleRecipe(AttackInformation attackInfo)
    {
        if (recipe.Count > recipeIndex)
        {
            if (recipe[recipeIndex].element == attackInfo.element)
            {
                recipe[recipeIndex].found = true;
                recipeIndex++;
                attackInfo.MatchedRecipe(recipeIndex);

                if (recipeIndex == recipe.Count)
                {
                    attackInfo.effect = ElementEffect.RecipeCompleted;
                    SetRecipe();
                }
                return true;
            }
            else
            {
                if (recipeIndex != 0)
                {
                    ResetRecipe();
                    attackInfo.effect = ElementEffect.RecipeFailed;
                    recipeFailedIndex++;
                    if (recipeFailedIndex >= recipeFailsafe)
                    {
                        attackInfo.effect = ElementEffect.RecipeSuperFailed;
                        SetRecipe();
                    }
                }
            }
        }
        return false;


    }

    public void OnEveryTurn()
    {
        if (recipeIndex != 0)
        {
            if (turnsNotAttacked > 1 || GetTeam() != BattleManager.Singleton?.GetActor().GetTeam())
            {
                ResetRecipe();
            }
            turnsNotAttacked++;
        }
    }

    public void ResetRecipe()
    {
        recipeIndex = 0;
    }

    public void GiveNextCommand(Command command)
    {
        if (currentCommand != null)
        {
            if (currentCommand.nextCommand == null)
            {
                currentCommand.nextCommand = command;

            }
        }
    }

    public bool WillKokusen(Command command)//Lets know the current command if the current attack will be the last of a recipe.
    {
        if (command is AttackCommand)
        {
            if (recipeIndex == recipe.Count - 1)
            {
                if (command.GetElement() == recipe[recipeIndex].element)
                {
                    return true;
                }
            }
        }
        return false;
    }


    public void RevealRecipe()//Reveals the entirety of the current Battle Characters recipe
    {
        foreach (ElementalAttribute ee in recipe)
        {
            ee.found = true;
        }
    }



    public void Block()//Starts the blocking animation adn state
    {
        isBlocking = true;
        StartCoroutine(ConsumeBlock());
        Animator.Block();
    }
    public void SetActing(bool _isActing) //Lets the current BattleCharacter if it is currently performing a command
    {
        isActing = _isActing;
    }
    public CharacterData GetData() //returns the character data (CharacterData)  of the current battle character
    {
        return currentCharacter.characterData;
    }

    public CharacterObject GetReference()//returns the character reference (CharacterObject) of the current battle character
    {
        return currentCharacter;
    }

    public bool CanAct() //lets known if the BattleCharacter can act
    {
        return !isActing && !Entity.isDead;
    }

    public Command CreateCommand() //Creates a random command using the character references skill set.
    {
        float prob = Random.Range(0f, 100f);
        Command comm;
        if (prob > 50)
        {
            comm = new AttackCommand();
        }
        else
        {
            Skill attack = GetRandomAttack();
            if (attack)
            {
                comm = new SkillCommand(attack);
            }
            else
            {
                comm = new AttackCommand();
            }
        }
        comm.SetNewId();
        return comm;
    }

    public int IsFacing()//Lets know which direction the battlecharacter is facing (-1 being left and 1 being right)
    {
        return IsPlayerTeam() ? -1 : 1;
    }

    public Skill GetRandomAttack() //returns a random skill from the character reference.
    {

        List<Skill> returnAttacks = new List<Skill>();
        List<Skill> possibleAttacks = GetAttacks();
        foreach (Skill attack in possibleAttacks)
        {
            if (BattleManager.Singleton.GetPossibleTarget(attack, this).Count > 0)
            {
                returnAttacks.Add(attack);
            }
        }
        if (returnAttacks.Count == 0)
        {
            return null;
        }
        return returnAttacks[Random.Range(0, returnAttacks.Count)];
    }



    public List<Skill> GetAttacks() //returns everyskill of the current character reference.
    {
        return GetReference().Attacks;
    }



    public void StartParryWindow() //Starts the parry window of the current battle character
    {
        StartCoroutine(Parry(GetReference().GetParryWindowTime()));
    }
    IEnumerator Parry(float duration) //Handles the parry window
    {
        Parry();
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }


        StopParry();
    }


    IEnumerator ConsumeBlock() //consumes 1 focus for every second of blocking 
    {
        while (isBlocking)
        {
            yield return new WaitForSeconds(1);
            if (isBlocking && !isParrying)
            {
                Entity.AddFocus(-1);
            }
            yield return null;

        }


    }
    public void StopParry() //stops the parry state and resets the character color.
    {
        isParrying = false;
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
    }



    public void Parry()//starts the parry state and resets the character color.
    {

        isParrying = true;
        GetComponentInChildren<SpriteRenderer>().color = Color.red;
    }
    public bool IsPlayerTeam() //return if the BC is from the player party
    {
        return GetComponent<TeamComponent>().teamIndex == TeamIndex.Player;
    }

    public TeamIndex GetTeam() //returns the team index
    {
        return GetComponent<TeamComponent>().teamIndex;
    }
    public Skill GetAttack(string v) //returns an attack with the name 
    {
        foreach (Skill attack in GetAttacks())
        {
            if (attack.ToString() == v)
            {
                return attack;
            }
        }
        return null;
    }

    public void TriggerHit() //Triggers the current command effect
    {
        currentCommand.ActivateCommand();
    }


    public void ApplyAttackAnimationOverride(Skill attack) //Overrides the attack animation with the skill animation
    {
        AnimatorOverrideController originalController = new AnimatorOverrideController(Animator.GetController());
        var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (var a in originalController.animationClips)
        {
            if (a.name == "Attack_temp")
            {
                anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, attack.animationClip));
            }
        }
        originalController.ApplyOverrides(anims);
        Animator.SetController(originalController);
    }



    public void ResetAnimatorController() //Resets the current animator controller
    {
        Animator.SetController(GetReference().animationController);
    }

    public void PlaySFX(AudioClip audioClip)
    {
        PlayAudioClip(sfxAudioSource, audioClip);
    }
    public void PlayVoiceLine(AudioClip audioClip)
    {

        PlayAudioClip(voiceAudioSource, audioClip);
    }


    public void PlayAudioClip(AudioSource source, AudioClip audioClip)
    {
        if (audioClip == null)
        {
            return;
        }
        source?.PlayOneShot(audioClip);

    }
    public void SetReference(CharacterObject characterObject)
    {
        currentCharacter = characterObject;
        ResetAnimatorController();
    }

    public void Flip(int flipIndex)
    {
        transform.localScale = new Vector3(flipIndex, 1, 1);
    }

    public bool IsTargetted()
    {
        List<BattleCharacter> targets = BattleManager.Singleton.GetCurrentTarget();
        return targets.Count > 0 ? targets.Contains(this) : false;
    }



    public void SetTeam(TeamIndex index)
    {
        GetComponent<TeamComponent>().teamIndex = index;
    }

    public void StopBlock()
    {
        Animator.StopBlock();
        isBlocking = false;
    }

    public void SetOptions(int[] opt)
    {
        OptionManager.DisableMenus(opt);
    }
}
