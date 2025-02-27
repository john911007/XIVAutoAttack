﻿using System.Linq;
using XIVAutoAttack.Actions;
using XIVAutoAttack.Data;
using XIVAutoAttack.SigReplacers;

namespace XIVAutoAttack.Helpers
{
    internal static class IActionHelper
    {
        internal static bool IsLastSpell(bool isAdjust, params IAction[] actions)
        {
            return IsLastSpell(GetIDFromActions(isAdjust, actions));
        }
        internal static bool IsLastSpell(params ActionID[] ids)
        {
            return IsActionID(Watcher.LastSpell, ids);
        }


        internal static bool IsLastAbility(bool isAdjust, params IAction[] actions)
        {
            return IsLastAbility(GetIDFromActions(isAdjust, actions));
        }
        internal static bool IsLastAbility(params ActionID[] ids)
        {
            return IsActionID(Watcher.LastAbility, ids);
        }

        internal static bool IsLastWeaponSkill(bool isAdjust, params IAction[] actions)
        {
            return IsLastWeaponSkill(GetIDFromActions(isAdjust, actions));
        }
        internal static bool IsLastWeaponSkill(params ActionID[] ids)
        {
            return IsActionID(Watcher.LastWeaponskill, ids);
        }

        internal static bool IsLastAction(bool isAdjust, params IAction[] actions)
        {
            return IsLastAction(GetIDFromActions(isAdjust, actions));
        }
        internal static bool IsLastAction(params ActionID[] ids)
        {
            return IsActionID(Watcher.LastAction, ids);
        }

        internal static bool IsAnySameAction(this IAction action, bool isAdjust, params IAction[] actions)
        {
            return IsActionID(isAdjust ? (ActionID)action.AdjustedID : (ActionID)action.ID, GetIDFromActions(isAdjust, actions));
        }

        private static bool IsActionID(ActionID id, params ActionID[] ids)
        {
            foreach (var i in ids)
            {
                if (i == id) return true;
            }
            return false;
        }

        private static ActionID[] GetIDFromActions(bool isAdjust, params IAction[] actions)
        {
            return actions.Select(a => isAdjust ? (ActionID)a.AdjustedID : (ActionID)a.ID).ToArray();
        }
    }
}
