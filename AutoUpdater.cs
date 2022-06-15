using SGUI;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Diagnostics;

namespace AutoUpdate
{
	
	class AutoUpdater
	{

		private int ModDownload;
		private int modID;

		bool gotModDownload = false;
		private bool updating = false;
		public bool done = false;
		bool downloaded = false;

		private string latestModVersion;
		private string oldVersion;
		private string modFileName;
		
		private string name;
		private string directory;
		public void CheckForUpdate(int ModID, string modName, string ModsVersion, string dir)
		{
			try
			{
				oldVersion = ModsVersion;
				modID = ModID;
				name = modName;
				directory = dir;
				ETGMod.StartGlobalCoroutine(GetModVersion(modID));
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError("AutoUpdater Error" + e);
			}
		}

		private static void no(SButton obj)
		{
			AdvancedLogging.LogPlain("Update ignored.");
		}


		private void AreYouSure(SButton obj)
		{
			AdvancedLogging.LogPlain("Are you sure? this will restart ETG.");
			var yesButton = AdvancedLogging.LogButton("Yes", Color.green);
			var noButton = AdvancedLogging.LogButton("I change my mind", Color.green);
			yesButton.OnClick += OkDownload;
			noButton.OnClick += no;
		}

		private void OkDownload(SButton obj)
		{
			if (!updating)
			{
				updating = true;
				ETGModConsole.Log($"Updating {name}... Please wait.");

				File.WriteAllText(ETGMod.ModsListFile, File.ReadAllText(ETGMod.ModsListFile)
						.Replace(directory.Split('\\')[directory.Split('\\').Length - 1], string.Empty));
				File.WriteAllText(Module.modDeletionFile, File.ReadAllText(Module.modDeletionFile) + "\n" + directory);

				ETGMod.StartGlobalCoroutine(doRestOfStuff());
			}
		}

		public void Display()
		{
			if (oldVersion != latestModVersion.Trim())
			{
				AdvancedLogging.LogPlain("Newer version of \"" + name + "\" found!  \nUpdate? (click text options)", Color.red);
				var yesButton = AdvancedLogging.LogButton("Yes, update " + name, Color.green);
				var noButton = AdvancedLogging.LogButton("No thanks", Color.green);
				yesButton.OnClick += AreYouSure;
				noButton.OnClick += no;
			}
		}

		private IEnumerator doRestOfStuff()
		{
			yield return new WaitForSecondsRealtime(2);
			ETGMod.StartGlobalCoroutine(GetModDownload(modID));
			yield return new WaitUntil(() => gotModDownload == true);
			ETGMod.StartGlobalCoroutine(Download(ModDownload));
			yield return new WaitUntil(() => downloaded == true);
			File.WriteAllText(ETGMod.ModsListFile, File.ReadAllText(ETGMod.ModsListFile) + "\n" + modFileName);
			Process.Start(Application.dataPath + "/../EtG.exe");
			Application.Quit();
		}

		private IEnumerator Download(int modDownload)
		{

			UnityWebRequest www = UnityWebRequest.Get("https://api.modwork.shop/api.php?command=DownloadFile&fid=" + modDownload + "&token=Je3KeUETqqym6V8b5T7nFdudz74yWXgU");
			www.downloadHandler = new DownloadHandlerFile(Path.Combine(ETGMod.ModsDirectory, modFileName));
			yield return www.SendWebRequest();
			downloaded = true;
			if (www.isHttpError)
			{
				AdvancedLogging.LogError(www.error);
				

			}
			else
			{

			}
		}

