using System.Collections.Generic;
using XIVAutoAttack.Actions;
using XIVAutoAttack.Combos.Basic;
using XIVAutoAttack.Combos.CustomCombo;
using XIVAutoAttack.Configuration;
using XIVAutoAttack.Data;
using XIVAutoAttack.Helpers;
using static XIVAutoAttack.Combos.Melee.DRGCombos.DRGCombo_Default;

namespace XIVAutoAttack.Combos.Melee.DRGCombos;

internal sealed class DRGCombo_Default : DRGCombo_Base<CommandType>
{
    public override string Author => "汐ベMoon";

    internal enum CommandType : byte
    {
        None,
    }

    protected override SortedList<CommandType, string> CommandDescription => new SortedList<CommandType, string>()
    {
        //{CommandType.None, "" }, //写好注释啊！用来提示用户的。
    };
    private static bool safeMove = false;


    private protected override ActionConfiguration CreateConfiguration()
    {
        return base.CreateConfiguration().SetBool("DRG_ShouldDelay", true, "延后红龙血")
            .SetBool("DRG_Opener", false, "88级起手")
            .SetBool("DRG_SafeMove", true, "安全位移");
    }

    public override SortedList<DescType, string> DescriptionDict => new SortedList<DescType, string>()
    {
        {DescType.移动技能, $"{SpineshatterDive}, {DragonfireDive}"},
    };

    private protected override bool MoveAbility(byte abilityRemain, out IAction act)
    {
        if (abilityRemain > 1)
        {
            if (SpineshatterDive.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
            if (DragonfireDive.ShouldUse(out act, mustUse: true)) return true;
        }

        act = null;
        return false;
    }
    private protected override bool EmergencyAbility(byte abilityRemain, IAction nextGCD, out IAction act)
    {
        if (nextGCD.IsAnySameAction(true, FullThrust, CoerthanTorment)
            || Player.HasStatus(true, StatusID.LanceCharge) && nextGCD.IsAnySameAction(false, FangandClaw))
        {
            //龙剑
            if (abilityRemain == 1 && LifeSurge.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
        }

        return base.EmergencyAbility(abilityRemain, nextGCD, out act);
    }

    private protected override bool AttackAbility(byte abilityRemain, out IAction act)
    {
        if (SettingBreak)
        {
            //猛枪
            if (LanceCharge.ShouldUse(out act, mustUse: true))
            {
                if (abilityRemain == 1 && !Player.HasStatus(true, StatusID.PowerSurge)) return true;
                if (Player.HasStatus(true, StatusID.PowerSurge)) return true;
            }

            //巨龙视线
            if (DragonSight.ShouldUse(out act, mustUse: true)) return true;

            //战斗连祷
            if (BattleLitany.ShouldUse(out act, mustUse: true)) return true;
        }

        //死者之岸
        if (Nastrond.ShouldUse(out act, mustUse: true)) return true;

        //坠星冲
        if (Stardiver.ShouldUse(out act, mustUse: true)) return true;

        //高跳
        if (HighJump.EnoughLevel)
        {
            if (HighJump.ShouldUse(out act)) return true;
        }
        else
        {
            if (Jump.ShouldUse(out act)) return true;
        }

        //尝试进入红龙血
        if (Geirskogul.ShouldUse(out act, mustUse: true)) return true;

        //破碎冲
        if (SpineshatterDive.ShouldUse(out act, emptyOrSkipCombo: true))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceCharge.ElapsedAfterGCD(3)) return true;
        }
        if (Player.HasStatus(true, StatusID.PowerSurge) && SpineshatterDive.CurrentCharges != 1 && SpineshatterDive.ShouldUse(out act)) return true;

        //幻象冲
        if (MirageDive.ShouldUse(out act)) return true;

        //龙炎冲
        if (DragonfireDive.ShouldUse(out act, mustUse: true))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceCharge.ElapsedAfterGCD(3)) return true;
        }

        //天龙点睛
        if (WyrmwindThrust.ShouldUse(out act, mustUse: true)) return true;

        return false;
    }

    private protected override bool GeneralGCD(out IAction act)
    {
        safeMove = Config.GetBoolByName("DRG_SafeMove");

        #region 群伤
        if (CoerthanTorment.ShouldUse(out act)) return true;
        if (SonicThrust.ShouldUse(out act)) return true;
        if (DoomSpike.ShouldUse(out act)) return true;

        #endregion

        #region 单体
        if (Config.GetBoolByName("ShouldDelay"))
        {
            if (WheelingThrust.ShouldUse(out act)) return true;
            if (FangandClaw.ShouldUse(out act)) return true;
        }
        else
        {
            if (FangandClaw.ShouldUse(out act)) return true;
            if (WheelingThrust.ShouldUse(out act)) return true;
        }

        //看看是否需要续Buff
        if (!Player.WillStatusEndGCD(5, 0, true, StatusID.PowerSurge))
        {
            if (FullThrust.ShouldUse(out act)) return true;
            if (VorpalThrust.ShouldUse(out act)) return true;
            if (ChaosThrust.ShouldUse(out act)) return true;
        }
        else
        {
            if (Disembowel.ShouldUse(out act)) return true;
        }
        if (TrueThrust.ShouldUse(out act)) return true;

        if (CommandController.Move && MoveAbility(1, out act)) return true;
        if (PiercingTalon.ShouldUse(out act)) return true;

        return false;

        #endregion
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        //牵制
        if (Feint.ShouldUse(out act)) return true;
        return false;
    }
}
