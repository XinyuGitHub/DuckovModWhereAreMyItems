using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace WhereAreMyItems
{
    internal class HarmonyUtils
    {
        public static event Action s_OnPlayerStorageDestroy;

        private static Harmony s_harmonyInstance = null;
        private static bool s_prepared = false;

        public static void s_Prepare()
        {
            if (s_prepared)
                return;

            s_harmonyInstance = new Harmony("DisplayRequiredItemCount");
            s_harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            s_prepared = true;
            Debug.Log($"[WhereAreMyItems] HarmonyUtils is Prepared");
        }

        [HarmonyPatch(typeof(PlayerStorage), "OnDestroy")]
        public class PlayerStorageOnDestroyPatch
        {
            public static void Prefix(PlayerStorage __instance)
            {
                s_OnPlayerStorageDestroy?.Invoke();
            }
        }
    }
}