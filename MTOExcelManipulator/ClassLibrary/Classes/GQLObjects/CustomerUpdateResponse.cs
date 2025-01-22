public class CustomerUpdateResponse
{
    public CustomerUpdatePayload CustomerUpdate { get; set; }
}

public class CustomerUpdatePayload
{
    public UpdatedCustomer Customer { get; set; }
    public List<UserError> UserErrors { get; set; }
}

public class UpdatedCustomer
{
    public string Id { get; set; }
    public MetafieldsConnection Metafields { get; set; }
}

public class MetafieldsConnection
{
    public List<MetafieldEdge> Edges { get; set; }
}

public class MetafieldEdge
{
    public Metafield Node { get; set; }
}

public class Metafield
{
    public string Namespace { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string Type { get; set; }
}

public class UserError
{
    public List<string> Field { get; set; }  // Shopify returns "field" as a list of strings
    public string Message { get; set; }
}
