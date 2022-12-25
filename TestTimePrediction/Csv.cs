using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace TestTimePrediction
{
    public class Csv
    {
       public void Write<T>(string fileName, IEnumerable<T> data)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
            };
            using var writer = new StreamWriter(fileName);
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(data);
        }
    }

}
