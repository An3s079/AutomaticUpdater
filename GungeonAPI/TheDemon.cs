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
     public static class TheDemon
    {
        public static void Add()
        {
            ShrineFactory shrineFactory = new ShrineFactory
            {

                name = "The Demon",
                modID = "An3sMod",             
                spritePath = "An3sMod/Resources/EasyNPC/EasyNPC_idle_001.png",
                shadowSpritePath = "An3sMod/Resources/EasyNPC/EasyNPC_shadow_001.png",
                acceptText = "Fine, I accept.",
                declineText = "Not happening shady guy.",
                OnAccept = new Action<PlayerController, GameObject>(TheDemon.Accept),
                OnDecline = null,
                CanUse = new Func<PlayerController, GameObject, bool>(TheDemon.CanUse),
                offset = new Vector3(40.2f, 50.8f, 51.3f),
                talkPointOffset = new Vector3(1.5f, 3, 0f),
                isToggle = false,
                isBreachShrine = true,
                interactableComponent = typeof(TheDemonInteractible)
            };
            
            GameObject gameObject = shrineFactory.Build();
            gameObject.AddAnimation("idle", "An3sMod/Resources/EasyNPC/EasyNPC_idle", 4, NPCBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            gameObject.AddAnimation("talk", "An3sMod/Resources/EasyNPC/EasyNPC_talk", 12, NPCBuilder.AnimationType.Talk, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            gameObject.AddAnimation("talk_start", "An3sMod/Resources/EasyNPC/EasyNPC_talk", 12, NPCBuilder.AnimationType.Other, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            gameObject.AddAnimation("do_effect", "An3sMod/Resources/EasyNPC/EasyNPC_talk", 12, NPCBuilder.AnimationType.Other, DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType.None);
            TheDemonInteractible component = gameObject.GetComponent<TheDemonInteractible>();
            component.conversation = new List<string>
            {
                "...",
                "Hey, I got something I would like to give you",
                "It should make the game a little more... ",
                "interesting.",  
                "What do you say?"
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
            string header = "Hard Mode Enabled";
            string text = "";
            Notify(header, text);
            storedPlayer = player;
            player.GiveItem("ans:the_demons_curse");
        }
       
        private static void Notify(string header, string text)
        {
            tk2dSpriteCollectionData encounterIconCollection = AmmonomiconController.Instance.EncounterIconCollection;
            int spriteIdByName = encounterIconCollection.GetSpriteIdByName("notreal/Resources/EasyNpc/EasyMode_icon");
            GameUIRoot.Instance.notificationController.DoCustomNotification(header, text, null, spriteIdByName, UINotificationController.NotificationColor.GOLD, false, true);
        }
    }
}

