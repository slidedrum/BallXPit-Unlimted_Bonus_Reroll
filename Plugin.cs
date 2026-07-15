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
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} V1.0.3 is loaded!");
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
            private static GameObject originalPassive;
            private static GameObject NewRerollPassiveButtonGobject;
            private static TextMeshProUGUI NewRerollPassiveButtonText;
            private static CoolButton NewRerollPassiveButton;
            private static GameObject originalBall;
            private static GameObject NewRerollBallButtonGobject;
            private static TextMeshProUGUI NewRerollBallButtonText;
            private static CoolButton NewRerollBallButton;

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
                Plugin.Log.LogInfo($"Level up UI actiaveted with: {t}");
                if (t == LevelUpType.kBonusPassive)
                {
                    if (NewRerollPassiveButton == null)
                    {
                        Plugin.Log.LogInfo("Creating passive reroll button");
                        originalPassive = __instance.BtnTreasureContinue.gameObject;
                        NewRerollPassiveButtonGobject = UnityEngine.Object.Instantiate(originalPassive, originalPassive.transform.parent);
                        NewRerollPassiveButtonText = NewRerollPassiveButtonGobject.GetComponentInChildren<TextMeshProUGUI>();
                        NewRerollPassiveButton = NewRerollPassiveButtonGobject.GetComponent<CoolButton>();
                        NewRerollPassiveButtonGobject.transform.position += new Vector3(-1.8f, 0, 0);
                        NewRerollPassiveButtonText.SetText("Reroll");
                        NewRerollPassiveButton.OnClicked = __instance.BtnReroll.OnClicked;
                    }
                    else
                    {
                        Plugin.Log.LogInfo("Activating passive reroll button");
                        NewRerollPassiveButtonGobject.SetActive(true);
                    }

                }
                else if (t == LevelUpType.kBonusBall)
                {
                    if (NewRerollBallButton == null)
                    {
                        Plugin.Log.LogInfo("Creating ball reroll button");
                        originalBall = __instance.BtnTreasureContinue.gameObject;
                        NewRerollBallButtonGobject = UnityEngine.Object.Instantiate(originalBall, originalBall.transform.parent);
                        NewRerollBallButtonText = NewRerollBallButtonGobject.GetComponentInChildren<TextMeshProUGUI>();
                        NewRerollBallButton = NewRerollBallButtonGobject.GetComponent<CoolButton>();
                        NewRerollBallButtonGobject.transform.position += new Vector3(-1.8f, 0, 0);
                        NewRerollBallButtonText.SetText("Reroll");
                        NewRerollBallButton.OnClicked = __instance.BtnReroll.OnClicked;
                    }
                    else
                    {
                        Plugin.Log.LogInfo("Activating ball reroll button");
                        NewRerollBallButtonGobject.SetActive(true);
                    }
                }
                else
                {
                    if (t != LevelUpType.kBonusPassive && NewRerollPassiveButton != null && NewRerollPassiveButtonGobject.activeInHierarchy)
                    {
                        Plugin.Log.LogInfo("Dissabling passive reroll button");
                        NewRerollPassiveButtonGobject.SetActive(false);
                    }
                    if (t != LevelUpType.kBonusBall && NewRerollBallButton != null && NewRerollBallButtonGobject.activeInHierarchy)
                    {
                        Plugin.Log.LogInfo("Dissabling ball reroll button");
                        NewRerollBallButtonGobject.SetActive(false);
                    } 
                }
            }
        }

    }
}
