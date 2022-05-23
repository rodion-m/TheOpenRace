using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;

namespace OpenRace.Helpers;

public static class TinyCsvHelper
{
    public static string CreateCsvContent(
        IEnumerable<string>? headers, IEnumerable<string[]> rows, string delimeter = ";")
    {
        if (rows == null) throw new ArgumentNullException(nameof(rows));
        if (headers is not null)
        {
            rows = (IEnumerable<string[]>) new[] { headers }
                .Concat(rows);
        }
        var csv = rows
            .Select(it => string.Join(delimeter, it))
            .ToDelimitedString("\r\n");
        return csv;
    }
}