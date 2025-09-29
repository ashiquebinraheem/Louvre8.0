using Louvre.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Louvre.Helpers
{
    public static class SearchValidationHelper
    {
        public static void ValidateSearchData(string searchColumnName, string orderByFieldName, IEnumerable<string> validFields)
        {
            ValidateField(searchColumnName, validFields, "Invalid Search Column Name");

            if (!string.Equals(orderByFieldName, "1 desc", StringComparison.OrdinalIgnoreCase))
            {
                var orderByField = ExtractOrderByField(orderByFieldName);
                ValidateField(orderByField, validFields, "Invalid Order By Column Name");
            }
        }

        public static void ValidateField(string fieldValue, IEnumerable<string> validFields, string errorTitle)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
                return;

            if (!validFields.Any(f => string.Equals(f, fieldValue, StringComparison.OrdinalIgnoreCase)))
            {
                throw new PreDefinedException("Your request has been blocked",errorTitle);
            }
        }

        public static string ExtractOrderByField(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input.Split(' ')[0];
        }
    }

}
