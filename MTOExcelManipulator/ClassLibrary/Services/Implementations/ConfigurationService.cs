using ClassLibrary.Classes;
using Microsoft.Extensions.Configuration;

public class ConfigurationService
{
    private readonly IConfiguration _configuration;

    public ConfigurationService(string configFileName = "appsettings.json")
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(configFileName, optional: false, reloadOnChange: true);

        _configuration = builder.Build();
    }

    public ShopifyAdminAPISettings GetShopifySettings()
    {
        string environment = GetValue("Environment");
        string sectionName = environment == "Production" ? "ShopifySettings" : "DevShopifySettings";

        return GetSection<ShopifyAdminAPISettings>(sectionName);
    }

    public T GetSection<T>(string sectionName) where T : new()
    {
        var section = new T();
        _configuration.GetSection(sectionName).Bind(section);
        return section;
    }

    public string GetValue(string key)
    {
        return _configuration[key];
    }
}
