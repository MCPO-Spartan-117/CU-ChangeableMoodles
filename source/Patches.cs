using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ChangeableMoodles;

[HarmonyPatch(typeof(MoodleManager), nameof(MoodleManager.AddMoodle))]
internal static class MoodleSpritePatch {
	static ManualLogSource Logger = Plugin.Logger;
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ILgen) {
		Logger.LogInfo("Inserting function entry in MoodleManager::AddMoodle");
		var startidx = -1;
		var endidx = -1;
		var compidx = -1;
		bool iconsfound = false;
		List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
		MethodInfo GetComp = AccessTools.Method(typeof(GameObject), nameof(GameObject.GetComponent), generics: [typeof(Image)]);
		FieldInfo iconsfield = AccessTools.Field(typeof(MoodleManager), nameof(MoodleManager.icons));
		MethodInfo setsprite = AccessTools.Method(typeof(Image), "set_" + nameof(Image.sprite));
		for(var i = 0; i < codes.Count; i++) {
			if(!iconsfound) {
				if(codes[i].opcode == OpCodes.Ldloc_2) {
					startidx = i;
					Logger.LogDebug("Found start " + i);
				} else if(i - startidx == 1 && codes[i].Calls(GetComp)) {
					compidx = i;
					Logger.LogDebug("Found GetComponent " + i);
				} else if(codes[i].LoadsField(iconsfield)) {
					iconsfound = true;
					Logger.LogDebug("Found Dictionary " + i);
				}
			} else {
				if(codes[i].Calls(setsprite)) {
					endidx = i;
					Logger.LogDebug("Found end " + i);
					break;
				} else if(codes[i].IsLdloc()) {
					Logger.LogDebug("Past end " + i);
					break;
				}
			}
		}

		if(endidx != -1) {
			LocalBuilder replacement = ILgen.DeclareLocal(typeof(Sprite));
			Label afterfunct = ILgen.DefineLabel();
			Label endlab = ILgen.DefineLabel();
			codes[compidx + 1].labels.Add(afterfunct);
			codes[endidx].labels.Add(endlab);
			List<CodeInstruction> callfunct = null;
			callfunct = [
				CodeInstruction.LoadField(typeof(Plugin), nameof(Plugin.SpriteCache)),
				new(OpCodes.Ldarg_2),
				new(OpCodes.Ldarga, 1),
				CodeInstruction.Call(typeof(int32_t), nameof(int32_t.ToString), []),
				CodeInstruction.Call(typeof(string), nameof(string.Concat), [typeof(string), typeof(string)]),
				new(OpCodes.Ldloca, replacement),
				CodeInstruction.Call(typeof(Dictionary<string, Sprite>), nameof(Dictionary<string, Sprite>.TryGetValue)),
				new(OpCodes.Brfalse, afterfunct),
				new(OpCodes.Ldloc, replacement),
				new(OpCodes.Br, endlab)
			];

			codes.InsertRange(compidx + 1, (IEnumerable<CodeInstruction>)callfunct);
		} else {
			Logger.LogError("Not patching");
		}

		return (IEnumerable<CodeInstruction>)codes;
	}
}
