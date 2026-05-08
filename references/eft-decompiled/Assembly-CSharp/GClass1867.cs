using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public abstract class GClass1867<T, U>
{
	public U _parent;

	public JToken _props;

	public abstract U TypeToKey(Type type);

	public abstract Type KeyToType(U type);

	public virtual T Deserialize(JsonReader reader, Type objectType, JsonSerializer serializer)
	{
		return (T)_props.ToObject(KeyToType(_parent), serializer);
	}

	public virtual void Serialize(JsonWriter writer, object value, JsonSerializer serializer)
	{
		_parent = TypeToKey(value.GetType());
		_props = JToken.FromObject(value, serializer);
		serializer.Serialize(writer, this);
	}

	public GClass1867()
	{
	}
}
