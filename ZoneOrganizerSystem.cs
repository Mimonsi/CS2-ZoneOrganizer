using System;
using System.Collections.Generic;
using Colossal.Entities;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.Prefabs;
using Game.Zones;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ZoneOrganizer
{
	public enum ZoneTypeFilter
	{
		Any = 0,
		ResiLow = 1,
		ResiRow = 2,
		ResiMedium = 4,
		ResiHigh = 8,
		Mixed = 16,
        CommLow = 32,
        CommHigh = 64,
        OfficeLow = 128,
        OfficeHigh = 256,
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
		private static Dictionary<Entity, ZoneTypeFilter> _zoneTypeCache = new();
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

			Enabled = true;
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
				var menuUI = menuPrefab.AddComponent<UIObject>();
				menuUI.m_Icon = icon;
				menuUI.m_Priority = priority;
				menuUI.active = true;
				menuUI.m_IsDebugObject = false;
				menuUI.m_Group = null;
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
			
            var entities = _zoneQuery.ToEntityArray(Allocator.Temp);
            Log.Info("Zone query results: " + entities.Length);
            foreach (Entity entity in entities)
            {
				var zone = entity;
				if (EntityManager.TryGetComponent(zone, out PrefabData prefabData) && _prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase) &&
                _prefabSystem.TryGetComponentData(prefabBase, out ZonePropertiesData zoneProp) &&
                _prefabSystem.TryGetComponentData(prefabBase, out ZoneData zoneData))
                {
                    string zoneName = _prefabSystem.GetPrefabName(zone);
					ZoneDensity zd = Game.Buildings.PropertyUtils.GetZoneDensity(zoneData, zoneProp);

                    if (zoneData.m_AreaType == AreaType.Residential)
                    {
                        switch (zd)
                        {
                            case ZoneDensity.Low:
	                            _zoneTypeCache[zone] = ZoneTypeFilter.ResiLow;
                                break;
                            case ZoneDensity.Medium:
                                if (!(zoneProp.m_AllowedSold.ToString() == "NoResource" &&
                                    zoneProp.m_AllowedManufactured.ToString() == "NoResource"
                                    && zoneProp.m_AllowedStored.ToString() == "NoResource"))
                                {
	                                _zoneTypeCache[zone] = ZoneTypeFilter.Mixed;
                                    break;
                                }
                                _zoneTypeCache[zone] = ZoneTypeFilter.ResiMedium;
                                break;
                            case ZoneDensity.High:
	                            _zoneTypeCache[zone] = ZoneTypeFilter.ResiHigh;
                                break;
                            default:
                                break;
                        }
                        ProcessMovingAssets(entity);
                    }
                    else if (zoneData.m_AreaType == AreaType.Commercial)
                    {
                        switch (zd)
                        {
                            case ZoneDensity.Low:
	                            _zoneTypeCache[zone] = ZoneTypeFilter.CommLow;
                                break;
                            case ZoneDensity.High:
	                            _zoneTypeCache[zone] = ZoneTypeFilter.CommHigh;
                                break;
                            default:
                                break;
                        }
                        ProcessMovingAssets(entity);
                    }
                    else
                    {
                        if (zoneData.m_AreaType != AreaType.Industrial)
                        {
                            continue;
                        }
                        if (!zoneData.IsOffice())
                        {
                            continue;
                        }
                        switch (zd)
                        {
                            case ZoneDensity.Low:
	                            _zoneTypeCache[zone] = ZoneTypeFilter.OfficeLow;
                                break;
                            case ZoneDensity.High:
	                            _zoneTypeCache[zone] = ZoneTypeFilter.OfficeHigh;
                                break;
                            default:
                                break;
                        }
                        ProcessMovingAssets(entity);
                    }
                    //					if (zoneProp.m_ResidentialProperties <= 0f)
                    //                    {
                    //#if DEBUG
                    //                        Log.Info($"{zoneName} is skipped");
                    //#endif
                    //                        continue;
                    //					}

                    //					if (!zoneProp.m_ScaleResidentials)
                    //					{
                    //						dictionary[zone] = ZoneTypeFilter.Low;
                    //                        ProcessMovingAssets(entity);
                    //#if DEBUG
                    //                        Log.Info($"{zoneName} set to Low");
                    //#endif
                    //                        continue;
                    //					}

                    //					if (zoneProp.m_ResidentialProperties / zoneProp.m_SpaceMultiplier < 1f)
                    //					{
                    //                        //var isRowHousing = true;

                    //                        //for (var j = 0; j < spawnableBuildings.Length; j++)
                    //                        //{
                    //                        //	if (spawnableBuildings[j].m_ZonePrefab == zone && buildingsData[j].m_LotSize.x >= 2)
                    //                        //	{
                    //                        //		isRowHousing = false;
                    //                        //		break;
                    //                        //	}
                    //                        //}

                    //                        //dictionary[zone] = isRowHousing ? ZoneTypeFilter.Row : ZoneTypeFilter.Medium;

                    //						if (!(zoneProp.m_AllowedSold.ToString() == "NoResource" && zoneProp.m_AllowedManufactured.ToString() == "NoResource" && zoneProp.m_AllowedStored.ToString() == "NoResource"))
                    //						{
                    //							dictionary[zone] = ZoneTypeFilter.Mixed;
                    //                            ProcessMovingAssets(entity);
                    //#if DEBUG
                    //                            Log.Info($"{zoneName} set to Mixed");
                    //#endif
                    //                            continue;
                    //						}

                    //                            dictionary[zone] = ZoneTypeFilter.Medium;
                    //                        ProcessMovingAssets(entity);
                    //#if DEBUG
                    //                        Log.Info($"{zoneName} set to Medium");
                    //#endif
                    //                        continue;
                    //                    }
                    //					dictionary[zone] = ZoneTypeFilter.High;
                    //                    ProcessMovingAssets(entity);
                    //#if DEBUG
                    //                    Log.Info($"{zoneName} set to High");
                    //#endif
                }
            }

			//_zoneTypeCache = dictionary;
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

            if (ztf.Equals(ZoneTypeFilter.ResiLow))
            {
                tab = CreateUIAssetCategoryPrefab("Low Density Residential", "Zones", "Media/Game/Icons/ZoneResidentialLow.svg", 1);
            }
			else if (ztf.Equals(ZoneTypeFilter.ResiMedium))
            {
                tab = CreateUIAssetCategoryPrefab("Medium Density Residential", "Zones",
                "Media/Game/Icons/ZoneResidentialMedium.svg", 2);
            }
            else if (ztf.Equals(ZoneTypeFilter.ResiHigh))
			{
				tab = CreateUIAssetCategoryPrefab("High Density Residential", "Zones", "Media/Game/Icons/ZoneResidentialHigh.svg", 3);
            }
            else if (ztf.Equals(ZoneTypeFilter.Mixed))
            {
                tab = CreateUIAssetCategoryPrefab("Mixed Housing", "Zones", "Media/Game/Icons/ZoneResidentialMixed.svg", 4);
            }
            else if (ztf.Equals(ZoneTypeFilter.CommLow))
            {
                tab = CreateUIAssetCategoryPrefab("Low Density Commercial", "Zones", "Media/Game/Icons/ZoneCommercialLow.svg", 5);
            }
            else if (ztf.Equals(ZoneTypeFilter.CommHigh))
            {
                tab = CreateUIAssetCategoryPrefab("High Density Commercial", "Zones", "Media/Game/Icons/ZoneCommercialHigh.svg", 6);
            }
            else if (ztf.Equals(ZoneTypeFilter.OfficeLow))
            {
                tab = CreateUIAssetCategoryPrefab("Low Density Office", "Zones", "Media/Game/Icons/ZoneOfficeLow.svg", 31);
            }
            else if (ztf.Equals(ZoneTypeFilter.OfficeHigh))
            {
                tab = CreateUIAssetCategoryPrefab("High Density Office", "Zones", "Media/Game/Icons/ZoneOfficeHigh.svg", 32);
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
                    Log.Debug($"Removing {itemName} from {tabNameOld}");
                    break;
				}
			}

			var tabNameNew = _prefabSystem.GetPrefabName(newCat);
			EntityManager.GetBuffer<UIGroupElement>(newCat).Add(new UIGroupElement(moverEntity));
			EntityManager.GetBuffer<UnlockRequirement>(newCat).Add(new UnlockRequirement(moverEntity, UnlockFlags.RequireAny));
			Log.Debug($"Adding {itemName} to {tabNameNew}");
		}
    }
}