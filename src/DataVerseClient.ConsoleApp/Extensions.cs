using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVerseClient.ConsoleApp
{
    public static class Extensions
    {
        public static string Print(this object o)
        {
            if (o is EntityCollection)
                return Print((EntityCollection)o);
            if (o is Entity)
                return Print((Entity)o);
            if (o is EntityReference)
                return Print((EntityReference)o);
            if (o is OptionSetValue)
                return Print(((OptionSetValue)o).Value);

            return o.ToString() ?? "";
        }

        public static string Print(this EntityCollection entityCollection)
        {
            string printString = $"{entityCollection.EntityName}{Environment.NewLine}-------------------------------{Environment.NewLine}";
            foreach (var entity in entityCollection.Entities)
            {
                printString += entity.Print();
            }

            return printString;
        }

        public static string Print(this Entity entity)
        {
            string printString = $"{nameof(entity.Id)}: {entity.Id}{Environment.NewLine}" +
                                 $"{nameof(entity.LogicalName)}: {entity.LogicalName}{Environment.NewLine}" +
                                 $"{nameof(entity.EntityState)}: {entity.EntityState}{Environment.NewLine}";

            foreach (var attribute in entity.Attributes)
            {
                printString += $"{attribute.Key}: {attribute.Value.Print()}{Environment.NewLine}";
            }
            printString += $"-------------------------------{Environment.NewLine}";

            return printString;
        }

        public static string Print(this EntityReference entity) =>
            entity.Name;

        public static string Print(this OptionSetValue option) =>
            option.Value.ToString();
    }
}
