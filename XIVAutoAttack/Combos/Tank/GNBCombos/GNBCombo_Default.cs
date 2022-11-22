using System.Collections.Generic;
using XIVAutoAttack.Actions;
using XIVAutoAttack.Combos.Basic;
using XIVAutoAttack.Combos.CustomCombo;
using XIVAutoAttack.Data;
using XIVAutoAttack.Helpers;
using XIVAutoAttack.Updaters;
using static XIVAutoAttack.Combos.Tank.GNBCombos.GNBCombo_Default;

namespace XIVAutoAttack.Combos.Tank.GNBCombos;

internal sealed class GNBCombo_Default : GNBCombo_Base<CommandType>
{
    public override string Author => "cese";

    internal enum CommandType : byte
    {
        None,
    }

    protected override SortedList<CommandType, string> CommandDescription => new SortedList<CommandType, string>()
    {
        //{CommandType.None, "" }, //写好注释啊！用来提示用户的。
    };
    protected override bool CanHealSingleSpell => false;
    protected override bool CanHealAreaSpell => false;

    public override SortedList<DescType, string> DescriptionDict => new()
    {
        {DescType.单体治疗, $"{Aurora}"},
        {DescType.范围防御, $"{HeartofLight}"},
        {DescType.单体防御, $"{HeartofStone}, {Nebula}, {Camouflage}"},
        {DescType.移动技能, $"{RoughDivide}"},
    };

