using System.IO;

public abstract class GClass1451
{
	public static void SerializeNode(BinaryWriter writer, GClass1448 node)
	{
		writer.Write((byte)node.NodeType);
		node.Serialize(writer);
		writer.Write(node.Childs.Count);
		for (int i = 0; i < node.Childs.Count; i++)
		{
			SerializeNode(writer, node.Childs[i]);
		}
	}

	public static GClass1448 DeserializeNode(BinaryReader reader, GInterface135 paramsKeeper)
	{
		GClass1448 gClass = smethod_0((GClass1448.EComputedNodeType)reader.ReadByte(), paramsKeeper);
		gClass.Deserialize(reader);
		int num = reader.ReadInt32();
		gClass.Childs.Clear();
		for (int i = 0; i < num; i++)
		{
			gClass.Childs.Add(DeserializeNode(reader, paramsKeeper));
		}
		gClass.ChildrensInitialized();
		return gClass;
	}

	public static GClass1448 smethod_0(GClass1448.EComputedNodeType nodeType, GInterface135 paramsKeeper)
	{
		return nodeType switch
		{
			GClass1448.EComputedNodeType.Tree => new GClass1450(paramsKeeper), 
			GClass1448.EComputedNodeType.Clip => new GClass1449(paramsKeeper), 
			_ => null, 
		};
	}
}
