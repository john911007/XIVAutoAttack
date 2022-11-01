using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using XIVAutoAttack.Actions;
using XIVAutoAttack.Actions.BaseAction;
using XIVAutoAttack.Combos.CustomCombo;
using XIVAutoAttack.Combos.Healer;
using XIVAutoAttack.Configuration;
using XIVAutoAttack.Data;
using XIVAutoAttack.Helpers;

namespace XIVAutoAttack.Combos.Melee;

internal sealed class DRGCombo : JobGaugeCombo<DRGGauge>
{
    internal override uint JobID => 22;
    //private static bool inOpener = false;
    private static bool safeMove = false;

    internal struct Actions
    {
        public static readonly BaseAction
            //��׼��
            TrueThrust = new (75),

            //��ͨ��
            VorpalThrust = new (78) { OtherIDsCombo = new [] { 16479u } },

            //ֱ��
            FullThrust = new (84),

            //����
            HeavensThrust = new (25771),

            //����ǹ
            Disembowel = new (87) { OtherIDsCombo = new [] { 16479u } },

            //ӣ��ŭ��
            ChaosThrust = new (88),

            //ӣ��ŭ��
            ChaoticSpring = new(25772),

            //������צ
            FangandClaw = new (3554)
            {
                BuffsNeed = new ushort[] { ObjectStatus.SharperFangandClaw },
            },

            //��β�����
            WheelingThrust = new (3556)
            {
                BuffsNeed = new ushort[] { ObjectStatus.EnhancedWheelingThrust },
            },

            //�����׵�
            RaidenThrust = new (16479),

            //�ᴩ��
            PiercingTalon = new (90),

            //����ǹ
            DoomSpike = new (86),

            //���ٴ�
            SonicThrust = new (7397) { OtherIDsCombo = new [] { 25770u } },

            //ɽ������
            CoerthanTorment = new (16477),

            //�����
            SpineshatterDive = new (95)
            {
                OtherCheck = b =>
                {
                    if (safeMove && b.DistanceToPlayer() > 2) return false;
                    if (IsLastAction(true, SpineshatterDive)) return false;

                    return true;
                }
            },

            //���׳�
            DragonfireDive = new (96)
            {
                OtherCheck = b => !safeMove || b.DistanceToPlayer() < 2,
            },

            //��Ծ
            Jump = new (92)
            {
                BuffsProvide = new ushort[] { ObjectStatus.DiveReady },
                OtherCheck = b => (!safeMove || b.DistanceToPlayer() < 2) && Player.HaveStatus(ObjectStatus.PowerSurge),
            },
            //����
            HighJump = new (16478)
            {
                OtherCheck = Jump.OtherCheck,
            },
            //�����
            MirageDive = new (7399)
            {
                BuffsNeed = new [] { ObjectStatus.DiveReady },

                OtherCheck = b => !Geirskogul.WillHaveOneChargeGCD(4)
            },

            //����ǹ
            Geirskogul = new (3555)
            {
                OtherCheck = b => Jump.IsCoolDown || HighJump.IsCoolDown,
            },

            //����֮��
            Nastrond = new (7400)
            {
                OtherCheck = b => JobGauge.IsLOTDActive,
            },

            //׹�ǳ�
            Stardiver = new (16480)
            {
                OtherCheck = b => JobGauge.IsLOTDActive && JobGauge.LOTDTimer < 25000,
            },

            //�����㾦
            WyrmwindThrust = new (25773)
            {
                OtherCheck = b => JobGauge.FirstmindsFocusCount == 2 && !IsLastAction(true, Stardiver),
            },

            //����
            LifeSurge = new (83) 
            { 
                BuffsProvide = new [] { ObjectStatus.LifeSurge },

                OtherCheck = b => !IsLastAbility(true, LifeSurge),
            },

            //��ǹ
            LanceCharge = new (85),

            //��������
            DragonSight = new (7398)
            {
                ChoiceTarget = Targets =>
                {
                    Targets = Targets.Where(b => b.ObjectId != Service.ClientState.LocalPlayer.ObjectId &&
                    b.StatusList.Select(status => status.StatusId).Intersect(new uint[] { ObjectStatus.Weakness, ObjectStatus.BrinkofDeath }).Count() == 0).ToArray();

                    var targets = TargetFilter.GetJobCategory(Targets, Role.��ս);
                    if (targets.Length > 0) return TargetFilter.RandomObject(targets);

                    targets = TargetFilter.GetJobCategory(Targets, Role.Զ��);
                    if (targets.Length > 0) return TargetFilter.RandomObject(targets);

                    targets = Targets;
                    if (targets.Length > 0) return TargetFilter.RandomObject(targets);

                    return Player;
                },

                BuffsNeed = new [] {ObjectStatus.PowerSurge},

            },

            //ս������
            BattleLitany = new (3557)
            {
                BuffsNeed = new[] { ObjectStatus.PowerSurge },
            };
    }

    private protected override ActionConfiguration CreateConfiguration()
    {
        return base.CreateConfiguration().SetBool("DRG_ShouldDelay", true, "�Ӻ����Ѫ")
            .SetBool("DRG_Opener", false, "88������")
            .SetBool("DRG_SafeMove", true, "��ȫλ��");
    }

    internal override SortedList<DescType, string> Description => new SortedList<DescType, string>()
    {
        {DescType.�ƶ�����, $"{Actions.SpineshatterDive}, {Actions.DragonfireDive}"},
    };

