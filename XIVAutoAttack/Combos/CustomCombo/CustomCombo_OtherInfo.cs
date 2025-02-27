﻿using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.ComponentModel;
using System.Reflection;
using XIVAutoAttack.Actions;
using XIVAutoAttack.Data;
using XIVAutoAttack.Helpers;
using XIVAutoAttack.SigReplacers;
using XIVAutoAttack.Updaters;
using static XIVAutoAttack.SigReplacers.Watcher;

namespace XIVAutoAttack.Combos.CustomCombo
{
    internal abstract partial class CustomCombo<TCmd> where TCmd : Enum
    {
        /// <summary>
        /// 玩家
        /// </summary>
        protected static PlayerCharacter Player => Service.ClientState.LocalPlayer;
        /// <summary>
        /// 目标
        /// </summary>
        protected static BattleChara Target => Service.TargetManager.Target is BattleChara b ? b : Player;

        /// <summary>
        /// 是否正在移动
        /// </summary>
        [DisplayName("在移动")]
        [Description("玩家正在位移")]
        protected static bool IsMoving => MovingUpdater.IsMoving;
        /// <summary>
        /// 攻击范围内是否有敌人
        /// </summary>
        [DisplayName("范围内有敌人")]
        [Description("近战3米内有敌人，远程25米内有敌人。")]
        protected static bool HaveHostilesInRange => TargetUpdater.HaveHostilesInRange;

        private bool _canUseHealAction => Job.GetJobRole() == JobRole.Healer || Service.Configuration.UseHealWhenNotAHealer;

        /// <summary>
        /// 是否可以使用范围治疗能力技
        /// </summary>
        protected virtual bool CanHealAreaAbility => TargetUpdater.CanHealAreaAbility && _canUseHealAction;
        /// <summary>
        /// 是否可以使用范围治疗魔法
        /// </summary>
        protected virtual bool CanHealAreaSpell => TargetUpdater.CanHealAreaSpell && _canUseHealAction;

        /// <summary>
        /// 是否可以使用单体治疗能力技
        /// </summary>
        protected virtual bool CanHealSingleAbility => TargetUpdater.CanHealSingleAbility && _canUseHealAction;
        /// <summary>
        /// 是否可以使用单体治疗魔法
        /// </summary>
        protected virtual bool CanHealSingleSpell => TargetUpdater.CanHealSingleSpell && _canUseHealAction;

        /// <summary>
        /// 满编小队
        /// </summary>
        [DisplayName("满编小队")]
        protected static bool IsFullParty => TargetUpdater.PartyMembers.Length is 8;

        /// <summary>
        /// 是否处于爆发，自动爆发或者手动点击的爆发
        /// </summary>
        [DisplayName("处于爆发")]
        protected static bool SettingBreak => CommandController.BreakorProvoke || Service.Configuration.AutoBreak;

        /// <summary>
        /// 当前等级
        /// </summary>
        [DisplayName("玩家等级")]
        protected static byte Level => Player?.Level ?? 0;
        /// <summary>
        /// 是否在战斗中
        /// </summary>
        [DisplayName("在战斗中")]
        protected static bool InCombat => ActionUpdater.InCombat;

        /// <summary>
        /// 成功释放的技能，其中第0个为最近成功释放的技能，也就是上一个技能。该列表最大记录64个
        /// </summary>
        protected static ActionRec[] RecordActions => Watcher.RecordActions;

        /// <summary>
        /// 距离上一个技能释放完后过了多少时间。
        /// </summary>
        protected static TimeSpan TimeSinceLastAction => Watcher.TimeSinceLastAction;

        /// <summary>
        /// 上一个魔法是否是<paramref name="actions"/>中的技能
        /// </summary>
        /// <param name="isAdjust">调整后ID</param>
        /// <param name="actions">技能</param>
        /// <returns></returns>
        [DisplayName("上一个魔法")]
        protected static bool IsLastSpell(bool isAdjust, params IAction[] actions)
            => IActionHelper.IsLastSpell(isAdjust, actions);

