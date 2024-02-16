namespace Azure.AI.DocumentIntelligence
{
    internal static class AnalyzeResultExtensions
    {
        /// <summary>
        /// Gets the value of a key from the <see cref="AnalyzeResult"/>.
        /// </summary>
        /// <param name="source">The <see cref="AnalyzeResult"/> instance.</param>
        /// <param name="keyname">The name of the key to retrieve.</param>
        /// <param name="confidence">The minimum confidence level required for the key-value pair.</param>
        /// <param name="caseSensitive">Indicates whether the key comparison should be case-sensitive. Default is false.</param>
        /// <returns>The value of the key if found; otherwise, null.</returns>
        internal static string GetKeyValue(this AnalyzeResult source, string keyname,
            float confidence, bool caseSensitive = false)
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

                if (key == keyname && item.Confidence >= confidence && item.Value != null)
                    return item.Value.Content;
            }
            return null;
        }

        /// <summary>
        /// Gets the column index of a checked cell in a specific table row from the <see cref="AnalyzeResult"/>.
        /// </summary>
        /// <param name="source">The <see cref="AnalyzeResult"/> instance.</param>
        /// <param name="tableIndex">The index of the table.</param>
        /// <param name="rowIndex">The index of the row.</param>
        /// <returns>The column index of the checked cell if found; otherwise, null.</returns>
        internal static int? GetCheckedColumnFromTableRow(this AnalyzeResult source, int tableIndex, int rowIndex)
        {
            if (tableIndex < 0 || tableIndex >= source.Tables.Count)
                return null;

            int? result = null;

            var table = source.Tables[tableIndex];
            if (rowIndex >= 0 && rowIndex < table.RowCount)
            {
                var firstIndex = rowIndex * table.ColumnCount;
                for (int i = 0; i < table.ColumnCount; i++)
                {
                    var cell = table.Cells[firstIndex + i];
                    if (cell.Content.Contains(":selected:", StringComparison.InvariantCultureIgnoreCase))
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
