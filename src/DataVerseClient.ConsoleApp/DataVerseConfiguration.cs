using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVerseClient.ConsoleApp
{
    public class DataVerseConfiguration
    {
        public string? Url { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public bool UseUniqueInstance { get; set; }

        public bool IsValid() =>
            !string.IsNullOrWhiteSpace(Url) && 
            !string.IsNullOrWhiteSpace(ClientId) && 
            !string.IsNullOrWhiteSpace(ClientSecret);
    }
}
