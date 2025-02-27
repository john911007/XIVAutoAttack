using Dalamud.Game.ClientState.Objects.Types;
using System.Collections.Generic;
using System.Linq;
using XIVAutoAttack.Actions;
using XIVAutoAttack.Actions.BaseAction;
using XIVAutoAttack.Combos.Basic;
using XIVAutoAttack.Combos.CustomCombo;
using XIVAutoAttack.Configuration;
using XIVAutoAttack.Data;
using XIVAutoAttack.Helpers;
using XIVAutoAttack.Updaters;
using static XIVAutoAttack.Combos.Healer.WHMCombos.WHMCombo_Default;

namespace XIVAutoAttack.Combos.Healer.WHMCombos;
internal sealed class WHMCombo_Default : WHMCombo_Base<CommandType>
{
    public override string Author => "逆光";

    internal enum CommandType : byte
    {
        None,
    }

    private protected override ActionConfiguration CreateConfiguration()
    {
        return base.CreateConfiguration().SetBool("UseLilyWhenFull", true, "蓝花集满时自动释放蓝花")
                                            .SetBool("UsePreRegen", false, "开始5秒时给t上盾和再生");
    }
    public override SortedList<DescType, string> DescriptionDict => new()
    {
        {DescType.范围治疗, $"GCD: {AfflatusRapture}, {Medica2}, {Cure3}, {Medica}\n                     能力: {Asylum}, {Assize}"},
        {DescType.单体治疗, $"GCD: {AfflatusSolace}, {Regen}, {Cure2}, {Cure}\n                     能力: {Tetragrammaton}"},
        {DescType.范围防御, $"{Temperance}, {LiturgyoftheBell}"},
        {DescType.单体防御, $"{DivineBenison}, {Aquaveil}"},
    };
    private protected override bool GeneralGCD(out IAction act)
    {
        //苦难之心
        if (AfflatusMisery.ShouldUse(out act, mustUse: true)) return true;

        //泄蓝花 团队缺血时优先狂喜之心
        bool liliesNearlyFull = Lily == 2 && LilyAfter(17);
        bool liliesFullNoBlood = Lily == 3 && BloodLily < 3;
        if (Config.GetBoolByName("UseLilyWhenFull") && (liliesNearlyFull || liliesFullNoBlood))
        {
            if (TargetUpdater.PartyMembersAverHP < 0.7)
            {
                if (AfflatusRapture.ShouldUse(out act)) return true;
            }
            if (AfflatusSolace.ShouldUse(out act)) return true;

        }

        //群体输出
        if (Holy.ShouldUse(out act)) return true;

        //单体输出
        if (Aero.ShouldUse(out act, mustUse: IsMoving && HaveHostilesInRange)) return true;
        if (Stone.ShouldUse(out act)) return true;


        act = null;
        return false;
    }

    private protected override bool AttackAbility(byte abilityRemain, out IAction act)
    {
        //加个神速咏唱
        if (PresenseOfMind.ShouldUse(out act)) return true;

        //加个法令
        if (Assize.ShouldUse(out act, mustUse: true)) return true;

        return false;
    }

    private protected override bool EmergencyAbility(byte abilityRemain, IAction nextGCD, out IAction act)
    {
        //加个无中生有
        if (nextGCD is BaseAction action && action.MPNeed >= 1000 &&
            ThinAir.ShouldUse(out act)) return true;

        //加个全大赦,狂喜之心 医济医治愈疗
        if (nextGCD.IsAnySameAction(true, AfflatusRapture, Medica, Medica2, Cure3))
        {
            if (PlenaryIndulgence.ShouldUse(out act)) return true;
        }

        return base.EmergencyAbility(abilityRemain, nextGCD, out act);
    }

    private protected override bool HealSingleGCD(out IAction act)
    {
        //安慰之心
        if (AfflatusSolace.ShouldUse(out act)) return true;

        //再生
        if (Regen.ShouldUse(out act)) return true;

        //救疗
        if (Cure2.ShouldUse(out act)) return true;

        //治疗
        if (Cure.ShouldUse(out act)) return true;

        return false;
    }

    private protected override bool HealSingleAbility(byte abilityRemain, out IAction act)
    {
        //天赐 大资源救急用
        if (Benediction.Target.GetHealthRatio() < 0.3
            && Benediction.ShouldUse(out act)) return true;

        //神名
        if (Tetragrammaton.ShouldUse(out act)) return true;

        //庇护所
        if (Asylum.ShouldUse(out act)) return true;

        //天赐
        if (Benediction.ShouldUse(out act)) return true;

        return false;
    }

    private protected override bool HealAreaGCD(out IAction act)
    {
        //狂喜之心
        if (AfflatusRapture.ShouldUse(out act)) return true;

        var PartyMembers = TargetUpdater.PartyMembers;
        int hasMedica2 = PartyMembers.Count((n) => n.HasStatus(true, StatusID.Medica2));

        //!Player.HaveStatus(true, StatusID.Medica2)
        //医济 在小队半数人都没有医济buff and 上次没放医济时使用
        if (Medica2.ShouldUse(out act) && !(hasMedica2 < PartyMembers.Length / 2) && !IsLastAction(true, Medica2)) return true;

        //愈疗
        if (Cure3.ShouldUse(out act)) return true;

        //医治
        if (Medica.ShouldUse(out act)) return true;

        return false;
    }

    private protected override bool HealAreaAbility(byte abilityRemain, out IAction act)
    {
        //庇护所
        if (Asylum.ShouldUse(out act)) return true;

        act = null;
        return false;
    }

    private protected override bool DefenceSingleAbility(byte abilityRemain, out IAction act)
    {
        //神祝祷
        if (DivineBenison.ShouldUse(out act)) return true;

        //水流幕
        if (Aquaveil.ShouldUse(out act)) return true;
        return false;
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        //节制
        if (Temperance.ShouldUse(out act)) return true;

        //礼仪之铃
        if (LiturgyoftheBell.ShouldUse(out act)) return true;
        return false;
    }
    //开局5s使用再生和盾给开了盾姿的t
    private protected override IAction CountDownAction(float remainTime)
    {
        if (Config.GetBoolByName("UsePreRegen") && remainTime <= 5 && remainTime > 3 && DivineBenison.ShouldUse(out _))
        {
            BattleChara mt = Player;
            foreach (BattleChara t in TargetUpdater.PartyTanks)
            {
                if (t.HasStatus(true, StatusID.Defiance) || t.HasStatus(true, StatusID.IronWill) ||
                    t.HasStatus(true, StatusID.Grit) || t.HasStatus(true, StatusID.RoyalGuard)) mt = t;
            }
            if (DivineBenison.ShouldUse(out _))
            {
                DivineBenison.Target = mt;
                return DivineBenison;
            }
            if (Regen.ShouldUse(out _))
            {
                Regen.Target = mt;
                return Regen;
            }

        }
        return base.CountDownAction(remainTime);
    }
}