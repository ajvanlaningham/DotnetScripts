using ClassLibrary.Classes.GQLObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class PurchasingEntityHelper : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(OrderByIDResponse.PurchasingEntity).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        var typename = jo["__typename"]?.Value<string>();

        OrderByIDResponse.PurchasingEntity target;

        switch (typename)
        {
            case "PurchasingCompany":
                target = new OrderByIDResponse.PurchasingCompanyEntity();
                break;
            case "Customer":
                target = new OrderByIDResponse.PurchasingCustomerEntity();
                break;
            default:
                throw new JsonSerializationException($"Unknown __typename value: {typename}");
        }

        serializer.Populate(jo.CreateReader(), target);
        return target;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException("Serialization not implemented");
    }
}
