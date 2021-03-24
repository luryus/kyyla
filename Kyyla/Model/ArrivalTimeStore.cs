using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace Kyyla.Model
{
    public class ArrivalTimeStore : IArrivalTimeStore
    {
        private struct ArrivalTimeObject
        {
            public DateTimeOffset ArrivalTime { get; set; }
        }

        private readonly ILogger _logger;
        private readonly string _appDataFolder;
        private readonly string _storeFilePath;

        public ArrivalTimeStore()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _appDataFolder = Path.Combine(appDataPath, "kyyla");
            const string storeFileName = "arrivalTime.json";
            _storeFilePath = Path.Combine(_appDataFolder, storeFileName);

            _logger = Log.Logger.ForContext<ArrivalTimeStore>();
        }

        public async Task<DateTimeOffset> GetArrivalTimeAsync()
        {
            try
            {
                using (var reader = File.OpenText(_storeFilePath))
                {
                    var contents = await reader.ReadToEndAsync();
                    var obj = JsonConvert.DeserializeObject<ArrivalTimeObject>(contents);
                    return obj.ArrivalTime;
                }
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                _logger.Error(e, "Exception while reading arrival time file");
                return DateTimeOffset.MinValue;
            }
            catch (JsonException e)
            {
                _logger.Error(e, "Exception while reading arrival time file");
                return DateTimeOffset.MinValue;
            }
        }

        public async Task SetArrivalTimeAsync(DateTimeOffset arrivalTime)
        {
            try
            {
                var obj = new ArrivalTimeObject {ArrivalTime = arrivalTime};
                var json = JsonConvert.SerializeObject(obj);
                Directory.CreateDirectory(_appDataFolder);

                using (var file = File.Open(_storeFilePath, FileMode.Create))
                using (var writer = new StreamWriter(file))
                {
                    await writer.WriteAsync(json);
                }
                ArrivalTimeChanged?.Invoke(this, arrivalTime);
                _logger.Debug("Stored new arrival time {arrivalTime}", arrivalTime);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while writing arrival time");
            }
        }

        public event EventHandler<DateTimeOffset>? ArrivalTimeChanged;
    }
}