    private protected override bool GeneralGCD(out IAction act)
    {
        //烈牙
        if (CanUseGnashingFang(out act)) return true;

        //音速破
        if (CanUseSonicBreak(out act)) return true;

        //倍攻
        if (CanUseDoubleDown(out act)) return true;

        //烈牙后二连
        if (WickedTalon.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
        if (SavageClaw.ShouldUse(out act, emptyOrSkipCombo: true)) return true;

        //命运之环 AOE
        if (FatedCircle.ShouldUse(out act)) return true;

        //爆发击   
        if (CanUseBurstStrike(out act)) return true;

        //AOE
        if (DemonSlaughter.ShouldUse(out act)) return true;
        if (DemonSlice.ShouldUse(out act)) return true;

        //单体三连
        //如果烈牙剩0.5秒冷却好,不释放基础连击,主要因为技速不同可能会使烈牙延后太多所以判定一下
        if (GnashingFang.IsCoolDown && GnashingFang.WillHaveOneCharge(0.5f) && GnashingFang.EnoughLevel) return false;
        if (SolidBarrel.ShouldUse(out act)) return true;
        if (BrutalShell.ShouldUse(out act)) return true;
        if (KeenEdge.ShouldUse(out act)) return true;

        if (CommandController.Move && MoveAbility(1, out act)) return true;

        if (LightningShot.ShouldUse(out act))
        {
            if (!IsFullParty && LightningShot.Target.DistanceToPlayer() > 3) return true;
        }
        return false;
    }



    private protected override bool AttackAbility(byte abilityRemain, out IAction act)
    {
        //无情,目前只有4GCD起手的判断
        if (SettingBreak && abilityRemain == 1 && CanUseNoMercy(out act)) return true;

        //危险领域
        if (DangerZone.ShouldUse(out act))
        {
            if (!IsFullParty) return true;

            //等级小于烈牙,
            if (!GnashingFang.EnoughLevel && (Player.HasStatus(true, StatusID.NoMercy) || !NoMercy.WillHaveOneCharge(15))) return true;

            //爆发期,烈牙之后
            if (Player.HasStatus(true, StatusID.NoMercy) && GnashingFang.IsCoolDown) return true;

            //非爆发期
            if (!Player.HasStatus(true, StatusID.NoMercy) && !GnashingFang.WillHaveOneCharge(20)) return true;
        }

        //弓形冲波
        if (CanUseBowShock(out act)) return true;

        //续剑
        if (JugularRip.ShouldUse(out act)) return true;
        if (AbdomenTear.ShouldUse(out act)) return true;
        if (EyeGouge.ShouldUse(out act)) return true;
        if (Hypervelocity.ShouldUse(out act)) return true;

        //血壤
        if (GnashingFang.IsCoolDown && Bloodfest.ShouldUse(out act)) return true;

        //搞搞攻击,粗分斩
        if (RoughDivide.Target.DistanceToPlayer() < 1 && !IsMoving)
        {
            if (RoughDivide.ShouldUse(out act)) return true;
            if (Player.HasStatus(true, StatusID.NoMercy) && RoughDivide.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
        }
        act = null;
        return false;
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        if (HeartofLight.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
        if (Reprisal.ShouldUse(out act, mustUse: true)) return true;
        return false;
    }

    private protected override bool MoveAbility(byte abilityRemain, out IAction act)
    {
        //突进
        if (RoughDivide.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
        return false;
    }
    private protected override bool DefenceSingleAbility(byte abilityRemain, out IAction act)
    {
        if (abilityRemain == 2)
        {
            //减伤10%）
            if (HeartofStone.ShouldUse(out act)) return true;

            //星云（减伤30%）
            if (Nebula.ShouldUse(out act)) return true;

            //铁壁（减伤20%）
            if (Rampart.ShouldUse(out act)) return true;

            //伪装（减伤10%）
            if (Camouflage.ShouldUse(out act)) return true;
        }
        //降低攻击
        //雪仇
        if (Reprisal.ShouldUse(out act)) return true;

        act = null;
        return false;
    }

    private protected override bool HealSingleAbility(byte abilityRemain, out IAction act)
    {
        if (Aurora.ShouldUse(out act, emptyOrSkipCombo: true) && abilityRemain == 1) return true;

        return false;
    }

    private bool CanUseNoMercy(out IAction act)
    {
        //等级低于爆发击是判断
        if (!BurstStrike.EnoughLevel && NoMercy.ShouldUse(out act)) return true;

        if (BurstStrike.EnoughLevel && NoMercy.ShouldUse(out act))
        {
            //4GCD起手判断
            if (IsLastWeaponSkill((ActionID)KeenEdge.ID) && Ammo == 1 && !GnashingFang.IsCoolDown && !Bloodfest.IsCoolDown) return true;

            //3弹进无情
            else if (Ammo == (Level >= 88 ? 3 : 2)) return true;

            //2弹进无情
            else if (Ammo == 2 && GnashingFang.IsCoolDown) return true;
        }


        act = null;
        return false;
    }

    private bool CanUseGnashingFang(out IAction act)
    {
        //基础判断
        if (GnashingFang.ShouldUse(out act))
        {
            //在4人本道中使用
            if (!IsFullParty) return true;

            //无情中3弹烈牙
            if (Ammo == (Level >= 88 ? 3 : 2) && (Player.HasStatus(true, StatusID.NoMercy) || !NoMercy.WillHaveOneCharge(55))) return true;

            //无情外烈牙
            if (Ammo > 0 && !NoMercy.WillHaveOneCharge(17) && NoMercy.WillHaveOneCharge(35)) return true;

            //3弹且将会溢出子弹的情况,提前在无情前进烈牙
            if (Ammo == 3 && IsLastWeaponSkill((ActionID)BrutalShell.ID) && NoMercy.WillHaveOneCharge(3)) return true;

            //1弹且血壤快冷却好了
            if (Ammo == 1 && !NoMercy.WillHaveOneCharge(55) && Bloodfest.WillHaveOneCharge(5)) return true;

            //4GCD起手烈牙判断
            if (Ammo == 1 && !NoMercy.WillHaveOneCharge(55) && (!Bloodfest.IsCoolDown && Bloodfest.EnoughLevel || !Bloodfest.EnoughLevel)) return true;
        }
        return false;
    }

    /// <summary>
    /// 音速破
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseSonicBreak(out IAction act)
    {
        //基础判断
        if (SonicBreak.ShouldUse(out act))
        {
            //在4人本道中不使用
            if (!IsFullParty) return false;

            if (!GnashingFang.EnoughLevel && Player.HasStatus(true, StatusID.NoMercy)) return true;

            //在烈牙后面使用音速破
            if (GnashingFang.IsCoolDown && Player.HasStatus(true, StatusID.NoMercy)) return true;

            //其他判断
            if (!DoubleDown.EnoughLevel && Player.HasStatus(true, StatusID.ReadyToRip)
                && GnashingFang.IsCoolDown) return true;

        }
        return false;
    }

    /// <summary>
    /// 倍攻
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseDoubleDown(out IAction act)
    {
        //基本判断
        if (DoubleDown.ShouldUse(out act, mustUse: true))
        {
            //在4人本道中
            if (IsFullParty)
            {
                //在4人本的道中已经聚好怪可以使用相关技能(不移动且身边有大于3只小怪),有无情buff
                if (Player.HasStatus(true, StatusID.NoMercy)) return true;

                return false;
            }

            //在音速破后使用倍攻
            if (SonicBreak.IsCoolDown && Player.HasStatus(true, StatusID.NoMercy)) return true;

            //2弹无情的特殊判断,提前使用倍攻
            if (Player.HasStatus(true, StatusID.NoMercy) && !NoMercy.WillHaveOneCharge(55) && Bloodfest.WillHaveOneCharge(5)) return true;

        }
        return false;
    }

    /// <summary>
    /// 爆发击
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseBurstStrike(out IAction act)
    {
        if (BurstStrike.ShouldUse(out act))
        {
            //在4人本道中且AOE时不使用
            if (IsFullParty && DemonSlice.ShouldUse(out _)) return false;

            //如果烈牙剩0.5秒冷却好,不释放爆发击,主要因为技速不同可能会使烈牙延后太多所以判定一下
            if (SonicBreak.IsCoolDown && SonicBreak.WillHaveOneCharge(0.5f) && GnashingFang.EnoughLevel) return false;

            //无情中爆发击判定
            if (Player.HasStatus(true, StatusID.NoMercy) &&
                AmmoComboStep == 0 &&
                !GnashingFang.WillHaveOneCharge(1)) return true;
            if (Level < 88 && Ammo == 2) return true;
            //无情外防止溢出
            if (IsLastWeaponSkill((ActionID)BrutalShell.ID) &&
                (Ammo == (Level >= 88 ? 3 : 2) ||
                Bloodfest.WillHaveOneCharge(6) && Ammo <= 2 && !NoMercy.WillHaveOneCharge(10) && Bloodfest.EnoughLevel)) return true;

        }
        return false;
    }

    private bool CanUseBowShock(out IAction act)
    {
        if (BowShock.ShouldUse(out act, mustUse: true))
        {
            if (!IsFullParty) return true;

            if (!SonicBreak.EnoughLevel && Player.HasStatus(true, StatusID.NoMercy)) return true;

            //爆发期,无情中且音速破在冷却中
            if (Player.HasStatus(true, StatusID.NoMercy) && SonicBreak.IsCoolDown) return true;

        }
        return false;
    }
}

