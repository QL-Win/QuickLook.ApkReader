using Newtonsoft.Json;
using System;
using System.IO;

namespace ApkReader.Run
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var extension = Path.GetExtension(args[0]);

            if (extension == ".apk")
            {
                var reader = new ApkReader();
                var info = reader.Read(args[0]);
                var json = JsonConvert.SerializeObject(info, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                });
                Console.WriteLine(json);
                Console.ReadLine();
            }
            else if (extension == ".aab")
            {
                var reader = new AabReader();
                var info = reader.Read(args[0]);
                var json = JsonConvert.SerializeObject(info, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                });
                Console.WriteLine(json);
                Console.ReadLine();
            }
        }
    }
}
