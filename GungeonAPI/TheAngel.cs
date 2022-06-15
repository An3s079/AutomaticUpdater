using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using An3sMod;
using Gungeon;
using GungeonAPI;
using UnityEngine;
namespace An3sMod
{
     public static class TheAngel
    {
        public static void Add()
        {
            ShrineFactory shrineFactory = new ShrineFactory
            {

                name = "The Angel",
                modID = "An3sMod",             
                spritePath = "An3sMod/Resources/EasyNPC/EasyNPC_idle_001.png",
                shadowSpritePath = "An3sMod/Resources/EasyNPC/EasyNPC_shadow_001.png",
                acceptText = "Yes Please!",
                declineText = "No, I like the difficulty.",
                OnAccept = new Action<PlayerController, GameObject>(TheAngel.Accept),
                OnDecline = null,
                CanUse = new Func<PlayerController, GameObject, bool>(TheAngel.CanUse),
                offset = new Vector3(43.2f, 56.8f, 51.3f),
                talkPointOffset = new Vector3(1.5f, 3, 0f),
                isToggle = false,
                isBreachShrine = true,
                interactableComponent = typeof(TheAngelInteractible)
            };
            
            GameObject gameObject = shrineFactory.Build();
            gameObject.AddAnimation("idle", "An3sMod/Resources/EasyNPC/EasyNPC_idle", 4, NPCBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            gameObject.AddAnimation("talk", "An3sMod/Resources/EasyNPC/EasyNPC_talk", 12, NPCBuilder.AnimationType.Talk, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            gameObject.AddAnimation("talk_start", "An3sMod/Resources/EasyNPC/EasyNPC_talk", 12, NPCBuilder.AnimationType.Other, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            gameObject.AddAnimation("do_effect", "An3sMod/Resources/EasyNPC/EasyNPC_talk", 12, NPCBuilder.AnimationType.Other, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            TheAngelInteractible component = gameObject.GetComponent<TheAngelInteractible>();
            component.conversation = new List<string>
            {
                "hello,",
                "you poor soul.",
                "dying countless times in this gungeon...",
                "allow me to make it easier.",     
            };
            gameObject.SetActive(false);
        }

        private static bool CanUse(PlayerController player, GameObject npc)
        {
            return player != storedPlayer;
            //HardModeItem.hardmodeEnabled
        }

        private static PlayerController storedPlayer;
        public static void Accept(PlayerController player, GameObject npc)
        {
            npc.GetComponent<tk2dSpriteAnimator>().PlayForDuration("do_effect", 2f, "idle", false);
            TheAngel.ApplyStats(player);
            string header = "Easy Mode Enabled";
            string text = "";
            Notify(header, text);
            storedPlayer = player;
        }

        private static void ApplyStats(PlayerController player)
        {
            ApplyStat(player, PlayerStats.StatType.Damage, 1.3f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ApplyStat(player, PlayerStats.StatType.EnemyProjectileSpeedMultiplier, -0.2f, StatModifier.ModifyMethod.ADDITIVE);
            ApplyStat(player, PlayerStats.StatType.AdditionalBlanksPerFloor, 1f, StatModifier.ModifyMethod.ADDITIVE);
            ApplyStat(player, PlayerStats.StatType.Coolness, 3f, StatModifier.ModifyMethod.ADDITIVE);
        }
       
        private static void Notify(string header, string text)
        {
            tk2dSpriteCollectionData encounterIconCollection = AmmonomiconController.Instance.EncounterIconCollection;
            int spriteIdByName = encounterIconCollection.GetSpriteIdByName("notreal/Resources/EasyNpc/EasyMode_icon");
            GameUIRoot.Instance.notificationController.DoCustomNotification(header, text, null, spriteIdByName, UINotificationController.NotificationColor.GOLD, false, true);
        }

        private static void ApplyStat(PlayerController player, PlayerStats.StatType statType, float amountToApply, StatModifier.ModifyMethod modifyMethod)
        {
            player.stats.RecalculateStats(player, false, false);
            StatModifier statModifier = new StatModifier()
            {
                statToBoost = statType,
                amount = amountToApply,
                modifyType = modifyMethod
            };
            player.ownerlessStatModifiers.Add(statModifier);
            player.stats.RecalculateStats(player, false, false);
        }
    }
}

