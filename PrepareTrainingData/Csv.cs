using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace TestTimePrediction
{
    public class Csv
    {
        private bool hasHeaderBeenWritten;

        public void Write(string fileName, IEnumerable<Dictionary<string,string>> records)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
            };
            using var writer = new StreamWriter(fileName);
            using var csv = new CsvWriter(writer, config);

            if (!hasHeaderBeenWritten)
            {
                foreach (var pair in records.First())
                {
                    csv.WriteField(pair.Key);
                }

                hasHeaderBeenWritten = true;
                
                csv.NextRecord();
            }

            foreach (var record in records)
            {
                foreach (var pair in record)
                {
                    csv.WriteField(pair.Value);
                }

                csv.NextRecord();
            }
        }
    }

}
