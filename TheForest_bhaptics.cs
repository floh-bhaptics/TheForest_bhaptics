using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MelonLoader;
using HarmonyLib;
using UnityEngine;

using MyBhapticsTactsuit;

namespace TheForest_bhaptics
{
    public class TheForest_bhaptics : MelonMod
    {
        public static TactsuitVR tactsuitVr;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        
        [HarmonyPatch(typeof(PlayerStats), "Drink", new Type[] { })]
        public class bhaptics_PlayerDrinks
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Drinking");
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "DrinkBooze", new Type[] { })]
        public class bhaptics_PlayerDrinksBooze
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Drinking");
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "DrinkLake", new Type[] { })]
        public class bhaptics_PlayerDrinksLake
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Drinking");
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "Explosion", new Type[] { typeof(float) })]
        public class bhaptics_Explosion
        {
            [HarmonyPostfix]
            public static void Postfix(float getDist)
            {
                float intensity = Math.Max((50f - getDist) / 50f, 0.0f);
                tactsuitVr.PlaybackHaptics("ExplosionBelly", intensity);
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "Explosion", new Type[] { typeof(float), typeof(bool) })]
        public class bhaptics_ExplosionTwo
        {
            [HarmonyPostfix]
            public static void Postfix(float dist, bool fromPlayer)
            {
                float intensity = Math.Max((50f - dist) / 50f, 0.0f);
                tactsuitVr.PlaybackHaptics("ExplosionBelly", intensity);
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "ExplosionPlayer", new Type[] { typeof(float) })]
        public class bhaptics_ExplosionPlayer
        {
            [HarmonyPostfix]
            public static void Postfix(float hitDist)
            {
                float intensity = Math.Max((50f - hitDist) / 50f, 0.0f);
                tactsuitVr.PlaybackHaptics("ExplosionBelly", intensity);
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "Fell", new Type[] {  })]
        public class bhaptics_PlayerFell
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("FallDamage");
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "HealthChange", new Type[] { typeof(float) })]
        public class bhaptics_HealthChange
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerStats __instance, float amount)
            {
                if (__instance.IsHealthInGreyZone) { tactsuitVr.StartHeartBeat(); }
                else { tactsuitVr.StopHeartBeat(); }
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "Hit", new Type[] { typeof(int), typeof(bool), typeof(PlayerStats.DamageType) })]
        public class bhaptics_PlayerHit
        {
            [HarmonyPostfix]
            public static void Postfix(int damage, bool ignoreArmor, PlayerStats.DamageType type)
            {
                switch (type)
                {
                    case PlayerStats.DamageType.Drowning:
                        if (!tactsuitVr.IsPlaying("Choking"))
                            tactsuitVr.PlaybackHaptics("Choking");
                        break;
                    case PlayerStats.DamageType.Fire:
                        if (!tactsuitVr.IsPlaying("Burning"))
                            tactsuitVr.PlaybackHaptics("Burning");
                        break;
                    case PlayerStats.DamageType.Frost:
                        if (!tactsuitVr.IsPlaying("Freezing"))
                            tactsuitVr.PlaybackHaptics("Freezing");
                        break;
                    case PlayerStats.DamageType.Poison:
                        if (!tactsuitVr.IsPlaying("Poison"))
                            tactsuitVr.PlaybackHaptics("Poison");
                        break;
                    case PlayerStats.DamageType.Physical:
                        tactsuitVr.PlaybackHaptics("Impact");
                        break;
                    default:
                        break;
                }
            }
        }

        
        [HarmonyPatch(typeof(PlayerStats), "getHitDirection", new Type[] { typeof(Vector3) })]
        public class bhaptics_getHitDirection
        {
            [HarmonyPostfix]
            public static void Postfix(Vector3 pos)
            {
                tactsuitVr.LOG("Hit direction: " + pos.x.ToString() + " " + pos.z.ToString());
            }
        }

        [HarmonyPatch(typeof(planeEvents), "fallForward1", new Type[] {  })]
        public class bhaptics_PlaneFallForward1
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("PlaneFall");
            }
        }

        [HarmonyPatch(typeof(planeEvents), "fallForward2", new Type[] { })]
        public class bhaptics_PlaneFallForward2
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("PlaneFall");
            }
        }

        [HarmonyPatch(typeof(planeEvents), "hitGround", new Type[] { })]
        public class bhaptics_PlaneHitGround
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("PlaneHitGround");
            }
        }

        [HarmonyPatch(typeof(planeEvents), "goBlack", new Type[] { })]
        public class bhaptics_PlaneGoBlack
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("HeartBeat");
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "hitFallDown", new Type[] {  })]
        public class bhaptics_HitFallDown
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("FallDamage");
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "KillPlayer", new Type[] {  })]
        public class bhaptics_KillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StopThreads();
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "KnockOut", new Type[] { })]
        public class bhaptics_KnockOut
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StopThreads();
                tactsuitVr.PlaybackHaptics("HeartBeat");
            }
        }
        
        [HarmonyPatch(typeof(PlayerStats), "WakeFromKnockOut", new Type[] { typeof(bool), typeof(WaitForSeconds) })]
        public class bhaptics_WakeFromKnockOut
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("NeckTingleShort");
            }
        }
        
    }
}
