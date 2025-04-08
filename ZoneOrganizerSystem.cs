using System;
using System.Collections.Generic;
using Colossal.Entities;
using Colossal.IO.AssetDatabase;
using Colossal.Json;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.Economy;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI.InGame;
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
		Mixed = 16,
	}

    public class AssetMenuData
    {
        public Dictionary<string, Entity> Menu { get; set; } = new Dictionary<string, Entity>();
        public Dictionary<string, int> Priority { get; set; } = new Dictionary<string, int>();
    }

    public partial class ZoneOrganizerSystem : GameSystemBase
	{
		private static ILog Log => Mod.log;
		private PrefabSystem _prefabSystem;
		private EntityQuery _uiAssetMenuDataQuery;
		//private EntityQuery _uiAssetCategoryDataQuery;
		private EntityQuery _zoneQuery;
		private static Dictionary<Entity, ZoneTypeFilter> _zoneTypeCache;
		private static readonly Dictionary<string, Entity> _assetMenuDataDict = new();
		//private static readonly Dictionary<string, Entity> _assetCatDataDict = new();
        public static AssetMenuData zoneAssetMenuData = new();

		protected override void OnCreate()
        {
            base.OnCreate();
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_uiAssetMenuDataQuery = GetEntityQuery(new EntityQueryDesc
				{ All = new[] { ComponentType.ReadWrite<UIAssetMenuData>() } });
			//_uiAssetCategoryDataQuery = GetEntityQuery(new EntityQueryDesc
			//	{ All = new[] { ComponentType.ReadWrite<UIAssetCategoryData>() } });
			_zoneQuery = GetEntityQuery(new EntityQueryDesc
			{
				All =
					new[]
					{
						ComponentType.ReadOnly<UIObjectData>(),
						ComponentType.ReadOnly<ZoneData>(),
					}
			});

			//GameManager.instance.RegisterUpdater(Init);
		}

		private void Init()
		{
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

			//try
			//{
			//	var entities = _uiAssetCategoryDataQuery.ToEntityArray(Allocator.Temp);
			//	foreach (Entity entity in entities)
			//	{
			//		_prefabSystem.TryGetPrefab(new PrefabID("UIAssetMenuPrefab", "Zones"), out PrefabBase zonePrefab);
			//		string prefabName = _prefabSystem.GetPrefabName(entity);
			//		_prefabSystem.TryGetPrefab(entity, out UIAssetCategoryPrefab uiA);
			//		if (!_assetCatDataDict.ContainsKey(prefabName) && uiA.m_Menu == (UIAssetMenuPrefab)zonePrefab)
			//		{
			//			_assetCatDataDict.Add(prefabName, entity);
			//                     Log.Info($"Adding {prefabName} to assetCatDataDict");
			//                 }

			//	}
			//}
			//catch (Exception e)
			//{
			//	Log.Error(e);
			//}
		}

//		private void EditZones()
//		{
//			var entities = _zoneQuery.ToEntityArray(Allocator.Temp);

//            foreach (Entity entity in entities)
//			{
//				if (_prefabSystem.TryGetPrefab(entity, out ZonePrefab zonePrefab))
//				{
//					if (zonePrefab == null)
//					{
//						continue;
//                    }
//#if DEBUG
//					Log.Info($"Zone prefab: {zonePrefab.name}");
//#endif

//                }
//			}
//		}

		protected override void OnUpdate()
		{
			//int x = 3;
			//int y = 3;
		}
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            CollectData();
            IndexZones();
            //EditZones();
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

			
			//var zonesQuery = SystemAPI.QueryBuilder().WithAll<ZoneData, ZonePropertiesData, PrefabData>().Build();
			//var zones = zonesQuery.ToEntityArray(Allocator.Temp);
			//var propertiesData = zonesQuery.ToComponentDataArray<ZonePropertiesData>(Allocator.Temp);

			//var buildingsQuery = SystemAPI.QueryBuilder().WithAll<BuildingData, SpawnableBuildingData, PrefabData>()
			//	.WithNone<SignatureBuildingData>().Build();
			//var buildingsData = buildingsQuery.ToComponentDataArray<BuildingData>(Allocator.Temp);
			//var spawnableBuildings = buildingsQuery.ToComponentDataArray<SpawnableBuildingData>(Allocator.Temp);

			var dictionary = new Dictionary<Entity, ZoneTypeFilter>();

            //for (var i = 0; i < zones.Length; i++)
            var entities = _zoneQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
				var zone = entity;
				if (EntityManager.TryGetComponent(zone, out PrefabData prefabData) && _prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase) &&
                _prefabSystem.TryGetComponentData(prefabBase, out ZonePropertiesData info))
                {
                    string zoneName = _prefabSystem.GetPrefabName(zone);
					if (info.m_ResidentialProperties <= 0f)
                    {
#if DEBUG
                        Log.Info($"{zoneName} is skipped");
#endif
                        continue;
					}

					if (!info.m_ScaleResidentials)
					{
						dictionary[zone] = ZoneTypeFilter.Low;
                        ProcessMovingAssets(entity);
#if DEBUG
                        Log.Info($"{zoneName} set to Low");
#endif
                        continue;
					}

					if (info.m_ResidentialProperties / info.m_SpaceMultiplier < 1f)
					{
                        //var isRowHousing = true;

                        //for (var j = 0; j < spawnableBuildings.Length; j++)
                        //{
                        //	if (spawnableBuildings[j].m_ZonePrefab == zone && buildingsData[j].m_LotSize.x >= 2)
                        //	{
                        //		isRowHousing = false;
                        //		break;
                        //	}
                        //}

                        //dictionary[zone] = isRowHousing ? ZoneTypeFilter.Row : ZoneTypeFilter.Medium;

						if (!(info.m_AllowedSold.ToString() == "NoResource" && info.m_AllowedManufactured.ToString() == "NoResource" && info.m_AllowedStored.ToString() == "NoResource"))
						{
							dictionary[zone] = ZoneTypeFilter.Mixed;
                            ProcessMovingAssets(entity);
#if DEBUG
                            Log.Info($"{zoneName} set to Mixed");
#endif
                            continue;
						}

                            dictionary[zone] = ZoneTypeFilter.Medium;
                        ProcessMovingAssets(entity);
#if DEBUG
                        Log.Info($"{zoneName} set to Medium");
#endif
                        continue;
                    }
					dictionary[zone] = ZoneTypeFilter.High;
                    ProcessMovingAssets(entity);
#if DEBUG
                    Log.Info($"{zoneName} set to High");
#endif
                }
            }

			_zoneTypeCache = dictionary;
			Log.Info("Zones have been indexed and edited");
        }

		public static ZoneTypeFilter GetZoneType(Entity zoneEntity)
		{
			if (_zoneTypeCache != null && _zoneTypeCache.TryGetValue(zoneEntity, out var type))
			{
				return type;
			}

			return ZoneTypeFilter.Any;
		}

        public void ProcessMovingAssets(Entity entity)
        {
            ZoneTypeFilter ztf = GetZoneType(entity);
			Entity tab = Entity.Null;

            if (ztf.Equals(ZoneTypeFilter.Low))
            {
                tab = CreateUIAssetCategoryPrefab("Low Density Residential", "Zones", "Media/Game/Icons/ZoneResidentialLow.svg", 1);
            }
			else if (ztf.Equals(ZoneTypeFilter.Medium))
            {
                tab =
            CreateUIAssetCategoryPrefab("Medium Density Residential", "Zones",
                "Media/Game/Icons/ZoneResidentialMedium.svg", 2);
            }
            else if (ztf.Equals(ZoneTypeFilter.High))
			{
				tab = CreateUIAssetCategoryPrefab("High Density Residential", "Zones", "Media/Game/Icons/ZoneResidentialHigh.svg", 3);
            }
            else if (ztf.Equals(ZoneTypeFilter.Mixed))
            {
                tab = CreateUIAssetCategoryPrefab("Mixed Housing", "Zones", "Media/Game/Icons/ZoneResidentialMixed.svg", 4);
            }

            if (tab == Entity.Null) return;

            try
            {
                var name = _prefabSystem.GetPrefabName(entity);
                if (!EntityManager.TryGetComponent(entity, out PrefabData assetPrefabData) || !_prefabSystem.TryGetPrefab(assetPrefabData, out PrefabBase assetPrefabBase))
                    return;
                if (!_prefabSystem.TryGetComponentData(assetPrefabBase, out UIObjectData assetUIObject))
                    return;

                try
                {
                    Entity currentTab = assetUIObject.m_Group;
                    int currentPriority = assetUIObject.m_Priority;
                    if (!zoneAssetMenuData.Menu.ContainsKey(name))
                    {
                        zoneAssetMenuData.Menu.Add(name, currentTab);
                        zoneAssetMenuData.Priority.Add(name, currentPriority);
                    }

                    if (EntityManager.TryGetComponent(currentTab, out PrefabData currentTabPrefabData) && _prefabSystem.TryGetPrefab(currentTabPrefabData, out PrefabBase currentTabPrefabBase) && _prefabSystem.TryGetComponentData(currentTabPrefabBase, out UIObjectData currentTabUIObject))
                    {
                        RefreshBuffer(currentTab, tab, name, entity);
                        int newPriority = (currentTabUIObject.m_Priority * 1000) + currentPriority;
                        assetUIObject.m_Priority = newPriority;
                        assetUIObject.m_Group = tab;
                        EntityManager.SetComponentData(entity, assetUIObject);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            
        }

		public void RefreshBuffer(Entity oldCat, Entity newCat, string moverName, Entity moverEntity)
		{
			DynamicBuffer<UIGroupElement> uiGroupElementbuffer = EntityManager.GetBuffer<UIGroupElement>(oldCat);

			var itemName = _prefabSystem.GetPrefabName(moverEntity);
			var tabNameOld = _prefabSystem.GetPrefabName(oldCat);
			for (int i = 0; i < uiGroupElementbuffer.Length; i++)
			{
				var uge = uiGroupElementbuffer[i].m_Prefab;
				var tabName = _prefabSystem.GetPrefabName(uge);
				if (tabName == moverName)
				{
					uiGroupElementbuffer.RemoveAt(i);
#if DEBUG
                    Log.Info($"Removing {itemName} from {tabNameOld}");
#endif
                    break;
				}
			}

			var tabNameNew = _prefabSystem.GetPrefabName(newCat);
			EntityManager.GetBuffer<UIGroupElement>(newCat).Add(new UIGroupElement(moverEntity));
			EntityManager.GetBuffer<UnlockRequirement>(newCat).Add(new UnlockRequirement(moverEntity, UnlockFlags.RequireAny));
#if DEBUG
			Log.Info($"Adding {itemName} to {tabNameNew}");
#endif
		}
    }
}