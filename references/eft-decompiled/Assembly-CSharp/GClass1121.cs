using System.IO;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1121
{
	public void Write(SerializableAudioRoutesBakeData data, string pathToSave)
	{
		if (File.Exists(pathToSave))
		{
			Debug.Log("Delete old bake data and create new");
			File.Delete(pathToSave);
		}
		using FileStream output = new FileStream(pathToSave, FileMode.Create);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		method_0(data, binaryWriter);
		method_1(data, binaryWriter);
		method_2(data, binaryWriter);
		data.BakeSettings.Write(binaryWriter);
		binaryWriter.Write(data.RoomPairMaxRoutesCapacity);
		binaryWriter.Write(data.RoomPairMaxPortalsCapacity);
	}

	public void method_0(SerializableAudioRoutesBakeData data, BinaryWriter writer)
	{
		writer.Write(data.RoomPairsItems.Length);
		RoomPairItem[] roomPairsItems = data.RoomPairsItems;
		for (int i = 0; i < roomPairsItems.Length; i++)
		{
			RoomPairItem roomPairItem = roomPairsItems[i];
			writer.Write(roomPairItem.Key);
			roomPairItem.Value.Write(writer);
		}
	}

	public void method_1(SerializableAudioRoutesBakeData data, BinaryWriter writer)
	{
		writer.Write(data.RoutesAwareRoomItems.Length);
		RoutesAwareRoomItem[] routesAwareRoomItems = data.RoutesAwareRoomItems;
		for (int i = 0; i < routesAwareRoomItems.Length; i++)
		{
			RoutesAwareRoomItem routesAwareRoomItem = routesAwareRoomItems[i];
			writer.Write(routesAwareRoomItem.Key);
			smethod_0(writer, routesAwareRoomItem.Value);
		}
	}

	public void method_2(SerializableAudioRoutesBakeData data, BinaryWriter writer)
	{
		writer.Write(data.RoutesByRoomPairIDItems.Length);
		RoutesByRoomPairIDItem[] routesByRoomPairIDItems = data.RoutesByRoomPairIDItems;
		for (int i = 0; i < routesByRoomPairIDItems.Length; i++)
		{
			RoutesByRoomPairIDItem routesByRoomPairIDItem = routesByRoomPairIDItems[i];
			writer.Write(routesByRoomPairIDItem.roomPairID);
			smethod_1(writer, routesByRoomPairIDItem.routesData);
		}
	}

	public static void smethod_0(BinaryWriter writer, uint[] array)
	{
		writer.Write(array.Length);
		foreach (uint value in array)
		{
			writer.Write(value);
		}
	}

	public static void smethod_1(BinaryWriter writer, AudioRouteData[] audioRoutes)
	{
		writer.Write(audioRoutes.Length);
		foreach (AudioRouteData audioRouteData in audioRoutes)
		{
			audioRouteData.Write(writer);
		}
	}
}
