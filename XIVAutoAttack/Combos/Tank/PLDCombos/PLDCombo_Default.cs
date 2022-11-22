using System.Collections.Generic;
using XIVAutoAttack.Actions;
using XIVAutoAttack.Combos.Basic;
using XIVAutoAttack.Combos.CustomCombo;
using XIVAutoAttack.Data;
using XIVAutoAttack.Helpers;
using XIVAutoAttack.Updaters;
using static XIVAutoAttack.Combos.Tank.PLDCombos.PLDCombo_Default;

namespace XIVAutoAttack.Combos.Tank.PLDCombos;

internal sealed class PLDCombo_Default : PLDCombo_Base<CommandType>
{
    public override string Author => "manju";

    internal enum CommandType : byte
    {
        None,
    }

    protected override SortedList<CommandType, string> CommandDescription => new SortedList<CommandType, string>()
    {
        //{CommandType.None, "" }, //写好注释啊！用来提示用户的。
    };


    protected override bool CanHealSingleSpell => TargetUpdater.PartyMembers.Length == 1 && base.CanHealSingleSpell;

    //private bool SlowLoop = false;

    public override SortedList<DescType, string> DescriptionDict => new()
    {
        {DescType.单体治疗, $"{Clemency}"},
        {DescType.范围防御, $"{DivineVeil}, {PassageofArms}"},
        {DescType.单体防御, $"{Sentinel}, {Sheltron}"},
        {DescType.移动技能, $"{Intervene}"},
    };

    private protected override bool GeneralGCD(out IAction act)
    {
        //三个大招
        if (BladeofValor.ShouldUse(out act, mustUse: true)) return true;
        if (BladeofFaith.ShouldUse(out act, mustUse: true)) return true;
        if (BladeofTruth.ShouldUse(out act, mustUse: true)) return true;

        //魔法三种姿势
        if (CanUseConfiteor(out act)) return true;

        //AOE 二连
        if (Prominence.ShouldUse(out act)) return true;
        if (TotalEclipse.ShouldUse(out act)) return true;

        //赎罪剑
        if (Atonement.ShouldUse(out act))
        {
            if (Player.HasStatus(true, StatusID.FightOrFlight)
                   && IsLastWeaponSkill(true, Atonement, RageofHalone)
                   && !Player.WillStatusEndGCD(2, 0, true, StatusID.FightOrFlight)) return true;

            if (Player.StatusStack(true, StatusID.SwordOath) > 1) return true;
        }
        //单体三连
        if (GoringBlade.ShouldUse(out act)) return true;
        if (RageofHalone.ShouldUse(out act)) return true;
        if (RiotBlade.ShouldUse(out act)) return true;
        if (FastBlade.ShouldUse(out act)) return true;

        //投盾
        if (CommandController.Move && MoveAbility(1, out act)) return true;
        if (ShieldLob.ShouldUse(out act)) return true;

        return false;
    }

    private protected override bool MoveAbility(byte abilityRemain, out IAction act)
    {
        //调停
        if (Intervene.ShouldUse(out act, emptyOrSkipCombo: true)) return true;

        return false;
    }

    private protected override bool HealSingleGCD(out IAction act)
    {
        //深仁厚泽
        if (Clemency.ShouldUse(out act)) return true;

        return false;
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        //圣光幕帘
        if (DivineVeil.ShouldUse(out act)) return true;

        //武装戍卫
        if (PassageofArms.ShouldUse(out act)) return true;

        if (Reprisal.ShouldUse(out act, mustUse: true)) return true;

        return false;
    }

