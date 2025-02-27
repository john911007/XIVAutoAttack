using System.Collections.Generic;
using XIVAutoAttack.Actions;
using XIVAutoAttack.Combos.Basic;
using XIVAutoAttack.Combos.CustomCombo;
using XIVAutoAttack.Configuration;
using XIVAutoAttack.Helpers;
using static XIVAutoAttack.Combos.RangedMagicial.SMNCombos.SMNCombo_Default;

namespace XIVAutoAttack.Combos.RangedMagicial.SMNCombos;

internal sealed class SMNCombo_Default : SMNCombo_Base<CommandType>
{
    public override string Author => "��ˮ";

    internal enum CommandType : byte
    {
        None,
    }

    protected override SortedList<CommandType, string> CommandDescription => new SortedList<CommandType, string>()
    {
        //{CommandType.None, "" }, //д��ע�Ͱ���������ʾ�û��ġ�
    };
    protected override bool CanHealSingleSpell => false;

    public override SortedList<DescType, string> DescriptionDict => new()
    {
        {DescType.�������, $"{RadiantAegis}"},
        {DescType.��������, $"{Physick}"},
    };

    private protected override bool MoveGCD(out IAction act)
    {
        if (CrimsonCyclone.ShouldUse(out act, mustUse: true)) return true;
        return base.MoveGCD(out act);
    }

    private protected override bool GeneralGCD(out IAction act)
    {
        //��ʯ���ٻ�
        if (SummonCarbuncle.ShouldUse(out act)) return true;

        //����
        if (!InBahamut && !InPhoenix)
        {
            if (RuinIV.ShouldUse(out act, mustUse: true)) return true;
            if (CrimsonStrike.ShouldUse(out act, mustUse: true)) return true;
            if (CrimsonCyclone.ShouldUse(out act, mustUse: true))
            {
                if (CrimsonCyclone.Target.DistanceToPlayer() < 2)
                {
                    return true;
                }
            }
            if (Slipstream.ShouldUse(out act, mustUse: true)) return true;
        }


        //�ٻ�
        if (Attunement == 0)
        {
            if (SummonBahamut.ShouldUse(out act))
            {
                if (SearingLight.IsCoolDown || !SearingLight.EnoughLevel)
                    return true;
            }
            else if (Aethercharge.ShouldUse(out act)) return true;

            if (AllReady ? SummonTimeEndAfterGCD() : true)
            {
                switch (Config.GetComboByName("SummonOrder"))
                {
                    default:
                        //�� ��
                        if (SummonRuby.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonTopaz.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonEmerald.ShouldUse(out act)) return true;
                        break;
                    case 1:
                        //�� ��
                        if (SummonRuby.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonEmerald.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonTopaz.ShouldUse(out act)) return true;
                        break;
                    case 2:
                        //�� ��
                        if (SummonTopaz.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonEmerald.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonRuby.ShouldUse(out act)) return true;
                        break;
                    case 3:
                        //�� ��
                        if (SummonTopaz.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonRuby.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonEmerald.ShouldUse(out act)) return true;
                        break;
                    case 4:
                        //�� ��
                        if (SummonEmerald.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonRuby.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonTopaz.ShouldUse(out act)) return true;
                        break;
                    case 5:
                        //�� ��
                        if (SummonEmerald.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonTopaz.ShouldUse(out act)) return true;
                        //�� ��
                        if (SummonRuby.ShouldUse(out act)) return true;
                        break;
                }
            }
        }

        //AOE
        if (PreciousBrilliance.ShouldUse(out act)) return true;
        if (Outburst.ShouldUse(out act)) return true;

        //����
        if (Gemshine.ShouldUse(out act)) return true;
        if (Ruin.ShouldUse(out act)) return true;
        return false;
    }
    private protected override ActionConfiguration CreateConfiguration()
    {
        return base.CreateConfiguration().SetCombo("SummonOrder", 0, "�����ٻ�˳��",
            "��-��-��", "��-��-��", "��-��-��", "��-��-��", "��-��-��", "��-��-��");
    }
    private protected override bool AttackAbility(byte abilityRemain, out IAction act)
    {
        if (SettingBreak)
        {
            //����֮��
            if (SearingLight.ShouldUse(out act, mustUse: true)) return true;
        }

        if (EnkindleBahamut.ShouldUse(out act, mustUse: true)) return true;
        if (Deathflare.ShouldUse(out act, mustUse: true)) return true;
        if (Rekindle.ShouldUse(out act, mustUse: true)) return true;
        if (MountainBuster.ShouldUse(out act, mustUse: true)) return true;


        //��������
        if (HasAetherflowStacks && InBreak)
        {
            if (Painflare.ShouldUse(out act)) return true;
            if (Fester.ShouldUse(out act)) return true;
        }
        else
        {
            if (EnergySiphon.ShouldUse(out act)) return true;
            if (EnergyDrain.ShouldUse(out act)) return true;
        }

        return false;
    }

    private protected override bool DefenceSingleAbility(byte abilityRemain, out IAction act)
    {
        //�ػ�֮��
        if (RadiantAegis.ShouldUse(out act)) return true;

        return false;
    }

    private protected override bool HealSingleGCD(out IAction act)
    {
        //ҽ��
        if (Physick.ShouldUse(out act)) return true;

        return false;
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        //����
        if (Addle.ShouldUse(out act)) return true;
        return false;
    }
}
