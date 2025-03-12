using System.Collections.Generic;
using Colossal.Entities;
using Game;
using Game.Prefabs;
using Game.UI.Editor;
using Unity.Entities;
using UnityEngine;

namespace ZoneOrganizer;

public partial class ZoneOrganizerSystem : GameSystemBase
{
	
	private PrefabSystem _prefabSystem;
	private EntityQuery _uiAssetMenuDataQuery;
	private EntityQuery _uiAssetCategoryDataQuery;

	protected override void OnCreate()
	{
		
	}
	
    protected override void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
    
    public Entity CreateUIAssetCategoryPrefab(string name, string group, string icon, int priority)
    {

	    if (!_prefabSystem.TryGetPrefab(new PrefabID("UIAssetCategoryPrefab", name), out PrefabBase tab))
	    {

		    UIAssetCategoryPrefab menuPrefab = ScriptableObject.CreateInstance<UIAssetCategoryPrefab>();
		    menuPrefab.name = name;
		    EntityManager.TryGetComponent(assetMenuDataDict[group], out PrefabData prefabData);
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
    
    
    /*private static void IndexZones()
    {
    	var zonesQuery = QueryBuilder().WithAll<ZoneData, ZonePropertiesData, PrefabData>().Build();
    	var zones = zonesQuery.ToEntityArray(Allocator.Temp);
    	var propertiesData = zonesQuery.ToComponentDataArray<ZonePropertiesData>(Allocator.Temp);

    	var buildingsQuery = QueryBuilder().WithAll<BuildingData, SpawnableBuildingData, PrefabData>().WithNone<SignatureBuildingData>().Build();
    	var buildingsData = buildingsQuery.ToComponentDataArray<BuildingData>(Allocator.Temp);
    	var spawnableBuildings = buildingsQuery.ToComponentDataArray<SpawnableBuildingData>(Allocator.Temp);

    	var dictionary = new Dictionary<Entity, EditorAssetCategorySystem.ZoneTypeFilter>();

    	for (var i = 0; i < zones.Length; i++)
    	{
    		var zone = zones[i];
    		var info = propertiesData[i];

    		if (info.m_ResidentialProperties <= 0f)
    		{
    			dictionary[zone] = EditorAssetCategorySystem.ZoneTypeFilter.Any;
    			continue;
    		}

    		var ratio = info.m_ResidentialProperties / info.m_SpaceMultiplier;

    		if (!info.m_ScaleResidentials)
    		{
    			dictionary[zone] = EditorAssetCategorySystem.ZoneTypeFilter.Low;
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

    			dictionary[zone] = isRowHousing ? EditorAssetCategorySystem.ZoneTypeFilter.Row : EditorAssetCategorySystem.ZoneTypeFilter.Medium;
    		}
    		else
    		{
    			dictionary[zone] = EditorAssetCategorySystem.ZoneTypeFilter.High;
    		}
    	}

    	_zoneTypeCache = dictionary;
    }

    public static EditorAssetCategorySystem.ZoneTypeFilter GetZoneType(Entity zonePrefab)
    {
    	if (_zoneTypeCache != null && _zoneTypeCache.TryGetValue(zonePrefab, out var type))
    	{
    		return type;
    	}

    	return EditorAssetCategorySystem.ZoneTypeFilter.Any;
    }*/
}