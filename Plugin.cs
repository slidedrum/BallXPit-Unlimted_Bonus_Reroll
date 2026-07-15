using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

namespace Inf_Reroll
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
    }
    namespace Inf_Reroll
    {
        [HarmonyPatch(typeof(LevelUpUI))]
        internal static class LevelUpUIPatches
        {
            private static int? savedRerollCount = null;
            private static GameObject original;
            private static GameObject NewRerollButtonGobject;
            private static TextMeshProUGUI NewRerollButtonText;
            private static CoolButton NewRerollButton;

            [HarmonyPrefix]
            [HarmonyPatch(nameof(LevelUpUI.OnRerollClicked))]
            private static void OnRerollClickedPrefix(LevelUpUI __instance)
            {
                if (BattleSaveData.I.ElapsedTime != 0)
                    return;
                savedRerollCount = BattleSaveData.I.NumFreeRerolls;
                BattleSaveData.I.NumFreeRerolls = Math.Max(1, BattleSaveData.I.NumFreeRerolls);
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(LevelUpUI.OnRerollClicked))]
            private static void OnRerollClickedPostfix(LevelUpUI __instance)
            {
                if (BattleSaveData.I.ElapsedTime != 0)
                    return;
                if (savedRerollCount == null)
                    throw new Exception("Null rerolls");
                BattleSaveData.I.NumFreeRerolls = (int)savedRerollCount;
                savedRerollCount = null;
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(LevelUpUI.Activate))]
            private static void ActivatePostfix(LevelUpUI __instance, LevelUpType t)
            {

                if (t != LevelUpType.kBonusPassive)
                {
                    if (NewRerollButton != null)
                        NewRerollButtonGobject.SetActive(false);
                    return;
                }
                if (NewRerollButton != null)
                    return;
                original = __instance.BtnTreasureContinue.gameObject;
                NewRerollButtonGobject = UnityEngine.Object.Instantiate(original, original.transform.parent);
                NewRerollButtonText = NewRerollButtonGobject.GetComponentInChildren<TextMeshProUGUI>();
                NewRerollButton = NewRerollButtonGobject.GetComponent<CoolButton>();
                NewRerollButtonGobject.transform.position += new Vector3(-1.8f, 0 ,0 );
                NewRerollButtonText.SetText("Reroll");
                NewRerollButton.OnClicked = __instance.BtnReroll.OnClicked;
            }
        }

    }
}