		IEnumerator GetModDownload(int s)
		{
			UnityWebRequest www = UnityWebRequest.Get("https://api.modwork.shop/api.php?command=AssocFiles&did=" + s + "&token=Je3KeUETqqym6V8b5T7nFdudz74yWXgU");
			yield return www.Send();

			if (www.isHttpError)
			{
				AdvancedLogging.LogError(www.error);
			}
			else
			{
				// Show results as text
				ModDownload = int.Parse(www.downloadHandler.text.Split('\"')[1]);
				modFileName = www.downloadHandler.text.Split('\"')[2].Insert(0, modID + "_");
				modFileName = modFileName.Replace(".zip", "_"+latestModVersion.Trim()+ "_.zip");
				modFileName = modFileName.Replace(".zip", UnityEngine.Random.Range(0, 9999999).ToString()) + ".zip";
				gotModDownload = true;
			}
		}
		IEnumerator GetModVersion(int s)
		{
			UnityWebRequest www = UnityWebRequest.Get("https://api.modwork.shop/api.php?command=CompareVersion&did=" + s + "&vid=$version$&token=Je3KeUETqqym6V8b5T7nFdudz74yWXgU");
			yield return www.Send();

			if (www.isHttpError)
			{
				AdvancedLogging.LogError(www.error);
			}
			else
			{
				string[] invalidChars = new string[] { "\\", "/", ":", "*","?", "\"","<",">","|"};
				// Show results as text
				//ETGModConsole.Log(www.downloadHandler.text);
				latestModVersion = www.downloadHandler.text;
				if (!string.IsNullOrEmpty(latestModVersion))
				{
					if (oldVersion.Contains("⃞"))
					{
						for(int i = 0; i < invalidChars.Length; i++)
						{
							if(oldVersion.Replace("⃞", invalidChars[i]) == latestModVersion.Trim())
							{
								oldVersion = oldVersion.Replace("⃞", invalidChars[i]);
							}
						}
					}
					if (oldVersion != latestModVersion.Trim())
					{
						Module.modsWithUpdates++;
					}
					done = true;
				}
				else
				{
					latestModVersion = oldVersion;
				}
			}
		}

		
		public class AdvancedLogging
		{
			public static void LogError(object msg)
			{
				string message = msg.ToString();
				Color color = Color.red;

				SGroup sGroup = new SGroup();
				sGroup.AutoGrowDirection = SGroup.EDirection.Vertical;
				sGroup.AutoLayout = (SGroup g) => g.AutoLayoutHorizontal;
				sGroup.OnUpdateStyle = delegate (SElement elem)
				{
					elem.Fill();
				};
				sGroup.AutoLayoutVerticalStretch = false;
				sGroup.AutoLayoutHorizontalStretch = false;
				sGroup.GrowExtra = Vector2.zero;
				sGroup.ContentSize = Vector2.zero;
				sGroup.AutoLayoutPadding = 0;
				sGroup.Background = Color.clear;

				LogLabel modname = new LogLabel(Module.metadata.Name + ": ");
				modname.Colors[0] = color;
				sGroup.Children.Add(modname);

				LogLabel label = new LogLabel(message);
				label.Colors[0] = color;
				label.Background = Color.clear;
				sGroup.Children.Add(label);

				ETGModConsole.Instance.GUI[0].Children.Add(sGroup);

				ETGModConsole.Instance.GUI[0].UpdateStyle();
			}

			public static void LogPlain(object msg, Color32? col = null)
			{
				Color color = Color.white;
				if (col != null)
				{
					color = col.Value;
				}
				SLabel label = new SLabel(msg.ToString());
				label.Colors[0] = color;
				ETGModConsole.Instance.GUI[0].Children.Add(label);
			}

