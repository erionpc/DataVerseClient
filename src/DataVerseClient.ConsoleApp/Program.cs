using DataVerseApiSample.ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

try
{
    IConfiguration config =
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddUserSecrets<Program>()
            .Build();

    var dataVerseConfig = new DataVerseConfiguration();
    config.GetSection("DataVerseConfiguration").Bind(dataVerseConfig);

    if (!dataVerseConfig.IsValid())
        throw new Exception("Invalid configuration");

    var serviceClient = new ServiceClient(new Uri(dataVerseConfig.Url ?? ""), dataVerseConfig.ClientId, dataVerseConfig.ClientSecret, true);

    Console.WriteLine($"Connected to the DataVerse web-API: {serviceClient.ConnectedOrgFriendlyName}");

    while (true)
    {
        try
        {
            Console.WriteLine($"{System.Environment.NewLine}Write name of entity to query:");
            string entityName = Console.ReadLine() ?? "";
            Console.WriteLine($"Querying entity: {entityName}{System.Environment.NewLine}");

            var entitiesFound = await serviceClient.RetrieveMultipleAsync(new QueryExpression()
            {
                EntityName = entityName,
                TopCount = 10
            });

            Console.WriteLine($"Found: {entitiesFound.TotalRecordCount}");

            Console.WriteLine(string.Join(System.Environment.NewLine,
                entitiesFound.Entities));
            
            Console.WriteLine();
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
