using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gungeon;
using GungeonAPI;
using UnityEngine;
using MonoMod.RuntimeDetour;
using System.Reflection;
namespace An3sMod.GungeonAPI
{
    public static class ModdedNPC
    {
        public static void Add()
        {
            ShrineFactory shrineFactory = new ShrineFactory
            {

                name = "The Modder",
                modID = "An3sMod",
                spritePath = "An3sMod/Resources/ModdedNPC/ModdedNPC_idle_001.png",
                shadowSpritePath = "An3sMod/Resources/EasyNPC/EasyNPC_shadow_001.png",
                acceptText = "YES! modded items are the best!",
                declineText = "No, vanilla items are good too.",
                OnAccept = new Action<PlayerController, GameObject>(ModdedNPC.Accept),
                OnDecline = null,
                CanUse = new Func<PlayerController, GameObject, bool>(ModdedNPC.CanUse),
                offset = new Vector3(27.2f, 55.8f, 51.3f),
                talkPointOffset = new Vector3(0.75f, 1, 0f),
                isToggle = false,
                isBreachShrine = true,
                interactableComponent = typeof(ModdedNPCInteractible)
            };

            GameObject gameObject = shrineFactory.Build();
            gameObject.AddAnimation("idle", "An3sMod/Resources/ModdedNPC/ModdedNPC_idle", 6, NPCBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            gameObject.AddAnimation("talk", "An3sMod/Resources/ModdedNPC/ModdedNPC_talk", 14, NPCBuilder.AnimationType.Talk, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            gameObject.AddAnimation("talk_start", "An3sMod/Resources/ModdedNPC/ModdedNPC_talk", 15, NPCBuilder.AnimationType.Other, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            gameObject.AddAnimation("do_effect", "An3sMod/Resources/ModdedNPC/ModdedNPC_talk", 15, NPCBuilder.AnimationType.Other, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            ModdedNPCInteractible component = gameObject.GetComponent<ModdedNPCInteractible>();
            component.conversation = new List<string>
            {
                "Hey There!",
                "Do you like modded items?",
                "Because I love modded items!",
                "don't you wish you got more of them?",
                "what do you say?"
            };
            gameObject.SetActive(false);
        }

        private static bool CanUse(PlayerController player, GameObject npc)
        {
            return player != storedPlayer;
        }
        private static PlayerController storedPlayer;
        public static void Accept(PlayerController player, GameObject npc)
        {
           
            npc.GetComponent<tk2dSpriteAnimator>().PlayForDuration("do_effect", 2f, "idle", false);
            string header = "Modded Only Mode Enabled";
            string text = "";
            Notify(header, text);
            storedPlayer = player;
            MakeItemsModded makeItemsModded = new MakeItemsModded();
            makeItemsModded.Init();
        }

   

        private static void Notify(string header, string text)
        {
            tk2dSpriteCollectionData encounterIconCollection = AmmonomiconController.Instance.EncounterIconCollection;
            int spriteIdByName = encounterIconCollection.GetSpriteIdByName("NevernamedsItems/Resources/EasyNpc/EasyMode_icon");
            GameUIRoot.Instance.notificationController.DoCustomNotification(header, text, null, spriteIdByName, UINotificationController.NotificationColor.PURPLE, false, true);
        }

       
    }
}