			/// <summary>
			/// Used to log messages. messages can have the mod name, mod icon, both, or none in front.
			/// if color is set to null, messege color will be set to white
			/// note that modifiers are applied the sGroup children, not to the sGroup. if you want to change/add modifiers to the sGroup, use the returned sGroup
			/// n order to log an image in your text, you do "@(embedded file path, sizemult)" in your text, sizemult is not required, will default to 1.
			/// </summary>
			/// <param name="msg">the object or string you want to log</param>
			/// <param name="modifiers">an array of Selement modifiers to be added to each element logged</param>
			/// <param name="col">text color in unity Color32 or Color</param>
			/// <param name="HaveModName">whether your log messege will have the mod name at the front</param>
			/// <param name="HaveModIcon">whether your logged messege will have your mod icon at the front</param>
			public static SGroup Log(object msg, Color32? col = null, bool HaveModName = false, bool HaveModIcon = false, SModifier[] modifiers = null)
			{

				//in your module outside of methods you need:
				// public static ETGModuleMetadata metadata = new ETGModuleMetadata(); 
				//public static string ZipFilePath;

				//then in your modules init you need:
				// metadata = this.Metadata;

				Color color = Color.white;
				if (col != null)
				{
					color = col.Value;
				}
				string message = msg.ToString();

				SGroup sGroup = new SGroup();
				sGroup.AutoGrowDirection = SGroup.EDirection.Vertical;
				sGroup.AutoLayout = (SGroup g) => g.AutoLayoutHorizontal;
				sGroup.OnUpdateStyle = delegate (SElement elem)
				{
					elem.Fill();
				};
				sGroup.AutoLayoutVerticalStretch = false;
				sGroup.AutoLayoutHorizontalStretch = false;
				sGroup.GrowExtra = Vector2.zero;
				sGroup.ContentSize = Vector2.zero;
				sGroup.AutoLayoutPadding = 0;
				sGroup.Background = Color.clear;

				if (File.Exists(Module.metadata.Archive))
				{
					if (HaveModIcon)
					{
						SImage icon = new SImage(Module.metadata.Icon);
						sGroup.Children.Add(icon);
					}
				}
				if (HaveModName)
				{
					LogLabel modname = new LogLabel(Module.metadata.Name + ": ");
					modname.Colors[0] = color;
					sGroup.Children.Add(modname);
				}
				string[] split = Regex.Split(message, "(@\\(.+?\\))");
				foreach (string item in split)
				{

					if (item.StartsWith("@("))
					{
						string image = item.TrimStart('@', '(').TrimEnd(')');
						string[] sizeMult = image.Split(',');
						image = sizeMult[0];
						float SizeMultButForReal = 1;

						if (sizeMult.Length > 1)
						{
							if (sizeMult[1] != null && sizeMult[1] != "" && sizeMult[1] != " ")
								SizeMultButForReal = float.Parse(sizeMult[1]);
						}

						string extension = !image.EndsWith(".png") ? ".png" : "";
						string path = image + extension;
						Texture2D tex = GetTextureFromResource(path);
						TextureScale.Point(tex, Mathf.RoundToInt(tex.width * SizeMultButForReal), Mathf.RoundToInt(tex.height * SizeMultButForReal));

						SImage img = new SImage(tex);
						sGroup.Children.Add(img);
						var idx = sGroup.Children.IndexOf(img);
					}
					else
					{
						LogLabel label = new LogLabel(item);
						label.Colors[0] = color;
						label.Background = Color.clear;
						sGroup.Children.Add(label);
					}
					if (modifiers != null)
					{
						for (int i = 0; i < modifiers.Length; i++)
						{
							sGroup.Children[sGroup.Children.Count - 1].Modifiers.Add(modifiers[i]);
						}
					}
				}
				ETGModConsole.Instance.GUI[0].Children.Add(sGroup);

				ETGModConsole.Instance.GUI[0].UpdateStyle();
				return sGroup;
			}

			/// <summary>
			/// used to log buttons, buttons can run certain code when pressed. 
			/// do SButton button = YourLogCode.
			/// then button.OnClick += myMethod;
			/// </summary>
			/// <param name="msg">the object or string you want to log</param>
			/// <param name="col">text color you want</param>
			/// <param name="HaveModName">whether your log messege will have the mod name at the front</param>
			/// <param name="HaveModIcon">whether your logged messege will have your mod icon at the front</param>
			public static SButton LogButton(object msg, Color32? col = null, string UpdatedTextOnClick = null, bool HaveModName = false, bool HaveModIcon = false)
			{

				SButton btn;
				Color color = Color.white;
				if (col != null)
				{
					color = col.Value;
				}
				if (HaveModIcon == false)
				{
					if (HaveModName == false)
					{
						btn = new SButton($"{msg}");
						btn.Background = Color.clear;
						btn.Colors[0] = color;
						ETGModConsole.Instance.GUI[0].Children.Add(btn);

					}
					else
					{
						btn = new SButton($"{Module.metadata.Name}: {msg}");
						btn.Background = Color.clear;
						btn.Colors[0] = color;
						ETGModConsole.Instance.GUI[0].Children.Add(btn);
					}
				}
				else
				{
					if (HaveModName == false)
					{
						btn = new SButton($"{msg}");
						btn.Background = Color.clear;
						btn.Colors[0] = color;
						if (File.Exists(Module.metadata.Archive))
							btn.Icon = Module.metadata.Icon;
						ETGModConsole.Instance.GUI[0].Children.Add(btn);
					}
					else
					{
						btn = new SButton($"{Module.metadata.Name}: {msg}");
						btn.Background = Color.clear;
						btn.Colors[0] = color;
						if (File.Exists(Module.metadata.Archive))
							btn.Icon = Module.metadata.Icon;
						ETGModConsole.Instance.GUI[0].Children.Add(btn);
					}
				}

				bool ShowAlt = false;
				if (!string.IsNullOrEmpty(UpdatedTextOnClick))
				{
					btn.OnClick += (obj) =>
					{
						ShowAlt = !ShowAlt;
						ETGModConsole.Instance.GUI[0].UpdateStyle();
					};
					var i = new SLabel(UpdatedTextOnClick);
					i.Colors[0] = color;
					i.Background = Color.clear;

					i.OnUpdateStyle = delegate (SElement elem)
					{
						elem.Size.y = ShowAlt ? elem.Size.y : 0f;
						elem.Visible = ShowAlt;
					};
					ETGModConsole.Instance.GUI[0].Children.Add(i);

				}

				return btn;

			}

