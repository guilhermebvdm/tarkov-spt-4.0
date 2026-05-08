using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;

public class FlatItemsDataClass
{
	public MongoID _id;

	public MongoID _tpl;

	[JsonIgnore]
	public MongoID? parentId;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string slotId;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public GClass846 location;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public GClass846 upd;

	[UsedImplicitly]
	[JsonProperty("parentId", NullValueHandling = NullValueHandling.Ignore)]
	public string String_0
	{
		get
		{
			MongoID? mongoID = parentId;
			if (!mongoID.HasValue)
			{
				return null;
			}
			return mongoID.GetValueOrDefault();
		}
		set
		{
			parentId = ((value == "hideout") ? (parentId = null) : (parentId = new MongoID(value)));
		}
	}

	public FlatItemsDataClass Clone(MongoID id, MongoID parent, string slot)
	{
		return new FlatItemsDataClass
		{
			_id = id,
			_tpl = _tpl,
			parentId = parent,
			slotId = slot,
			location = location,
			upd = upd
		};
	}
}
