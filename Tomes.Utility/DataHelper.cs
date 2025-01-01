using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tomes.Utility
{
    public class DataHelper
    {
        public static List<T> ReadJsonFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: The file at {filePath} does not exists");
                return new List<T>();
            }
            string json = File.ReadAllText(filePath);
            try
            {
                return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Could not deserialize the json file located at {filePath} with exception {ex.Message}");
                return new List<T>();
            }
        }
    }
}
