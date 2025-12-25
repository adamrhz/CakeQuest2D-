using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SkillCommand : AttackCommand
{

    protected Skill attack;

    public SkillCommand(Skill _attack)
    {
        attack = _attack;
        friendliness = attack.friendliness;
    }
    public override void ExecuteCommand()
    {
        Source.Entity.AddToMana(-attack.manaCost);
        base.ExecuteCommand();
    }



    public override bool IsPhysical()
    {
        return attack.skillType == SkillType.Physical;
    }
    public override void SetTarget(List<BattleCharacter> _target)
    {
        Target = new List<BattleCharacter>();
        if (attack.targetType == TargetType.Single)
        {
            Target.Add(_target[Random.Range(0, _target.Count)]);
        }
        else
        {
            Target = _target;
        }
    }

    public override void ActivateCommand()
    {
        attack.UseSkill(Source, Target);
    }

    public override void ActivateCommand(BattleCharacter _target)
    {
        List<BattleCharacter> temp = new List<BattleCharacter>();
        temp.Add(_target);
        attack.UseSkill(Source, temp);

    }

    public override Element GetElement()
    {
        return attack.element;
    }


    public override bool CanBeTarget(BattleCharacter _character)
    {

        return base.CanBeTarget(_character) == (attack.targetStateType == TargetStateType.Alive);
    }
    public override IEnumerator WaitForAnimationOver()
    {
        Source.Animator.Attack();
        Source.ApplyAttackAnimationOverride(attack);
        yield return new WaitForSeconds(.1f);
        while (Source.Animator.GetCurrentAnim().clip == attack.animationClip && Source.Animator.GetCurrentAnimTime() < Source.Animator.GetCurrentAnimLen())
        {
            yield return null;
        }
        Source.ResetAnimatorController();

    }
    public Skill GetAttack()
    {
        return attack;
    }
}
