using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Vision_Align
{
    public class JsonConvertor
    {
        private static readonly object FileLock = new object();

        public static T JsonToClass<T>(string path)
        {
            try
            {
                return DeserializeFile<T>(path);
            }
            catch (Exception primaryException)
            {
                string backupPath = path + ".bak";
                if (!File.Exists(backupPath))
                    throw;

                CrashDiagnostics.ReportRecoverableException(
                    "Configuration recovery from backup: " + Path.GetFileName(path),
                    primaryException);
                return DeserializeFile<T>(backupPath);
            }
        }

        // Save through a temporary file so a power loss cannot leave a half-written JSON file.
        public static void ClassToJson(object value, string path)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string tempPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            string backupPath = path + ".bak";

            lock (FileLock)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(value, Formatting.Indented);
                    File.WriteAllText(tempPath, json, new UTF8Encoding(false));

                    if (File.Exists(path))
                        File.Replace(tempPath, path, backupPath, true);
                    else
                        File.Move(tempPath, path);
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
            }
        }

        private static T DeserializeFile<T>(string path)
        {
            T result = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            if (ReferenceEquals(result, null))
                throw new InvalidDataException("Configuration deserialized to null: " + path);

            return result;
        }
    }
}
