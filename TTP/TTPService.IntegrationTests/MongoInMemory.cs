using System;
using System.Diagnostics;
using System.IO;

namespace TTPService.IntegrationTests
{
    public class MongoInMemory : IDisposable
    {
        public const string ConnectionString = "mongodb://127.0.0.1:27017";
        public const string DbName = "test";
        public const string Host = "127.0.0.1";
        public const string Port = "27017";

        private readonly string _mongoFolder;
        private readonly Process _process;

        public MongoInMemory()
        {
            var assemblyFolder = Path.GetDirectoryName(new Uri(typeof(MongoInMemory).Assembly.CodeBase).LocalPath);
            _mongoFolder = Path.Combine(assemblyFolder, "mongo");
            var dbFolder = Path.Combine(_mongoFolder, "temp");

            if (!Directory.Exists(dbFolder))
            {
                try
                {
                    Directory.CreateDirectory(dbFolder);
                }
                catch (Exception e)
                {
                    throw new Exception("Integration-test Mongo database 'temp' folder creation failed", e);
                }
            }

            _process = new Process();
            _process.StartInfo.FileName = Path.Combine(_mongoFolder, "mongod.exe");
            _process.StartInfo.Arguments = "--dbpath " + dbFolder + " --storageEngine ephemeralForTest";
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
            }

            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
        }
    }
}