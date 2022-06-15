using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Gungeon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Globalization;
using Ionic.Zip;
using Alexandria.ItemAPI;
using System.Collections;
using InControl;
using Alexandria.BindingAPI;
namespace AutoUpdate
{
	public class Module : ETGModule
	{
		public static readonly string MOD_NAME = "Auto Updater";
		public static readonly string VERSION = "1.2";
		public static readonly string TEXT_COLOR = "#4287f5";
		public static int modsWithUpdates = 0;
		public static ETGModuleMetadata metadata;
	    static List<AutoUpdater> updaters = new List<AutoUpdater>();
		public static string modDeletionFile = Path.Combine(ETGMod.ResourcesDirectory, "AutoUpdater.txt");
		public static IEnumerator DelayedStartCR()
		{
			yield return null;
			DelayedStart();
			yield break;
		}

		public static void DelayedStart()
		{
			AutoUpdater latestUpdater = null;
			foreach(var line in File.ReadAllLines(ETGMod.ModsListFile))
			{
				if (!string.IsNullOrEmpty(line) && line[0] != '#')
				{
					var p = Path.Combine(ETGMod.ModsDirectory, line);
					int modId;
					//ETGModConsole.Log(p);
					if (fitsFormat(p, out modId))
					{
						var updater = new AutoUpdater();
						var shite = p.Split('\\').Last();
						updater.CheckForUpdate(modId, shite.Split('_')[1], shite.Split('_')[2].Replace(".zip", string.Empty).Trim(), p);
						updaters.Add(updater);
					}
				}
			}
			ETGMod.StartGlobalCoroutine(Results(latestUpdater));
			
		}

		private static IEnumerator Results(AutoUpdater updater)
		{
			yield return new WaitUntil(() => AllDone() == true);
			if (modsWithUpdates == 0)
			{
				Log($"{MOD_NAME} v{VERSION} started successfully with no updates!", TEXT_COLOR);
			}
			else
			{
				string upText;
				upText = (modsWithUpdates == 1) ? "update!" : "updates!";
				Log($"{MOD_NAME} v{VERSION} started successfully, and found " + modsWithUpdates + " " + upText, TEXT_COLOR);
			}
			foreach(var up in updaters)
			{
				up.Display();
			}
		}
		public static bool AllDone()
		{
			foreach(var updater in updaters)
			{
				if(updater.done == false)
				{
					return false;
				}
			}
			return true;
		}
		public override void Start()
		{
			try
			{
				metadata = this.Metadata;

				ETGModMainBehaviour.Instance.gameObject.AddComponent<MyBindingBehaviours>();
				if (!File.Exists(modDeletionFile))
				{
					File.Create(modDeletionFile);
				}
				foreach (var line in File.ReadAllLines(modDeletionFile))
				{
					if (!string.IsNullOrEmpty(line))
					{
						if (File.Exists(line))
						{
							File.Delete(line);

						}
						else if (Directory.Exists(line))
						{
							Directory.Delete(line, true);
						}

						File.WriteAllText(modDeletionFile, File.ReadAllText(modDeletionFile).Replace(line, string.Empty));
					}
				}
				
				ETGMod.StartGlobalCoroutine(DelayedStartCR());
			}
			catch (Exception e)
			{
				ETGModConsole.Log("mod Broke heres why: " + e);
			}
			
		}

	

		public static void Log(string text, string color = "FFFFFF")
		{
			ETGModConsole.Log($"<color={color}>{text}</color>");
		}

		public static bool fitsFormat(string filepath, out int id)
		{
			try
			{
				if (Directory.Exists(filepath))
				{
					var dir = filepath.Split('\\')[filepath.Split('\\').Length -1];
					var modId = dir.Split('_')[0];
					if (int.TryParse(modId, out id))
					{
						return true;
					}
				}
				else if (File.Exists(filepath))
				{
					var file = filepath.Split('\\')[filepath.Split('\\').Length - 1];
					var modId = file.Split('_')[0];
					if (int.TryParse(modId, out id))
					{

						return true ;
					}
				}
				id = -1;
				return false;
			}
			catch
			{
				id = -1;
				return false;
			}
		}

        public override void Exit() { }
		public override void Init() { }

	}
	class MyBindingBehaviours : MonoBehaviour
	{
		//my bindings
		PlayerAction die;
		PlayerAction speedUp;
		PlayerAction speedDown;

		FieldInfo m_currentGunAngle;

		//my oneaxisinputcontrol, aka can go from a positive to negative value
		OneAxisInputControl speed;
		public void Start()
		{
			m_currentGunAngle = typeof(PlayerController).GetField("m_currentGunAngle", BindingFlags.NonPublic | BindingFlags.Instance);

			//create bindings and default key binding
			die = BindingBuilder.CreateBinding("Die", InControl.Key.P);
			speedUp = BindingBuilder.CreateBinding("Speed Up", defaultmouse: InControl.Mouse.PositiveScrollWheel);
			speedDown = BindingBuilder.CreateBinding("Speed Down", defaultmouse: InControl.Mouse.NegativeScrollWheel);

			//creates the one axis input control
			speed = BindingBuilder.CreateOneAxisBinding(speedDown, speedUp);
		}

		void Update()
		{
			//runs code after die was pressed, there is also actions like, WasReleased, IsPressed, etc
			if (die.WasPressed)
			{
				if (GameManager.Instance.PrimaryPlayer != null)
					GameManager.Instance.PrimaryPlayer.healthHaver.ApplyDamage(9999999, Vector2.zero, "Fucking died", ignoreInvulnerabilityFrames: true);
				if (GameManager.Instance.SecondaryPlayer != null)
					GameManager.Instance.SecondaryPlayer.healthHaver.ApplyDamage(9999999, Vector2.zero, "Fucking died", ignoreInvulnerabilityFrames: true);
			}

			//gets the value of speed and applies that knockback
			if (GameManager.Instance.PrimaryPlayer != null)
				GameManager.Instance.PrimaryPlayer.knockbackDoer.ApplyKnockback(BraveMathCollege.DegreesToVector((float)m_currentGunAngle.GetValue(GameManager.Instance.PrimaryPlayer)), speed.Value * 10);
			if (GameManager.Instance.SecondaryPlayer != null)
				GameManager.Instance.SecondaryPlayer.knockbackDoer.ApplyKnockback(BraveMathCollege.DegreesToVector((float)m_currentGunAngle.GetValue(GameManager.Instance.SecondaryPlayer)), speed.Value * 10);
		}
	}
}
