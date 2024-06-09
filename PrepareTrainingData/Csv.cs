using System.Globalization;
using System.ServiceModel.Channels;
using CsvHelper;
using CsvHelper.Configuration;

namespace PrepareTrainingData
{
    public class Csv
    {
        private readonly string dataFilePath;
        private bool needToAddHeader;

        public Csv(string dataFilePath)
        {
            this.dataFilePath = dataFilePath;
            needToAddHeader = !File.Exists(dataFilePath);
        }

        public void Write(IEnumerable<Dictionary<string,string>> records)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            };

            using var writer = new StreamWriter(dataFilePath, true);
            using var csv = new CsvWriter(writer, config);

            if (needToAddHeader)
            {
                foreach (var pair in records.First())
                {
                    csv.WriteField(pair.Key);
                }

                needToAddHeader = false;
                
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
