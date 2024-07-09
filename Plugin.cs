using BepInEx;
using BepInEx.Configuration;
using BoplFixedMath;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace HeartAttack
{
    [BepInPlugin("com.PizzaMan730.HeartAttack", "HeartAttack", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {

        public static ConfigFile config;

		public static ConfigEntry<float> chance;


        private void Awake()
        {
            Logger.LogInfo("HeartAttack has loaded!");

            Plugin.config = base.Config;
			Plugin.chance = Plugin.config.Bind<float>("HeartAttack", "Heart attack chance", 1f, "Percent chance that you die each tick");

            Harmony harmony = new Harmony("com.PizzaMan730.HeartAttack");


            MethodInfo original = AccessTools.Method(typeof(PlayerBody), "UpdateSim");
            MethodInfo patch = AccessTools.Method(typeof(myPatches), "UpdateSim_Patch");
            harmony.Patch(original, new HarmonyMethod(patch));


        }

        

        public class myPatches
        {
            public static void UpdateSim_Patch(PlayerBody __instance, ref IPlayerIdHolder ___idHolder, ref FixTransform ___fixTransform)
            {
                if (Updater.RandomFix((Fix)0, (Fix)100) <= (Fix)(Plugin.chance.Value))
                {
                    FieldInfo selfRefField = typeof(GameSessionHandler).GetField("selfRef", BindingFlags.Static | BindingFlags.NonPublic);
                    FieldInfo slimeControllersField = typeof(GameSessionHandler).GetField("slimeControllers", BindingFlags.Instance | BindingFlags.NonPublic);
                    SlimeController[] slimeControllers = (SlimeController[])slimeControllersField.GetValue(selfRefField.GetValue(null));
                    
                    foreach (SlimeController controller in slimeControllers)
                    {
                        if (controller.playerNumber == ___idHolder.GetPlayerId()) 
                        {
                            
                            ((PlayerCollision)Traverse.Create(controller).Field("playerCollision").GetValue()).killPlayer(___idHolder.GetPlayerId(), true, true, CauseOfDeath.Other);
                            Updater.DestroyFix(___fixTransform.gameObject);
                            Debug.Log("A player had a heart attack!");
                        }
                    }
                }
            }
        }
    }
}
//      dotnet build "C:\Users\ajarc\BoplMods\HeartAttack\HeartAttack.csproj"