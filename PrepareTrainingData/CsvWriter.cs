using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace PrepareTrainingData
{
    public class CsvWriter
    {
        private readonly string dataFilePath;
        private bool needToAddHeader;

        public CsvWriter(string dataFilePath)
        {
            this.dataFilePath = dataFilePath;

            var fileAlreadyExists = File.Exists(dataFilePath);

            needToAddHeader = !fileAlreadyExists;

            if (fileAlreadyExists)
            {
                CreateBackup(dataFilePath);
            }
        }

        public void WriteRecords(IEnumerable<Dictionary<string, string>> records)
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            };

            using var writer = new StreamWriter(dataFilePath, true);
            using var csv = new CsvHelper.CsvWriter(writer, csvConfiguration);

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

        private void CreateBackup(string sourceFilePath)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            File.Copy(sourceFilePath, sourceFilePath + $".{timestamp}.backup");
        }
    }
}
