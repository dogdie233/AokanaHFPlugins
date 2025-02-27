﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

namespace SecondaryLanguageSubtitle.Patches;

[HarmonyPatch]
public class UIBacklogPatch
{
    [HarmonyPatch(typeof(UIBacklog), "RefreshUI")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileUIBacklogRefreshUI(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var hijack = AccessTools.Method(typeof(UIBacklogPatch), nameof(SplitLangStr));
        foreach (var instruction in instructions)
        {
            if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) && instruction.operand is MethodInfo methodInfo && methodInfo.ToString() == hijack.ToString())
            {
                yield return new CodeInstruction(OpCodes.Call, hijack);
                continue;
            }
            yield return instruction;
        }
        SecondaryLanguage.MyLogger.LogDebug($"Hijacking method {original.Name} in {original.DeclaringType?.Name}");
    }

    private static void SplitLangStr(string n, string t, out string usename, out string uestext)
    {
        usename = Utils.SplitLangStr(n, EngineMain.lang);
        if (t.Contains('\u2402'))
        {
            var primary = Utils.SplitLangStr(t, EngineMain.lang);
            var secondary = Utils.SplitLangStr(t, SecondaryLanguage.SecLang);
            uestext = $"<size=20>{primary}</size>\n<size=16>{secondary}</size>";
            return;
        }
        uestext = t;
    }
}