        /// <summary>
        /// 上一个魔法是否是<paramref name="ids"/>中的技能
        /// </summary>
        /// <param name="ids">技能ID</param>
        /// <returns></returns>
        protected static bool IsLastSpell(params ActionID[] ids)
            => IActionHelper.IsLastSpell(ids);

        /// <summary>
        /// 上一个能力技是否是<paramref name="actions"/>中的技能
        /// </summary>
        /// <param name="isAdjust">调整后ID</param>
        /// <param name="actions">技能</param>
        /// <returns></returns>
        [DisplayName("上一个能力技")]
        protected static bool IsLastAbility(bool isAdjust, params IAction[] actions)
            => IActionHelper.IsLastAbility(isAdjust, actions);

        /// <summary>
        /// 上一个能力技是否是<paramref name="ids"/>中的技能
        /// </summary>
        /// <param name="ids">技能ID</param>
        /// <returns></returns>
        protected static bool IsLastAbility(params ActionID[] ids)
            => IActionHelper.IsLastAbility(ids);

        /// <summary>
        /// 上一个战技是否是<paramref name="actions"/>中的技能
        /// </summary>
        /// <param name="isAdjust">调整后ID</param>
        /// <param name="actions">技能</param>
        /// <returns></returns>
        [DisplayName("上一个战技")]
        protected static bool IsLastWeaponSkill(bool isAdjust, params IAction[] actions)
            => IActionHelper.IsLastWeaponSkill(isAdjust, actions);

        /// <summary>
        /// 上一个战技是否是<paramref name="ids"/>中的技能
        /// </summary>
        /// <param name="ids">技能ID</param>
        /// <returns></returns>
        protected static bool IsLastWeaponSkill(params ActionID[] ids)
            => IActionHelper.IsLastWeaponSkill(ids);

        /// <summary>
        /// 上一个技能是否是<paramref name="actions"/>中的技能
        /// </summary>
        /// <param name="isAdjust">调整后ID</param>
        /// <param name="actions">技能</param>
        /// <returns></returns>
        [DisplayName("上一个技能")]
        protected static bool IsLastAction(bool isAdjust, params IAction[] actions)
            => IActionHelper.IsLastAction(isAdjust, actions);

        /// <summary>
        /// 上一个技能是否是<paramref name="ids"/>中的技能
        /// </summary>
        /// <param name="ids">技能ID</param>
        /// <returns></returns>
        protected static bool IsLastAction(params ActionID[] ids)
            => IActionHelper.IsLastAction(ids);

        /// <summary>
        /// 距离下几个GCD转好这个职业量谱或其他的剩余时间能结束吗。
        /// </summary>
        /// <param name="remain">剩余时间(职业量谱等的剩余时间,单位必须是秒)</param>
        /// <param name="gcdCount">要隔着多少个完整的GCD</param>
        /// <param name="abilityCount">再多少个能力技之后</param>
        /// <param name="addWeaponRemain">是否要把<see cref="ActionUpdater.WeaponRemain"/>加进去</param>
        /// <returns>这个时间点是否已经结束</returns>
        protected static bool EndAfterGCD(float remain, uint gcdCount = 0, uint abilityCount = 0)
            => CooldownHelper.RecastAfterGCD(remain, gcdCount, abilityCount);

        /// <summary>
        /// 几秒后这个职业量谱或其他的剩余时间能结束吗.
        /// </summary>
        /// <param name="remain">剩余时间(职业量谱等的剩余时间,单位必须是秒)</param>
        /// <param name="remainNeed">需要多少秒</param>
        /// <returns>这个时间点是否已经结束,即(<paramref name="remain"/> 小于等于 <paramref name="remainNeed"/>)</returns>
        protected static bool EndAfter(float remain, float remainNeed)
            => CooldownHelper.RecastAfter(remain, remainNeed);

        public MethodInfo[] AllLast => GetType().GetStaticBoolMethodInfo(m =>
        {
            var types = m.GetParameters();
            return types.Length == 2 
                && types[0].ParameterType == typeof(bool) 
                && types[1].ParameterType == typeof(IAction[]);
        });
    }
}
