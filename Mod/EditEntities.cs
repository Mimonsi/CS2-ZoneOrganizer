using System;
using System.Collections.Generic;
using Extra.Lib;
using Extra.Lib.Helper;
using Game.Net;
using Game.Prefabs;
using Unity.Collections;
using Unity.Entities;

namespace ExtraNetworksAndAreas.Mod
{
	internal static class EditEntities
	{
		internal static void SetupEditEntities()
		{
			EntityQueryDesc loggerQuery = new EntityQueryDesc
			{
				All =
				[

				],
				Any =
				[
					ComponentType.ReadOnly<UIAssetMenuData>(),
					ComponentType.ReadOnly<UIAssetCategoryData>(),
				]
			};
			ExtraLib.AddOnEditEnities(new(LoggerQuery, loggerQuery));


			EntityQueryDesc markerObjectsEntityQueryDesc = new EntityQueryDesc
			{
				All =
				[
					ComponentType.ReadOnly<UIObjectData>(),
					ComponentType.ReadOnly<ZoneData>(),
				],
				Any =
				[

				]
			};

			ExtraLib.AddOnEditEnities(new(OnEditMarkerObjectEntities, markerObjectsEntityQueryDesc));



		}

		private static void Log(string message)
		{
			ENA.Logger.Info(message);
		}

		private static string GetIcon(PrefabBase prefab)
		{
			Dictionary<string, string> overrideIcons = new()
			{
				{ "Oneway Tram Track - Inside", "Media/Game/Icons/OnewayTramTrack.svg" },
				{ "Twoway Subway Track", "Media/Game/Icons/TwoWayTrainTrack.svg" },
			};
			if (overrideIcons.TryGetValue(prefab.name, out string icon))
			{
				return icon;
			}
			return Icons.GetIcon(prefab);
		}

		private static void LoggerQuery(NativeArray<Entity> entities)
		{
			foreach (Entity entity in entities)
			{
				/*if (ExtraLib.m_PrefabSystem.TryGetPrefab(entity, out UIAssetMenuPrefab prefab1))
				{
					if (prefab1 != null)
						ENA.Logger.Info("Menu: " + prefab1.name);
				}*/

				if (ExtraLib.m_PrefabSystem.TryGetPrefab(entity, out UIAssetCategoryPrefab prefab2))
				{
					if (prefab2 != null)
					{
						ENA.Logger.Info("Category: " + prefab2.name);
						try
						{
							var zoneData = ExtraLib.m_PrefabSystem.GetComponentData<ZoneData>(prefab2);
							ENA.Logger.Info("Zone Index: " + zoneData.m_ZoneType.m_Index);
						}
						catch (Exception x)
						{}
					}
				}

			}
		}

		private static void OnEditMarkerObjectEntities(NativeArray<Entity> entities)
		{
			PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Low Density Residential","Media/Game/Icons/ZoneResidentialLow.svg");
			PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Medium Density Residential", "Media/Game/Icons/ZoneResidentialMedium.svg", "Low Density Residential");
			PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "High Density Residential", "Media/Game/Icons/ZoneResidentialHigh.svg", "Medium Density Residential");
			foreach (Entity entity in entities)
			{
				ExtraLib.m_PrefabSystem.TryGetPrefab(entity, out ZonePrefab zonePrefab);

				if (zonePrefab == null)
				{
					continue;
				}

				ENA.Logger.Info("Name: " + zonePrefab.name);
				var prefabUI = zonePrefab.GetComponent<UIObject>();
				prefabUI.m_Group?.RemoveElement(entity);

				//if (prefab.name == "")

				if (zonePrefab.name.Contains("Residential"))
				{
					// Zone name filter heavily inspired by Find It by T.D.W. (https://github.com/JadHajjar/FindIt-CSII)
					if (zonePrefab.name.Contains(" Row") || zonePrefab.name.Contains(" Medium") || zonePrefab.name.Contains(" Mixed"))
					{
						prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "High Density Residential", "Media/Game/Icons/ZoneResidentialHigh.svg", "Medium Density Residential");

					}
					else if (zonePrefab.name.Contains(" High") || zonePrefab.name.Contains(" LowRent"))
					{
						prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Medium Density Residential", "Media/Game/Icons/ZoneResidentialMedium.svg", "Low Density Residential");
					}
					else if (zonePrefab.name.Contains(" Low"))
					{
						prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Low Density Residential", "Media/Game/Icons/ZoneResidentialLow.svg");
						prefabUI.m_Icon = "Media/Game/Icons/ZoneResidentialLow.svg";
					}
				}
				/*else
				{
					prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Landscaping", "Marker Object Prefabs", Icons.GetIcon, "Spaces");
					prefabUI.m_Icon = GetIcon(prefab);
				}*/
				prefabUI.m_Group.AddElement(entity);

				ExtraLib.m_EntityManager.AddOrSetComponentData(entity, prefabUI.ToComponentData());
			}

			Log("Marker Object Entities Edited.");
		}

	}
}
