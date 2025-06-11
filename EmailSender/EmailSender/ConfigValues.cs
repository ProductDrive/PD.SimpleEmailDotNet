using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PD.EmailSender.Helpers
{

    public static class Config
    {
        private static readonly string _jsonstring;

        static Config()
        {
            _jsonstring = File.ReadAllText("appsettings.Local.json");

        }

        public static int[] GetPorts()
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(_jsonstring);
            return values.TryGetValue("ports", out var value) ? (int[])value : null;
        }

        public static string GetClientBaseUrl()
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(_jsonstring);
            return values.TryGetValue("clientBaseUrl", out var value) ? value : null;
        }

        public static string GetBaseUrl()
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(_jsonstring);
            return values.TryGetValue("baseUrl", out var value) ? value : null;
        }

        public static char[] GetCharModel()
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(_jsonstring);
            var stringValue = values.TryGetValue("characterModel", out var value) ? value : null;
            return stringValue.ToCharArray();
        }

        public static char[] GetReversedCharModel()
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(_jsonstring);
            var stringValue = values.TryGetValue("reverseCharModel", out var value) ? value : null;
            return stringValue.ToCharArray();
        }
        public static string GetRandomGenSecrete()
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(_jsonstring);
            return values.TryGetValue("randomGenSecrete", out var value) ? value : null;
            
        }
    }
}
