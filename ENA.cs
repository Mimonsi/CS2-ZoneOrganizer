using Colossal.Logging;
using Extra.Lib.Debugger;
using Game;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;
using System.IO;
using System.Linq;
using ExtraNetworksAndAreas.Mod;

namespace ExtraNetworksAndAreas
{
	public class ENA : IMod
	{
		private static readonly ILog log = LogManager.GetLogger($"{nameof(ExtraNetworksAndAreas)}").SetShowsErrorsInUI(false);
		internal static Logger Logger { get; private set; } = new(log, false);

		internal static string ResourcesIcons { get; private set; }

		private Harmony harmony;

		public void OnLoad(UpdateSystem updateSystem)
		{
			Logger.Info(nameof(OnLoad));

			if (!GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset)) return;

			Logger.Info($"Current mod asset at {asset.path}");

			FileInfo fileInfo = new(asset.path);

			ResourcesIcons = Path.Combine(fileInfo.DirectoryName, "Icons");

			EditEntities.SetupEditEntities();
			Icons.LoadIcons(fileInfo.DirectoryName);

			harmony = new($"{nameof(ExtraNetworksAndAreas)}.{nameof(ENA)}");
			harmony.PatchAll(typeof(ENA).Assembly);
			var patchedMethods = harmony.GetPatchedMethods().ToArray();
			Logger.Info($"Plugin ExtraNetworksAndAreas made patches! Patched methods: " + patchedMethods.Length);
			foreach (var patchedMethod in patchedMethods)
			{
				Logger.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
			}

		}

		public void OnDispose()
		{
			Logger.Info(nameof(OnDispose));
			harmony.UnpatchAll($"{nameof(ExtraNetworksAndAreas)}.{nameof(ENA)}");
		}
	}
}
