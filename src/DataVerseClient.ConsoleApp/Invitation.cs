using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVerseClient.ConsoleApp
{
    public class Invitation
    {
        public static string STATUS_ACTIVE = "100000000";
        public static string STATUS_REVOKED = "100000001";

        public string? UserName { get; set; }
        public string? AppName { get; set; }
        public string? EmailAddress { get; set; }
        public string? UserId { get; set; }
        public string? AppId { get; set; }
        public string? Status { get; set; }
        public string? UserNameApp => $"{UserName}:{AppName}";

        public Invitation()
        {

        }

        public Invitation(string? userName, string? appName, string? emailAddress, string? userId, string? appId)
        {
            UserName = userName;
            AppName = appName;
            EmailAddress = emailAddress;
            UserId = userId;
            AppId = appId;
            Status = STATUS_ACTIVE;
        }

        public Invitation(string? userName, string? appName, string? emailAddress, string? appId)
        {
            UserName = userName;
            AppName = appName;
            EmailAddress = emailAddress;
            AppId = appId;
            Status = STATUS_ACTIVE;
        }

        public Invitation(string? emailAddress, string? appId)
        {
            EmailAddress = emailAddress;
            AppId = appId;
            Status = STATUS_ACTIVE;
        }

        public void Revoke()
        {
            Status = STATUS_REVOKED;
        }
    }
}
