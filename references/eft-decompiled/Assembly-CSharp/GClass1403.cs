using Comfort.Common;
using EFT;

public class GClass1403 : ISerializer<LootItemPositionClass>, ISerializer<GClass1402>
{
	public string Id;

	public ClassVector3 Position;

	public ClassVector3 Rotation;

	public FlatItemsDataClass[] Items;

	public string Root;

	public bool IsContainer;

	public MongoID[] ValidProfiles;

	public bool useGravity;

	public bool randomRotation;

	public GClass2197 Customization;

	public EPlayerSide Side;

	public ClassTransformSync[] Bones;

	public string CorpseProfileID;

	public bool IsZombieCorpse;

	public ClassVector3 Shift = new ClassVector3();

	public LootItemPositionClass Deserialize()
	{
		LootItemPositionClass lootItemPositionClass = ((Customization == null) ? new LootItemPositionClass() : new GClass1402
		{
			Customization = Customization,
			Side = Side,
			Bones = ClassTransformSync.ToUnity(Bones),
			ProfileID = CorpseProfileID,
			IsZombieCorpse = IsZombieCorpse
		});
		lootItemPositionClass.Item = ((Items != null) ? Singleton<ItemFactoryClass>.Instance.FlatItemsToTree(Items).Items[Root] : null);
		lootItemPositionClass.Id = Id;
		lootItemPositionClass.Position = Position.ToUnityVector3();
		lootItemPositionClass.Rotation = Rotation.ToUnityVector3();
		lootItemPositionClass.IsContainer = IsContainer;
		lootItemPositionClass.ValidProfiles = ValidProfiles;
		lootItemPositionClass.useGravity = useGravity;
		lootItemPositionClass.randomRotation = randomRotation;
		lootItemPositionClass.Shift = Shift;
		return lootItemPositionClass;
	}

	LootItemPositionClass ISerializer<LootItemPositionClass>.Deserialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in Deserialize
		return this.Deserialize();
	}

	public object Serialize(GClass1402 t)
	{
		((ISerializer<LootItemPositionClass>)this).Serialize((LootItemPositionClass)t);
		Customization = t.Customization;
		Side = t.Side;
		CorpseProfileID = t.ProfileID;
		Bones = ClassTransformSync.FromUnity(t.Bones);
		return this;
	}

	object ISerializer<GClass1402>.Serialize(GClass1402 t)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Serialize
		return this.Serialize(t);
	}

	public object Serialize_1(LootItemPositionClass t)
	{
		Id = t.Id;
		Position = t.Position;
		Rotation = t.Rotation;
		Root = t.Item.Id;
		Items = Singleton<ItemFactoryClass>.Instance.TreeToFlatItems(t.Item);
		IsContainer = t.IsContainer;
		ValidProfiles = t.ValidProfiles;
		useGravity = t.useGravity;
		randomRotation = t.randomRotation;
		Shift = t.Shift;
		return this;
	}

	object ISerializer<LootItemPositionClass>.Serialize(LootItemPositionClass t)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Serialize_1
		return this.Serialize_1(t);
	}

	public GClass1402 Deserialize_1()
	{
		return ((ISerializer<GClass1402>)this).Deserialize();
	}

	GClass1402 ISerializer<GClass1402>.Deserialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in Deserialize_1
		return this.Deserialize_1();
	}
}
