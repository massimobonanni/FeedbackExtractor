using Azure.AI.DocumentIntelligence;
using FeedbackExtractor.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Azure.AI.DocumentIntelligence
{
    internal static class AnalyzeResultExtensions
    {
        internal static string GetKeyValue(this AnalyzeResult source, string keyname, 
            float confidence,bool caseSensitive = false)
        {
            if (!caseSensitive)
            {
                keyname = keyname.ToLower();
            }

            foreach (var item in source.KeyValuePairs)
            {
                var key = item.Key.Content;
                if (!caseSensitive)
                    key = key.ToLower();

                if (key == keyname && item.Confidence>=confidence && item.Value!= null)
                    return item.Value.Content;
            }
            return null;
        }

        internal static int? GetCheckedColumnFromTableRow(this AnalyzeResult source, int tableIndex,int rowIndex)
        {
            if (tableIndex < 0 || tableIndex >= source.Tables.Count)
                return null;

            int? result = null;

            var table= source.Tables[tableIndex];
            if (rowIndex >= 0 && rowIndex < table.RowCount)
            {
                var firstIndex = rowIndex * table.ColumnCount;
                for (int i = 0; i < table.ColumnCount; i++)
                {
                    var cell = table.Cells[firstIndex + i];
                    if (cell.Content.Contains(":selected:",StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = cell.ColumnIndex;
                        break;
                    }
                }
            }

            return result;
        }

        

    }
}
