using System;
using Newtonsoft.Json;
using UnityEngine;

namespace CommonAssets.Scripts.ArtilleryShelling;

[Serializable]
public struct ArtilleryShellingMapConfiguration
{
	public bool PlanedShellingOn;

	public float InitShellingTimer;

	public float InitCalledShellingTime;

	public float BeforeShellingSignalTime;

	public int ShellingCount;

	public int ZonesInShelling;

	public bool NewZonesForEachShelling;

	public ArtilleryShellingZone[] ShellingZones;

	public ArtilleryBrigade[] Brigades;

	public Vector2 PauseBetweenShellings;

	[JsonProperty("ArtilleryShellingAirDropSettings")]
	public BackendConfigSettingsClass.ArtilleryShellingAirDropSettings ArtilleryShellingAirDropSettings;
}
