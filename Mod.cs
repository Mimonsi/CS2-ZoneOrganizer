using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Unity.Entities;

namespace ZoneOrganizer
{
	public class Mod : IMod
	{
		public static readonly ILog log = LogManager.GetLogger($"{nameof(ZoneOrganizer)}").SetShowsErrorsInUI(false);

		public void OnLoad(UpdateSystem updateSystem)
		{
			log.Info(nameof(OnLoad));

			if (!GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset)) return;

			log.Info($"Current mod asset at {asset.path}");

            foreach (var item in new LocaleHelper("ZoneOrganizer.Locale.json").GetAvailableLanguages())
            {
                GameManager.instance.localizationManager.AddSource(item.LocaleId, item);
            }

            var zoneOrganizerSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ZoneOrganizerSystem>();
            // TODO: Add settings to enable separation
		}

		public void OnDispose()
		{

		}
	}
}