    private protected override bool AttackAbility(byte abilityRemain, out IAction act)
    {
        if (SettingBreak)
        {
            //战逃反应 加Buff
            if (Player.HasStatus(true, StatusID.RiotBlade) && CanUseFightorFlight(out act)) return true;

            //安魂祈祷
            //if (SlowLoop && CanUseRequiescat(out act)) return true;
            if (abilityRemain == 1 && CanUseRequiescat(out act)) return true;
        }


        //厄运流转
        if (CircleofScorn.ShouldUse(out act, mustUse: true))
        {
            if (!IsFullParty) return true;

            if (FightorFlight.ElapsedAfterGCD(2)) return true;

            //if (SlowLoop && inOpener && IsLastWeaponSkill(false, Actions.RiotBlade)) return true;

            //if (!SlowLoop && inOpener && OpenerStatus && IsLastWeaponSkill(true, Actions.RiotBlade)) return true;

        }

        //深奥之灵
        if (SpiritsWithin.ShouldUse(out act, mustUse: true))
        {
            //if (SlowLoop && inOpener && IsLastWeaponSkill(true, Actions.RiotBlade)) return true;

            if (!IsFullParty) return true;

            if (FightorFlight.ElapsedAfterGCD(3)) return true;
        }

        //调停
        if (Intervene.Target.DistanceToPlayer() < 1 && !IsMoving && Target.HasStatus(true, StatusID.GoringBlade))
        {
            if (FightorFlight.ElapsedAfterGCD(2) && Intervene.ShouldUse(out act, emptyOrSkipCombo: true)) return true;

            if (Intervene.ShouldUse(out act)) return true;
        }

        //Special Defense.
        if (OathGauge == 100 && OathDefense(out act) && Player.CurrentHp < Player.MaxHp) return true;

        act = null;
        return false;
    }

    private protected override bool DefenceSingleAbility(byte abilityRemain, out IAction act)
    {
        if (OathDefense(out act)) return true;

        if (abilityRemain == 1)
        {
            //预警（减伤30%）
            if (Sentinel.ShouldUse(out act)) return true;

            //铁壁（减伤20%）
            if (Rampart.ShouldUse(out act)) return true;
        }
        //降低攻击
        //雪仇
        if (Reprisal.ShouldUse(out act)) return true;

        //干预（减伤10%）
        if (!HaveShield && Intervention.ShouldUse(out act)) return true;

        act = null;
        return false;
    }

    /// <summary>
    /// 判断能否使用战逃反应
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseFightorFlight(out IAction act)
    {
        if (FightorFlight.ShouldUse(out act))
        {
            //在4人本道中
            if (!IsFullParty)
            {
                if (!Player.HasStatus(true, StatusID.Requiescat)
                    && !Player.HasStatus(true, StatusID.ReadyForBladeofFaith)
                    && Player.CurrentMp < 2000) return true;

                return false;
            }
            //起手在先锋剑后
            return true;
        }

        act = null;
        return false;
    }

    /// <summary>
    /// 判断能否使用安魂祈祷
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseRequiescat(out IAction act)
    {
        //安魂祈祷
        if (Requiescat.ShouldUse(out act, mustUse: true))
        {
            //在战逃buff时间剩17秒以下时释放
            if (Player.HasStatus(true, StatusID.FightOrFlight) && Player.WillStatusEnd(17, true, StatusID.FightOrFlight) && Target.HasStatus(true, StatusID.GoringBlade))
            {
                //在起手中时,王权剑后释放
                return true;
            }
        }

        act = null;
        return false;
    }


    /// <summary>
    /// 悔罪,圣灵,圣环
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseConfiteor(out IAction act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.SwordOath)) return false;

        //有安魂祈祷buff,且没在战逃中
        if (Player.HasStatus(true, StatusID.Requiescat) && !Player.HasStatus(true, StatusID.FightOrFlight))
        {
            //if (SlowLoop && !IsLastWeaponSkill(true, GoringBlade) && !IsLastWeaponSkill(true, Atonement)) return false;

            var statusStack = Player.StatusStack(true, StatusID.Requiescat);
            if (statusStack == 1 || Player.HasStatus(true, StatusID.Requiescat) && Player.WillStatusEnd(3, false, StatusID.Requiescat) || Player.CurrentMp <= 2000)
            {
                if (Confiteor.ShouldUse(out act, mustUse: true)) return true;
            }
            else
            {
                if (HolyCircle.ShouldUse(out act)) return true;
                if (HolySpirit.ShouldUse(out act)) return true;
            }
        }

        act = null;
        return false;
    }


    private bool OathDefense(out IAction act)
    {
        if (HaveShield)
        {
            //盾阵
            if (Sheltron.ShouldUse(out act)) return true;
        }
        else
        {
            //保护
            if (Cover.ShouldUse(out act)) return true;
        }

        return false;
    }
}
