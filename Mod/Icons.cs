using System.IO;
using Game.Prefabs;

namespace ZoneOrganizer.Mod
{
    internal class Icons
    {
        internal const string IconsResourceKey = "zoneorganizer";
        internal static readonly string COUIBaseLocation = $"coui://{IconsResourceKey}";

        public static readonly string Placeholder = $"{COUIBaseLocation}/Icons/Misc/placeholder.svg";
        public static readonly string GameCrashWarning = $"{COUIBaseLocation}/Icons/Misc/GameCrashWarning.svg";

        internal static void LoadIcons(string path)
        {
            Extra.Lib.UI.Icons.LoadIconsFolder(IconsResourceKey, path);
        }

        public static string GetIcon(PrefabBase prefab)
        {

            if (prefab is null)
                return $"{COUIBaseLocation}/Icons/Misc/placeholder.svg";

            if (File.Exists($"{ZoneOrganizer.ResourcesIcons}/{prefab.GetType().Name}/{prefab.name}.svg"))
            {
                return $"{COUIBaseLocation}/Icons/{prefab.GetType().Name}/{prefab.name}.svg";
            }

            if (prefab is UIAssetCategoryPrefab)
            {

                return $"{COUIBaseLocation}/Icons/Misc/placeholder.svg";
            }
            else if (prefab is UIAssetMenuPrefab)
            {

                return $"{COUIBaseLocation}/Icons/Misc/placeholder.svg";
            }

            return Placeholder;
        }

    }
}