    private protected override bool MoveAbility(byte abilityRemain, out IAction act)
    {
        if (abilityRemain > 1)
        {
            if (Actions.SpineshatterDive.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
            if (Actions.DragonfireDive.ShouldUse(out act, mustUse: true)) return true;
        }

        act = null;
        return false;
    }
    private protected override bool EmergercyAbility(byte abilityRemain, IAction nextGCD, out IAction act)
    {
        if (nextGCD.IsAnySameAction(true, Actions.FullThrust, Actions.CoerthanTorment)
            || Player.HaveStatus(ObjectStatus.LanceCharge) && nextGCD == Actions.FangandClaw)
        {
            //����
            if (abilityRemain ==1 && Actions.LifeSurge.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
        }

        return base.EmergercyAbility(abilityRemain, nextGCD, out act);
    }

    private protected override bool AttackAbility(byte abilityRemain, out IAction act)
    {
        if (SettingBreak)
        {
            //��ǹ
            if (Actions.LanceCharge.ShouldUse(out act, mustUse: true))
            {
                if (abilityRemain == 1 && !Player.HaveStatus(ObjectStatus.PowerSurge)) return true;
                if (Player.HaveStatus(ObjectStatus.PowerSurge)) return true;
            }

            //��������
            if (Actions.DragonSight.ShouldUse(out act, mustUse: true)) return true;

            //ս������
            if (Actions.BattleLitany.ShouldUse(out act, mustUse: true)) return true;
        }

        //����֮��
        if (Actions.Nastrond.ShouldUse(out act, mustUse: true)) return true;

        //׹�ǳ�
        if (Actions.Stardiver.ShouldUse(out act, mustUse: true)) return true;

        //����
        if (Actions.HighJump.EnoughLevel)
        {
            if (Actions.HighJump.ShouldUse(out act)) return true;
        }
        else
        {
            if (Actions.Jump.ShouldUse(out act)) return true;
        }

        //���Խ������Ѫ
        if (Actions.Geirskogul.ShouldUse(out act, mustUse: true)) return true;

        //�����
        if (Actions.SpineshatterDive.ShouldUse(out act, emptyOrSkipCombo: true))
        {
            if (Player.HaveStatus(ObjectStatus.LanceCharge) && Actions.LanceCharge.ElapsedAfterGCD(3)) return true;
        }
        if (Player.HaveStatus(ObjectStatus.PowerSurge) && Actions.SpineshatterDive.ChargesCount != 1 && Actions.SpineshatterDive.ShouldUse(out act)) return true;

        //�����
        if (Actions.MirageDive.ShouldUse(out act)) return true;

        //���׳�
        if (Actions.DragonfireDive.ShouldUse(out act, mustUse: true))
        {
            if (Player.HaveStatus(ObjectStatus.LanceCharge) && Actions.LanceCharge.ElapsedAfterGCD(3)) return true;
        }

        //�����㾦
        if (Actions.WyrmwindThrust.ShouldUse(out act, mustUse: true)) return true;

        return false;
    }

    private protected override bool GeneralGCD(uint lastComboActionID, out IAction act)
    {
        safeMove = Config.GetBoolByName("DRG_SafeMove");

        //if (!InCombat && Config.GetBoolByName("DRG_Opener") && Service.ClientState.LocalPlayer!.Level >= 88)
        //{
        //    inOpener = false;

        //    if (!Actions.LanceCharge.IsCoolDown && !Actions.BattleLitany.IsCoolDown)
        //    {
        //        inOpener = true;
        //    }
        //}
        //if (Actions.BattleLitany.IsCoolDown && !Player.HaveStatus(ObjectStatus.LanceCharge))
        //{
        //    inOpener = false;
        //}
        #region Ⱥ��
        if (Actions.CoerthanTorment.ShouldUse(out act, lastComboActionID)) return true;
        if (Actions.SonicThrust.ShouldUse(out act, lastComboActionID)) return true;
        if (Actions.DoomSpike.ShouldUse(out act, lastComboActionID)) return true;

        #endregion

        #region ����
        if (Config.GetBoolByName("ShouldDelay"))
        {
            if (Actions.WheelingThrust.ShouldUse(out act)) return true;
            if (Actions.FangandClaw.ShouldUse(out act)) return true;
        }
        else
        {
            if (Actions.FangandClaw.ShouldUse(out act)) return true;
            if (Actions.WheelingThrust.ShouldUse(out act)) return true;
        }

        //�����Ƿ���Ҫ��Buff
        if (!Player.WillStatusEndGCD(5, 0, true, ObjectStatus.PowerSurge))
        {
            if (Actions.FullThrust.ShouldUse(out act, lastComboActionID)) return true;
            if (Actions.VorpalThrust.ShouldUse(out act, lastComboActionID)) return true;
            if (Actions.ChaosThrust.ShouldUse(out act, lastComboActionID)) return true;
        }
        else
        {
            if (Actions.Disembowel.ShouldUse(out act, lastComboActionID)) return true;
        }
        if (Actions.TrueThrust.ShouldUse(out act)) return true;

        if (CommandController.Move && MoveAbility(1, out act)) return true;
        if (Actions.PiercingTalon.ShouldUse(out act)) return true;

        return false;

        #endregion
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        //ǣ��
        if (GeneralActions.Feint.ShouldUse(out act)) return true;
        return false;
    }
}
