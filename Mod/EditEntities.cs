using System;
using System.Collections.Generic;
using Extra.Lib;
using Extra.Lib.Helper;
using Game.Prefabs;
using Game.UI.Editor;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ZoneOrganizer.Mod
{
	internal static class EditEntities
	{
		internal static void SetupEditEntities()
		{
			// EntityQueryDesc loggerQuery = new EntityQueryDesc
			// {
			// 	All =
			// 	[
			//
			// 	],
			// 	Any =
			// 	[
			// 		ComponentType.ReadOnly<UIAssetMenuData>(),
			// 		ComponentType.ReadOnly<UIAssetCategoryData>(),
			// 	]
			// };

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
			// ExtraLib.AddOnEditEnities(new(LoggerQuery, loggerQuery));
			ExtraLib.AddOnEditEnities(new(OnEditZonePrefabEntities, markerObjectsEntityQueryDesc));
		}

		private static void Log(string message)
		{
			ZoneOrganizer.Logger.Info(message);
		}

		// private static void LoggerQuery(NativeArray<Entity> entities)
		// {
		// 	foreach (Entity entity in entities)
		// 	{
		// 		/*if (ExtraLib.m_PrefabSystem.TryGetPrefab(entity, out UIAssetMenuPrefab prefab1))
		// 		{
		// 			if (prefab1 != null)
		// 				ENA.Logger.Info("Menu: " + prefab1.name);
		// 		}*/
		//
		// 		if (ExtraLib.m_PrefabSystem.TryGetPrefab(entity, out UIAssetCategoryPrefab prefab2))
		// 		{
		// 			if (prefab2 != null)
		// 			{
		// 				Log("Category: " + prefab2.name);
		// 				try
		// 				{
		// 					var zoneData = ExtraLib.m_PrefabSystem.GetComponentData<ZoneData>(prefab2);
		// 					Log("Zone Index: " + zoneData.m_ZoneType.m_Index);
		// 				}
		// 				catch (Exception x)
		// 				{}
		// 			}
		// 		}
		//
		// 	}
		// }

		private static void OnEditZonePrefabEntities(NativeArray<Entity> entities)
		{
			PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Low Density Residential","Media/Game/Icons/ZoneResidentialLow.svg");
			PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Medium Density Residential", "Media/Game/Icons/ZoneResidentialMedium.svg", "Low Density Residential");
			PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "High Density Residential", "Media/Game/Icons/ZoneResidentialHigh.svg", "Medium Density Residential");
			PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Mixed Housing", "Media/Game/Icons/ZoneResidentialMixed.svg", "High Density Residential");
			foreach (Entity entity in entities)
			{
				ExtraLib.m_PrefabSystem.TryGetPrefab(entity, out ZonePrefab zonePrefab);

				if (zonePrefab == null)
				{
					continue;
				}

				//Log("Name: " + zonePrefab.name);
				var prefabUI = zonePrefab.GetComponent<UIObject>();
				prefabUI.m_Group?.RemoveElement(entity);

				//if (prefab.name == "")

				if (zonePrefab.name.Contains("Residential"))
				{
					// Zone name filter heavily inspired by Find It by T.D.W. (https://github.com/JadHajjar/FindIt-CSII)
					if (zonePrefab.name.Contains(" Mixed"))
					{
						prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Mixed Housing", "Media/Game/Icons/ZoneResidentialMixed.svg", "High Density Residential");
					}
					if (zonePrefab.name.Contains(" Row") || zonePrefab.name.Contains(" Medium"))
					{
						prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Medium Density Residential", "Media/Game/Icons/ZoneResidentialMedium.svg", "Low Density Residential");
					}
					else if (zonePrefab.name.Contains(" High") || zonePrefab.name.Contains(" LowRent"))
					{
						prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "High Density Residential", "Media/Game/Icons/ZoneResidentialHigh.svg", "Medium Density Residential");
					}
					else if (zonePrefab.name.Contains(" Low"))
					{
						prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Low Density Residential", "Media/Game/Icons/ZoneResidentialLow.svg");
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
		}

	}
}
