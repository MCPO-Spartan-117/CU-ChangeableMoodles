global using int8_t = System.SByte;
global using int16_t = System.Int16;
global using int32_t = System.Int32;
global using int64_t = System.Int64;
global using uint8_t = System.Byte;
global using uint16_t = System.UInt16;
global using uint32_t = System.UInt32;
global using uint64_t = System.UInt64;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;

namespace ChangeableMoodles;

[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInDependency("net.cucorelib", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin {
	public const string ModName = "ChangeableMoodles";
	public const string ModGUID = "LGPLv3.MCPO." + ModName;
	public const string ModVersion = "1.0.0";

	internal static new ManualLogSource Logger;
	private readonly Harmony _harmony = new(ModGUID);
	public static Plugin Instance { get; private set; }

	internal static string Moodlesdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Moodles";
	internal static readonly Dictionary<string, Sprite> SpriteCache = new(System.StringComparer.OrdinalIgnoreCase);

	private void Awake() {
		Logger = base.Logger;
		Instance = this;

		ModOptionsRegistry.Register(ModOptionDefinition.Bool(ModGUID + ".reload",
		"Reload Moodles",
		"For hotplugging Moodle sprites",
		Setting.SettingCategory.Video,
		true, value => {
			LoadSprites();
		}));

		_harmony.PatchAll();
		Logger.LogInfo($"Plugin {ModName} is awake!");
	}

	//LGPLv3 CUCoreLib
	internal static Sprite NormalizeIconSprite(Sprite iconSprite) {
		var normalizedSprite = Sprite.Create(
			iconSprite.texture,
			iconSprite.rect,
			iconSprite.pivot / iconSprite.rect.size,
			MoodleRegistry.VanillaMoodlePixelsPerUnit * (iconSprite.rect.width / 20.0f),
			0,
			SpriteMeshType.FullRect,
			iconSprite.border);

		normalizedSprite.name = iconSprite.name;
		return normalizedSprite;
	}

	internal static void LoadSprites() {
		SpriteCache.Clear();
		if(Directory.Exists(Moodlesdir)) {
			foreach(string png in Directory.GetFiles(Moodlesdir, "*.png", SearchOption.AllDirectories)) {
				string file = Path.GetFileNameWithoutExtension(png).ToLower();
				string key = (string)file.Clone();
				bool cucorelib = false;
				if(key.StartsWith("cucorelib.")) {
					cucorelib = true;
					key = key.Insert(10, "dynamic.|");
				}
				int32_t charint;
				//Bigger than end to prevent loop overflow
				uint16_t start = 0;
				uint8_t end = 0;
				uint8_t numidx = 0;
				string rangestring = null;
				while(numidx < key.Length) {
					charint = key[numidx] - '0';
					if(charint <= 9 && charint >= 0) {
						rangestring = key.Substring(numidx);
						if(cucorelib) {
							key = key.Insert(numidx, "|");
							numidx++;
						}
						break;
					}
					numidx++;
				}

				if(rangestring != null) {
					string[] rangestrings = rangestring.Split(["-"], System.StringSplitOptions.RemoveEmptyEntries);
					if(rangestrings.Length == 2) {
						try {
							start = Convert.ToByte(rangestrings[0]);
						} catch(System.OverflowException) {
							start = 0;
							Logger.LogError(file + ": Starting range bigger than 255, reseting to 0");
						}

						try {
							end = Convert.ToByte(rangestrings[1]);
						} catch(System.OverflowException) {
							end = 0;
							Logger.LogError(file + ": Ending range bigger than 255, reseting to 0");
						}

						if(start > end) {
							Logger.LogError(file + ": Start is bigger than end, setting start to end");
							start = end;
						}
						key = key.Substring(0, numidx);
					}
				} else {
					if(!cucorelib) {
						key += "0";
					} else {
						key += "|00";
					}
				}

				if(end == 0) {
					if(cucorelib && rangestring != null) {
						key += rangestring;
					}

					try {
						SpriteCache.Add(key, NormalizeIconSprite(AssetLoader.LoadSpriteFromFile(png)));
					} catch(System.ArgumentException) {
						Logger.LogWarning(key + " already exists.");
					}
				} else {
					Sprite samesprite = NormalizeIconSprite(AssetLoader.LoadSpriteFromFile(png));
					for(uint16_t i = start; i <= end; i++) {
						string intensity = i.ToString();
						if(cucorelib) {
							intensity += intensity;
						}

						try {
							SpriteCache.Add(key + intensity, samesprite);
						} catch(System.ArgumentException) {
							Logger.LogWarning(key + " already exists.");
						}
					}
				}
			}
		} else {
			Logger.LogWarning("Moodles directory doesn't exist");
		}
	}
}
