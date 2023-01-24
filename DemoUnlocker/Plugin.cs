﻿using BepInEx;
using HarmonyLib;

namespace DemoUnlocker
{
    [BepInPlugin("akarnokd.planbterraformmods.demounlocker", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSteam), nameof(SSteam.Init))]
        static void SSteam_Init()
        {
            GGame.isDemo = false;
        }
    }
}
