using System;
using System.Collections.Generic;
using Colossal.Entities;
using Colossal.Logging;
using Game;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI.Editor;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ZoneOrganizer
{


	public enum ZoneTypeFilter
	{
		Any = 0,
		Low = 1,
		Row = 2,
		Medium = 4,
		High = 8,
		Signature = 16,
	}

	public partial class ZoneOrganizerSystem : GameSystemBase
	{
		private static ILog Log => Log;
		private PrefabSystem _prefabSystem;
		private EntityQuery _uiAssetMenuDataQuery;
		private EntityQuery _uiAssetCategoryDataQuery;
		private EntityQuery _zoneQuery;
		private static Dictionary<Entity, ZoneTypeFilter> _zoneTypeCache;
		private static Dictionary<string, Entity> _assetMenuDataDict = new();
		private static Dictionary<string, Entity> _assetCatDataDict = new();

		protected override void OnCreate()
		{
			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_uiAssetMenuDataQuery = GetEntityQuery(new EntityQueryDesc
				{ All = new[] { ComponentType.ReadWrite<UIAssetMenuData>() } });
			_uiAssetCategoryDataQuery = GetEntityQuery(new EntityQueryDesc
				{ All = new[] { ComponentType.ReadWrite<UIAssetCategoryData>() } });
			_zoneQuery = GetEntityQuery(new EntityQueryDesc
			{
				All =
					new[]
					{
						ComponentType.ReadOnly<UIObjectData>(),
						ComponentType.ReadOnly<ZoneData>(),
					}
			});

			GameManager.instance.RegisterUpdater(Init);
		}

		private void Init()
		{
			CollectData();
			IndexZones();
			EditZones();
		}

		public void CollectData()
		{
			try
			{
				var entities = _uiAssetMenuDataQuery.ToEntityArray(Allocator.Temp);
				foreach (Entity entity in entities)
				{
					string prefabName = _prefabSystem.GetPrefabName(entity);
					if (!_assetMenuDataDict.ContainsKey(prefabName))
					{
						_assetMenuDataDict.Add(prefabName, entity);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}

			try
			{
				var entities = _uiAssetCategoryDataQuery.ToEntityArray(Allocator.Temp);
				foreach (Entity entity in entities)
				{
					string prefabName = _prefabSystem.GetPrefabName(entity);
					if (!_assetCatDataDict.ContainsKey(prefabName))
					{
						_assetCatDataDict.Add(prefabName, entity);
					}

					Log.Info($"Adding {prefabName} to assetCatDataDict");
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		private void EditZones()
		{
			CreateUIAssetCategoryPrefab("Low Density Residential", "Zones", "Media/Game/Icons/ZoneResidentialLow.svg",
				1);
			CreateUIAssetCategoryPrefab("Medium Density Residential", "Zones",
				"Media/Game/Icons/ZoneResidentialMedium.svg", 2);
			CreateUIAssetCategoryPrefab("High Density Residential", "Zones", "Media/Game/Icons/ZoneResidentialHigh.svg",
				3);
			CreateUIAssetCategoryPrefab("Mixed Housing", "Zones", "Media/Game/Icons/ZoneResidentialMixed.svg", 4);

			var entities = _zoneQuery.ToEntityArray(Allocator.Temp);
			foreach (Entity entity in entities)
			{
				if (_prefabSystem.TryGetPrefab(entity, out ZonePrefab zonePrefab))
				{
					Log.Info($"Zone prefab: {zonePrefab.name}");
					if (zonePrefab == null)
					{
						continue;
					}

					//Log("Name: " + zonePrefab.name);
					/*var prefabUI = zonePrefab.GetComponent<UIObject>();
					prefabUI.m_Group?.RemoveElement(entity);

					//if (prefab.name == "")

					if (zonePrefab.name.Contains("Residential"))
					{
						// Zone name filter heavily inspired by Find It by T.D.W. (https://github.com/JadHajjar/FindIt-CSII)
						if (zonePrefab.name.Contains(" Mixed"))
						{
							prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones", "Mixed Housing",
								"Media/Game/Icons/ZoneResidentialMixed.svg", "High Density Residential");
						}

						if (zonePrefab.name.Contains(" Row") || zonePrefab.name.Contains(" Medium"))
						{
							prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones",
								"Medium Density Residential", "Media/Game/Icons/ZoneResidentialMedium.svg",
								"Low Density Residential");
						}
						else if (zonePrefab.name.Contains(" High") || zonePrefab.name.Contains(" LowRent"))
						{
							prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones",
								"High Density Residential", "Media/Game/Icons/ZoneResidentialHigh.svg",
								"Medium Density Residential");
						}
						else if (zonePrefab.name.Contains(" Low"))
						{
							prefabUI.m_Group = PrefabsHelper.GetOrCreateUIAssetCategoryPrefab("Zones",
								"Low Density Residential", "Media/Game/Icons/ZoneResidentialLow.svg");
						}
					}

					prefabUI.m_Group.AddElement(entity);

					ExtraLib.m_EntityManager.AddOrSetComponentData(entity, prefabUI.ToComponentData());*/
				}
			}
		}

		protected override void OnUpdate()
		{
			int x = 3;
			int y = 3;
		}

		public Entity CreateUIAssetCategoryPrefab(string name, string group, string icon, int priority)
		{

			if (!_prefabSystem.TryGetPrefab(new PrefabID("UIAssetCategoryPrefab", name), out PrefabBase tab))
			{

				UIAssetCategoryPrefab menuPrefab = ScriptableObject.CreateInstance<UIAssetCategoryPrefab>();
				menuPrefab.name = name;
				EntityManager.TryGetComponent(_assetMenuDataDict[group], out PrefabData prefabData);
				_prefabSystem.TryGetPrefab(prefabData, out PrefabBase roadMenu);

				menuPrefab.m_Menu = roadMenu.GetComponent<UIAssetMenuPrefab>();
				var MenuUI = menuPrefab.AddComponent<UIObject>();
				MenuUI.m_Icon = icon;
				MenuUI.m_Priority = priority;
				MenuUI.active = true;
				MenuUI.m_IsDebugObject = false;
				MenuUI.m_Group = null;
				_prefabSystem.AddPrefab(menuPrefab);
				tab = menuPrefab;
			}

			_prefabSystem.TryGetEntity(tab, out Entity tabEntity);

			return tabEntity;
		}


		private void IndexZones()
		{
			var zonesQuery = SystemAPI.QueryBuilder().WithAll<ZoneData, ZonePropertiesData, PrefabData>().Build();
			var zones = zonesQuery.ToEntityArray(Allocator.Temp);
			var propertiesData = zonesQuery.ToComponentDataArray<ZonePropertiesData>(Allocator.Temp);

			var buildingsQuery = SystemAPI.QueryBuilder().WithAll<BuildingData, SpawnableBuildingData, PrefabData>()
				.WithNone<SignatureBuildingData>().Build();
			var buildingsData = buildingsQuery.ToComponentDataArray<BuildingData>(Allocator.Temp);
			var spawnableBuildings = buildingsQuery.ToComponentDataArray<SpawnableBuildingData>(Allocator.Temp);

			var dictionary = new Dictionary<Entity, ZoneTypeFilter>();

			for (var i = 0; i < zones.Length; i++)
			{
				var zone = zones[i];
				var info = propertiesData[i];

				if (info.m_ResidentialProperties <= 0f)
				{
					dictionary[zone] = ZoneTypeFilter.Any;
					continue;
				}

				var ratio = info.m_ResidentialProperties / info.m_SpaceMultiplier;

				if (!info.m_ScaleResidentials)
				{
					dictionary[zone] = ZoneTypeFilter.Low;
				}
				else if (ratio < 1f)
				{
					var isRowHousing = true;

					for (var j = 0; j < spawnableBuildings.Length; j++)
					{
						if (spawnableBuildings[j].m_ZonePrefab == zone && buildingsData[j].m_LotSize.x > 2)
						{
							isRowHousing = false;
							break;
						}
					}

					dictionary[zone] = isRowHousing ? ZoneTypeFilter.Row : ZoneTypeFilter.Medium;
				}
				else
				{
					dictionary[zone] = ZoneTypeFilter.High;
				}
			}

			_zoneTypeCache = dictionary;
			Log.Info("Zones have been indexed");
		}

		public static ZoneTypeFilter GetZoneType(Entity zonePrefab)
		{
			if (_zoneTypeCache != null && _zoneTypeCache.TryGetValue(zonePrefab, out var type))
			{
				return type;
			}

			return ZoneTypeFilter.Any;
		}
	}
}