			/// <summary>
			/// Used to log images. 
			/// if you want to log something you have in your mod, 
			/// ie. an embedded resource, use log and put in @(filepath)
			/// </summary>
			/// <param name="img">The texture of the item/enemy/image you want to log.</param> 
			/// <returns></returns>
			public static SImage LogImage(Texture img)
			{
				var image = new SImage(img);
				ETGModConsole.Instance.GUI[0].Children.Add(image);
				return image;
			}

			///// <summary>
			///// Used to log animations. 
			///// </summary>
			///// <param name="frames">a list of spritepaths used for your animation *in order*.</param> 
			///// <param name="TimeBetweenFrames"> how many seconds it should wait until it playes the next frame.</param> 
			///// <returns></returns>
			//public static void LogAnim(List<string> frames, float TimeBetweenFrames)
			//{
			//	var img = l(frames[0]);
			//	var imgthang = ETGModConsole.Instance.GUI[0].Children[img] as SImage;
			//	var anim = ETGModMainBehaviour.Instance.gameObject.GetOrAddComponent<PlayAnim>();
			//	anim.period = TimeBetweenFrames;
			//	anim.imgToAnim = img;
			//	anim.frames = frames;
			//}

			public static byte[] ExtractEmbeddedResource(String filePath)
			{
				filePath = filePath.Replace("/", ".");
				filePath = filePath.Replace("\\", ".");
				var baseAssembly = Assembly.GetCallingAssembly();
				using (Stream resFilestream = baseAssembly.GetManifestResourceStream(filePath))
				{
					if (resFilestream == null)
					{
						return null;
					}
					byte[] ba = new byte[resFilestream.Length];
					resFilestream.Read(ba, 0, ba.Length);
					return ba;
				}
			}

			public static Texture2D GetTextureFromResource(string resourceName)
			{
				string file = resourceName;
				byte[] bytes = ExtractEmbeddedResource(file);
				if (bytes == null)
				{
					AdvancedLogging.Log("No bytes found in " + file, Color.red);
					return null;
				}
				Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
				ImageConversion.LoadImage(texture, bytes);
				texture.filterMode = FilterMode.Point;

				string name = file.Substring(0, file.LastIndexOf('.'));
				if (name.LastIndexOf('.') >= 0)
				{
					name = name.Substring(name.LastIndexOf('.') + 1);
				}
				texture.name = name;

				return texture;
			}
		}


		public class LogLabel : SElement
		{

			public string Text;

			public TextAnchor Alignment = TextAnchor.MiddleLeft;

			public LogLabel()
				: this("") { }

			public LogLabel(string text)
			{
				Text = text;
			}

			public override void UpdateStyle()
			{
				// This will get called again once this element gets added to the root.
				if (Root == null) return;

				if (UpdateBounds)
				{
					if (Parent == null)
					{
						Size = Backend.MeasureText(Text);
					}
					else
					{
						Size = Backend.MeasureText(Text, Parent.InnerSize, font: Font);
					}
				}

				base.UpdateStyle();
			}

			public override void Render()
			{
				RenderBackground();
				Draw.Text(this, Vector2.zero, Size, Text, Alignment);
			}
		}

	}

}
