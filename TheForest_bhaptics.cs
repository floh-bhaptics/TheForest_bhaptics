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
        public static bool isRightHanded = true;
        public static bool planeFallen = false;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        private static KeyValuePair<float, float> getAngleAndShift(Transform player, Vector3 hit)
        {
            // bhaptics starts in the front, then rotates to the left. 0° is front, 90° is left, 270° is right.
            Vector3 patternOrigin = new Vector3(0f, 0f, 1f);
            // y is "up", z is "forward" in local coordinates
            Vector3 hitPosition = hit - player.position;
            //tactsuitVr.LOG("Relative x-z-position: " + hitPosition.x.ToString() + " " + hitPosition.z.ToString());
            Quaternion myPlayerRotation = player.rotation;
            Vector3 playerDir = myPlayerRotation.eulerAngles;
            //tactsuitVr.LOG("PlayerDir: " + playerDir.y.ToString());
            //tactsuitVr.LOG("PlayerRot: " + playerRotation.ToString());
            Vector3 flattenedHit = new Vector3(hitPosition.x, 0f, hitPosition.z);
            float earlyhitAngle = Vector3.Angle(flattenedHit, patternOrigin);
            Vector3 earlycrossProduct = Vector3.Cross(flattenedHit, patternOrigin);
            if (earlycrossProduct.y > 0f) { earlyhitAngle *= -1f; }
            //tactsuitVr.LOG("EarlyHitAngle: " + earlyhitAngle.ToString());
            //float myRotation = earlyhitAngle - playerRotation;
            float myRotation = earlyhitAngle - playerDir.y;
            myRotation *= -1f;
            if (myRotation < 0f) { myRotation = 360f + myRotation; }
            //tactsuitVr.LOG("myHitAngle: " + myRotation.ToString());


            float hitShift = hitPosition.y;
            //tactsuitVr.LOG("HitShift: " + hitShift.ToString());
            float upperBound = -1.0f;
            float lowerBound = -2.0f;
            if (hitShift > upperBound) { hitShift = 0.5f; }
            else if (hitShift < lowerBound) { hitShift = -0.5f; }
            else { hitShift = (hitShift - lowerBound) / (upperBound - lowerBound) - 0.5f; }
            //tactsuitVr.LOG("HitShift: " + hitShift.ToString());
            //tactsuitVr.LOG(" ");

            //tactsuitVr.LOG("Relative x-z-position: " + relativeHitDir.x.ToString() + " "  + relativeHitDir.z.ToString());
            //tactsuitVr.LOG("HitAngle: " + hitAngle.ToString());
            //tactsuitVr.LOG("HitShift: " + hitShift.ToString());

            return new KeyValuePair<float, float>(myRotation, hitShift);
        }

        [HarmonyPatch(typeof(VRPlayerControl), "UpdateWeaponHandedness", new Type[] { })]
        public class bhaptics_UpdateHandedness
        {
            [HarmonyPostfix]
            public static void Postfix(VRPlayerControl __instance)
            {
                isRightHanded = !__instance.RightHandedActive;
                //tactsuitVr.LOG("Right hand: " + isRightHanded.ToString());
            }
        }



        #region Eating / drinking

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

        [HarmonyPatch(typeof(PlayerSfx), "PlayEat", new Type[] { })]
        public class bhaptics_PlayEat
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Eating");
            }
        }

        [HarmonyPatch(typeof(PlayerSfx), "PlayStaminaBreath", new Type[] { })]
        public class bhaptics_PlayStaminaBreath
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Breathing");
            }
        }

        [HarmonyPatch(typeof(PlayerSfx), "PlayColdSfx", new Type[] { })]
        public class bhaptics_PlayCold
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StartShiver();
            }
        }

        [HarmonyPatch(typeof(PlayerSfx), "StopColdSfx", new Type[] { })]
        public class bhaptics_StopCold
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StopShiver();
            }
        }

        
        #endregion

        #region World interaction

        [HarmonyPatch(typeof(WaterViz), "Update", new Type[] {  })]
        public class bhaptics_InWaterChecker
        {
            [HarmonyPostfix]
            public static void Postfix(WaterViz __instance)
            {
                if (!__instance.InWater) { tactsuitVr.StopWater(); return; }
                else { tactsuitVr.StartWater(); }
                tactsuitVr.LOG("Water height: " + __instance.WaterLevel.ToString());
            }
        }

        [HarmonyPatch(typeof(PlayerSfx), "PlayPutOnClothingSfx", new Type[] { })]
        public class bhaptics_PutOnClothing
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.LOG("Put on clothing");
            }
        }

        [HarmonyPatch(typeof(FirstPersonCharacter), "OnCollisionEnterProxied", new Type[] { typeof(Collision) })]
        public class bhaptics_CollisionEnter
        {
            [HarmonyPostfix]
            public static void Postfix(FirstPersonCharacter __instance, Collision coll)
            {
                if (coll.impulse.y >= 100f) { tactsuitVr.PlaybackHaptics("FallDamage"); return; }
                //tactsuitVr.LOG("Collision vector: " + coll.impulse.x.ToString() + " " + coll.impulse.y.ToString() + " " + coll.impulse.z.ToString());
                //tactsuitVr.LOG("Collision impulse: " + coll.impulse.magnitude.ToString());
                //tactsuitVr.LOG("Character rotation: " + __instance.transform.rotation.eulerAngles.y.ToString());
                string colliderName = coll.collider.name;
                string[] unimportant_Colliders = { "ground", "Collision", "Collsion_Cube", "MainTerrain" };
                Transform myPlayer = __instance.transform;
                float intensity = Math.Min(coll.impulse.magnitude / 100f, 1f);
                foreach (ContactPoint point in coll.contacts )
                {
                    Vector3 myHit = point.point - myPlayer.position;
                    if (myHit.y >= 0f) { continue; }
                    if (myHit.y <= -2.0f)
                    {
                        // player bumped into something with their feet.
                        // (fall damage is already done, so) for nonessential stuff, just skip feedback
                        if (unimportant_Colliders.Any(colliderName.Contains))
                        {
                            // tactsuitVr.LOG("Ignored collider: " + colliderName);
                            continue;
                        }
                    }
                    //tactsuitVr.LOG("Contact point: " + point.point.x.ToString() + " " + point.point.y.ToString() + " " + point.point.z.ToString());
                    //tactsuitVr.LOG("Hit point: " + myHit.x.ToString() + " " + myHit.y.ToString() + " " + myHit.z.ToString());
                    var angleShift = getAngleAndShift(myPlayer, point.point);
                    tactsuitVr.PlayBackHit("Impact", angleShift.Key, angleShift.Value, intensity);
                }
                //tactsuitVr.LOG(" ");
            }
        }
        

        #endregion

        #region Take damage

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
                tactsuitVr.LOG("Fell");
                tactsuitVr.PlaybackHaptics("FallDamage");
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "Hit", new Type[] { typeof(int), typeof(bool), typeof(PlayerStats.DamageType) })]
        public class bhaptics_PlayerHit
        {
            [HarmonyPostfix]
            public static void Postfix(int damage, bool ignoreArmor, PlayerStats.DamageType type)
            {
                tactsuitVr.LOG("Damage: " + type.ToString());
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


        [HarmonyPatch(typeof(PlayerStats), "hitFallDown", new Type[] {  })]
        public class bhaptics_HitFallDown
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.LOG("HitFallDown");
                tactsuitVr.PlaybackHaptics("FallDamage");
            }
        }

        #endregion

        #region Melee

        [HarmonyPatch(typeof(TheForest.Player.WeaponBonus), "OnAttack", new Type[] {  })]
        public class bhaptics_axeHitTree
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                //tactsuitVr.Recoil("Blade", isRightHanded);
                tactsuitVr.LOG("OnAttack");
            }
        }

        [HarmonyPatch(typeof(weaponInfo), "OnTriggerEnter", new Type[] { typeof(Collider) })]
        public class bhaptics_TriggerEnter
        {
            [HarmonyPostfix]
            public static void Postfix(weaponInfo __instance, Collider other)
            {
                tactsuitVr.Recoil("Blade", isRightHanded);
                tactsuitVr.LOG("TriggerEnter: " + __instance.weaponSpeed.ToString());
                tactsuitVr.LOG("Collider: " + other.name);
            }
        }

        [HarmonyPatch(typeof(animEventsManager), "PlayWeaponOneshot", new Type[] { typeof(string) })]
        public class bhaptics_PlayWeaponOneshot
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.LOG("Weapon Oneshot");
            }
        }

        #endregion

        #region Die / Knockout

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

        #endregion

        #region Planecrash cutscene

        [HarmonyPatch(typeof(planeCrashHeight), "skipPlaneCrash", new Type[] { })]
        public class bhaptics_PlaneCrashSkipped
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StopHapticFeedback("PlaneFall");
                planeFallen = true;
            }
        }

        [HarmonyPatch(typeof(planeEvents), "fallForward1", new Type[] { })]
        public class bhaptics_PlaneFallForward1
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (planeFallen) return;
                //tactsuitVr.LOG("Fall1");
                tactsuitVr.PlaybackHaptics("PlaneFall");
                planeFallen = true;
            }
        }

        [HarmonyPatch(typeof(planeEvents), "fallForward2", new Type[] { })]
        public class bhaptics_PlaneFallForward2
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                //tactsuitVr.LOG("Fall2");
                tactsuitVr.StopHapticFeedback("PlaneFall");
                tactsuitVr.PlaybackHaptics("PlaneHitGround");
            }
        }

        [HarmonyPatch(typeof(planeEvents), "hitGround", new Type[] { })]
        public class bhaptics_PlaneHitGround
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                //tactsuitVr.LOG("Crash");
                //tactsuitVr.StopHapticFeedback("PlaneFall");
                tactsuitVr.PlaybackHaptics("PlaneHitGround");
            }
        }

        [HarmonyPatch(typeof(planeEvents), "crashStop", new Type[] { })]
        public class bhaptics_PlaneCrashStop
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                //tactsuitVr.LOG("Crash");
                tactsuitVr.StopHapticFeedback("PlaneFall");
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
        #endregion

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

    }
}
