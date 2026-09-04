using System.IO;
using Newtonsoft.Json;

namespace Vision_Align
{
    public class JsonConvertor
    {
        public static T JsonToClass<T>(string _path)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(_path));
        }

        // Class to Json(File) Save
        public static void ClassToJson(object _object, string _path)
        {
            System.IO.File.WriteAllText(_path, JsonConvert.SerializeObject(_object, Formatting.Indented));
        }
    }
}
