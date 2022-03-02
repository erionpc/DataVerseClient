using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVerseClient.ConsoleApp
{
    public class Invitation
    {
        public const string STATUS_ACTIVE = "100000000";
        public const string STATUS_REVOKED = "100000001";

        public string? UserName { get; set; }
        public string? AppName { get; set; }
        public string? EmailAddress { get; set; }
        public string? UserId { get; set; }
        public string? AppId { get; set; }
        public string? Status { get; set; } = STATUS_ACTIVE;
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public string? UserNameApp => $"{UserName}:{AppName}";
        public bool IsRevoked => Status == STATUS_REVOKED;
        public bool IsActive => Status == STATUS_ACTIVE;

        public Invitation()
        {

        }
        public Invitation(string? userName, string? appName, string? emailAddress, string? userId, string? appId, bool revoked = false)
        {
            UserName = userName;
            AppName = appName;
            EmailAddress = emailAddress;
            UserId = userId;
            AppId = appId;

            if (revoked)
            {
                Status = STATUS_REVOKED;
            }
        }

        public Invitation(string? userName, string? appName, string? emailAddress, string? userId, string? appId)
        {
            UserName = userName;
            AppName = appName;
            EmailAddress = emailAddress;
            UserId = userId;
            AppId = appId;
        }

        public Invitation(string? userName, string? appName, string? emailAddress, string? appId)
        {
            UserName = userName;
            AppName = appName;
            EmailAddress = emailAddress;
            AppId = appId;
        }

        public Invitation(string? emailAddress, string? appId)
        {
            EmailAddress = emailAddress;
            AppId = appId;
        }

        public void Revoke()
        {
            Status = STATUS_REVOKED;
        }
    }
}
