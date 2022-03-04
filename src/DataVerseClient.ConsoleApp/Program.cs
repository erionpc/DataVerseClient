using DataVerseClient.ConsoleApp;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

const string brhub_apphubinvite = "brhub_apphubinvite";
const string adx_externalidentity = "adx_externalidentity";
const string adx_username = "adx_username";
const string brhub_name = "brhub_name";
const string brhub_username = "brhub_username";
const string brhub_applicationid = "brhub_applicationid";
const string brhub_userid = "brhub_userid";
const string brhub_expirydate = "brhub_expirydate";
const string brhub_revokeinvitation = "brhub_revokeinvitation";

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

    var serviceClient = new ServiceClient(new Uri(dataVerseConfig.Url ?? ""), dataVerseConfig.ClientId, dataVerseConfig.ClientSecret, dataVerseConfig.UseUniqueInstance);

    Console.WriteLine($"Connected to the DataVerse web-API: {serviceClient.ConnectedOrgFriendlyName}");

    var whoIAm = (WhoAmIResponse)serviceClient.Execute(new WhoAmIRequest());
    Console.WriteLine($"as user id: {whoIAm.UserId}");

    while (true)
    {
        try
        {
            Console.WriteLine("Available operations:");
            Console.WriteLine("1. Get users");
            Console.WriteLine("2. Get invitations");
            Console.WriteLine("3. Get user by id");
            Console.WriteLine("4. Get invitation by email address and app id");
            Console.WriteLine("5. Upsert invitation");
            Console.WriteLine("6. Revoke invitation");
            Console.WriteLine("7. Delete invitation");
            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.WriteLine($"{Environment.NewLine}Getting list of users...");
                    var users = await GetUsers(serviceClient);
                    PrintEntities(users);
                    break;

                case "2":
                    Console.WriteLine($"{Environment.NewLine}Getting list of invitations...");
                    var invitations = await GetInvitations(serviceClient);
                    PrintEntities(invitations);
                    break;

                case "3":
                    Console.WriteLine($"{Environment.NewLine}Getting user by id...");
                    string? userId = "";
                    while (string.IsNullOrWhiteSpace(userId))
                    {
                        AcquireParameter("user id", ref userId);
                    }
                    var user = await GetUserByUserId(serviceClient, userId);
                    PrintEntity(user!);
                    break;

                case "4":
                    Console.WriteLine($"{Environment.NewLine}Getting invitation by email address and app id...");
                    string? emailAddress = "";
                    string? appId = "";
                    while (string.IsNullOrWhiteSpace(emailAddress)
                        || string.IsNullOrWhiteSpace(appId))
                    {
                        AcquireParameter("email address", ref emailAddress);
                        AcquireParameter("app id", ref appId);
                    }
                    var invitation = await GetInvitationByEmailAddressAndAppId(serviceClient, emailAddress, appId);
                    PrintEntity(invitation!);
                    break;

                case "5":
                    Console.WriteLine($"{Environment.NewLine}Upserting invitation...");
                    string? upsert_name = "";
                    string? upsert_appName = "";
                    string? upsert_email = "";
                    string? upsert_userId = "";
                    string? upsert_appId = "";
                    while (string.IsNullOrWhiteSpace(upsert_name) 
                        || string.IsNullOrWhiteSpace(upsert_appName)
                        || string.IsNullOrWhiteSpace(upsert_email) 
                        || string.IsNullOrWhiteSpace(upsert_appId))
                    {
                        AcquireParameter("name", ref upsert_name);
                        AcquireParameter("app name", ref upsert_appName);
                        AcquireParameter("email address", ref upsert_email);
                        AcquireParameter("user id", ref upsert_userId, optional: true);
                        AcquireParameter("app id", ref upsert_appId);
                    }

                    await UpsertInvitation(serviceClient, new Invitation(upsert_name, upsert_appName, upsert_email, upsert_userId, upsert_appId));
                    //await CreateOrUpdateInvitation(serviceClient, new Invitation(upsert_name, upsert_appName, upsert_email, upsert_userId, upsert_appId));
                    Console.WriteLine($"Invitation upserted{Environment.NewLine}");

                    break;

                case "6":
                    Console.WriteLine($"{Environment.NewLine}Revoking invitation...");
                    string? revoke_name = "";
                    string? revoke_appName = "";
                    string? revoke_email = "";
                    string? revoke_userId = "";
                    string? revoke_appId = "";
                    while (string.IsNullOrWhiteSpace(revoke_name)
                        || string.IsNullOrWhiteSpace(revoke_appName)
                        || string.IsNullOrWhiteSpace(revoke_email)
                        || string.IsNullOrWhiteSpace(revoke_userId)
                        || string.IsNullOrWhiteSpace(revoke_appId))
                    {
                        AcquireParameter("name", ref revoke_name);
                        AcquireParameter("app name", ref revoke_appName);
                        AcquireParameter("email address", ref revoke_email);
                        AcquireParameter("user id", ref revoke_userId, optional: true);
                        AcquireParameter("app id", ref revoke_appId);
                    }

                    await UpsertInvitation(serviceClient, new Invitation(revoke_name, revoke_appName, revoke_email, revoke_userId, revoke_appId, revoked: true));
                    //await RevokeInvitation(serviceClient, new Invitation(revoke_name, revoke_appName, revoke_email, revoke_userId, revoke_appId, revoked: true));
                    Console.WriteLine($"Invitation revoked{Environment.NewLine}");

                    break;

                case "7":
                    Console.WriteLine($"{Environment.NewLine}Deleting invitation...");
                    string? id = "";
                    while (string.IsNullOrWhiteSpace(id))
                    {
                        AcquireParameter("id", ref id);
                    }

                    await DeleteInvitation(serviceClient, new Guid(id));
                    Console.WriteLine($"Invitation deleted{Environment.NewLine}");

                    break;

                default:
                    throw new Exception("Select 1-7");
            }
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

Guid CreateInvitation(ServiceClient serviceClient, Invitation invitation)
{
    Dictionary<string, DataverseDataTypeWrapper> attributes = new()
    {
        { brhub_name, new DataverseDataTypeWrapper(invitation.UserNameApp, DataverseFieldType.String) },
        { brhub_username, new DataverseDataTypeWrapper(invitation.EmailAddress, DataverseFieldType.String) },
        { brhub_applicationid, new DataverseDataTypeWrapper(invitation.AppId, DataverseFieldType.String) },
    };

    if (string.IsNullOrWhiteSpace(invitation.UserId))
    {
        attributes.Add(brhub_userid, new DataverseDataTypeWrapper(invitation.UserId, DataverseFieldType.String));
    }

    return CreateInvitationRecord(serviceClient, attributes);
}

void AcquireParameter(string parameterName, ref string parameterValue, bool optional = false)
{
    Console.WriteLine($"Insert {parameterName}: ");
    parameterValue = Console.ReadLine()!;
    if (string.IsNullOrEmpty(parameterValue) && !optional)
        throw new Exception($"Please insert a valid {parameterName}");
}

void PrintEntities(EntityCollection entityCollection)
{
    Console.WriteLine(entityCollection.Print());
}

void PrintEntity(Entity entity)
{
    Console.WriteLine(entity.Print());
}

Task<EntityCollection> GetUsers(ServiceClient serviceClient) =>
    GetEntities(serviceClient, adx_externalidentity);

Task<EntityCollection> GetInvitations(ServiceClient serviceClient) =>
    GetEntities(serviceClient, brhub_apphubinvite);

Task<EntityCollection> GetEntities(ServiceClient serviceClient, string entityName)
{
    return serviceClient.RetrieveMultipleAsync(new QueryExpression()
    {
        EntityName = entityName,
        ColumnSet = new ColumnSet(true),
        TopCount = 10
    });
}

Task<EntityCollection> GetEntityByCriteria(ServiceClient serviceClient, string entityName, List<Tuple<string, ConditionOperator, string>> filters)
{
    var query = new QueryExpression(entityName)
    {
        ColumnSet = new ColumnSet(true),
        Criteria = new FilterExpression(LogicalOperator.And),
        TopCount = 1
    };

    foreach (var filter in filters)
    {
        query.Criteria.AddCondition(filter.Item1, filter.Item2, filter.Item3);
    }

    return serviceClient.RetrieveMultipleAsync(query);
}

async Task<Entity?> GetUserByUserId(ServiceClient serviceClient, string userId) 
{ 
    var entities = await GetEntityByCriteria(serviceClient, adx_externalidentity, new List<Tuple<string, ConditionOperator, string>>() 
    {
        new(adx_username, ConditionOperator.Equal, userId)
    });

    if (!entities.Entities.Any())
        throw new Exception("User not found");

    return entities.Entities.FirstOrDefault();
}

async Task<Entity?> GetInvitationByEmailAddressAndAppId(ServiceClient serviceClient, string emailAddress, string appId)
{
    var entities = await GetEntityByCriteria(serviceClient, brhub_apphubinvite, new List<Tuple<string, ConditionOperator, string>>()
    {
        new(brhub_username, ConditionOperator.Equal, emailAddress),
        new(brhub_applicationid, ConditionOperator.Equal, appId)
    });

    if (!entities.Entities.Any())
        throw new Exception("Invitation not found");

    return entities.Entities.FirstOrDefault();
}

Guid CreateInvitationRecord(ServiceClient serviceClient, Dictionary<string, DataverseDataTypeWrapper> properties)
{
    return serviceClient.CreateNewRecord(brhub_apphubinvite, properties);
}

async Task<Entity?> GetInvitation(ServiceClient serviceClient, Invitation invitation)
{
    var entities = await GetEntityByCriteria(serviceClient, brhub_apphubinvite, new List<Tuple<string, ConditionOperator, string>>()
    {
        new(brhub_username, ConditionOperator.Equal, invitation.EmailAddress),
        new(brhub_applicationid, ConditionOperator.Equal, invitation.AppId)
    });

    if (!entities.Entities.Any())
        throw new Exception("Invitation not found");

    return entities.Entities.FirstOrDefault();
}

Task UpsertInvitation(ServiceClient serviceClient, Invitation invitation)
{
    KeyAttributeCollection keyAttributes = new()
    {
        { brhub_applicationid, invitation.AppId },
        { brhub_username, invitation.EmailAddress }
    };

    Entity invitationEntity = new(brhub_apphubinvite, keyAttributes);
    invitationEntity[brhub_name] = invitation.UserNameApp;
    invitationEntity[brhub_expirydate] = invitation.ExpiryDate;

    if (!string.IsNullOrWhiteSpace(invitation.UserId))
    {
        invitationEntity[brhub_userid] = invitation.UserId;
    }

    if (invitation.IsActive)
    {
        invitationEntity.Attributes[brhub_revokeinvitation] = new OptionSetValue(int.Parse(Invitation.STATUS_ACTIVE));
    }
    else if (invitation.IsRevoked)
    {
        invitationEntity.Attributes[brhub_revokeinvitation] = new OptionSetValue(int.Parse(Invitation.STATUS_REVOKED));
    }

    UpsertRequest request = new()
    {
        Target = invitationEntity
    };

    return serviceClient.ExecuteAsync(request);
}

async Task CreateOrUpdateInvitation(ServiceClient serviceClient, Invitation invitation)
{
    var entity = await GetInvitation(serviceClient, invitation);
    if (entity == null)
    {
        CreateInvitation(serviceClient, invitation);
    }
    else
    {
        await UpdateInvitation(serviceClient, invitation).ConfigureAwait(false);
    }
}

async Task UpdateInvitation(ServiceClient serviceClient, Invitation invitation)
{
    var entity = await GetInvitation(serviceClient, invitation);
    if (entity == null)
        throw new Exception("Invitation not found");

    entity[brhub_expirydate] = DateTime.UtcNow.AddDays(7);
    entity[brhub_revokeinvitation] = Invitation.STATUS_ACTIVE;

    await serviceClient.UpdateAsync(entity).ConfigureAwait(false);
}

async Task RevokeInvitation(ServiceClient serviceClient, Invitation invitation)
{
    var entity = await GetInvitation(serviceClient, invitation);
    if (entity == null)
        throw new Exception("Invitation not found");

    entity[brhub_revokeinvitation] = Invitation.STATUS_REVOKED;

    await serviceClient.UpdateAsync(entity).ConfigureAwait(false);
}

Task DeleteInvitation(ServiceClient serviceClient, Guid id)
{
    return serviceClient.DeleteAsync(brhub_apphubinvite, id);
}

#region old attempts
//async Task CreateNewInviteUsingWebClient(ServiceClient serviceClient)
//{    
//    string url = $"https://xxxxxxx.xxxxxxxxx/xxxxxxxxx";

//    string body = "{\"brhub_name\":\"xxxxxxx:xxxxxx\"," +
//                   "\"brhub_username\":\"xxxxxxx@xxxxxxxx\"," +
//                   "\"brhub_userid\":\"xxxxxxxxxxxxxxx\"," +
//                   "\"brhub_applicationid\":\"xxxxxxxxxxxxx\"}";

//    Dictionary<string, List<string>> headers = new()
//    {
//        { "Content-Type", new List<string>() { "application/json" } },
//        { "OData-Version", new List<string>() { "4.0" } },
//        { "Accept", new List<string>() { "application/json" } }
//    };

//    var resp = serviceClient.ExecuteWebRequest(HttpMethod.Post, url, body, headers);
//    var rspContent = await resp.Content.ReadAsStringAsync();
//}

//Guid CreateNewInviteUsingCreateNewRecord(ServiceClient serviceClient)
//{
//    return serviceClient.CreateNewRecord(brhub_apphubinvite, new Dictionary<string, DataverseDataTypeWrapper>()
//    {
//        { brhub_name, new DataverseDataTypeWrapper("xxxxxx@xxxxxxx:xxxxxxxxx", DataverseFieldType.String) },
//        { brhub_username, new DataverseDataTypeWrapper("xxxx@xxxx", DataverseFieldType.String) },
//        { brhub_userid, new DataverseDataTypeWrapper("", DataverseFieldType.String) },
//        { brhub_applicationid, new DataverseDataTypeWrapper("xxxxxxxx", DataverseFieldType.String) },
//    });
//}

//Task<EntityCollection> GetUserByEmail(ServiceClient serviceClient, string emailAddress)
//{

//    var query = new QueryExpression(adx_externalidentity)
//    {
//        ColumnSet = new ColumnSet(true),
//        Criteria = new FilterExpression(LogicalOperator.And),
//        TopCount = 1
//    };
//    query.Criteria.AddCondition("adx_email", ConditionOperator.Equal, emailAddress);

//    var entitiesFound = serviceClient.RetrieveMultipleAsync(query);

//    return entitiesFound;
//}

//Task<EntityCollection> GetInvitationsOld(ServiceClient serviceClient)
//{
//    var query = new QueryExpression(brhub_apphubinvite)
//    {
//        ColumnSet = new ColumnSet(true),
//        Criteria = new FilterExpression(LogicalOperator.And),
//        TopCount = 10
//    };

//    var entitiesFound = serviceClient.RetrieveMultipleAsync(query);

//    return entitiesFound;
//}
#endregion