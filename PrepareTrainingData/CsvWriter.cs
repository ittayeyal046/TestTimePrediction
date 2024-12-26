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
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var sourceDirectory = Path.GetDirectoryName(sourceFilePath);
            var fileName = Path.GetFileName(sourceFilePath);
            var backupFolder = Path.Combine(sourceDirectory,"CsvBackup");

            // Ensure the backup folder exists
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            var destinationFilePath = Path.Combine(backupFolder, fileName + $".{timestamp}.backup");
            File.Copy(sourceFilePath, destinationFilePath);
        }
    }
}
