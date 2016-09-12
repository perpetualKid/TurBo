using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassLibrary1
{
    public static class JsonStreamReader
    {
        public static void ReadEndless(string fileName)
        {
            using (TextReader textReader = new StreamReader(new FileStream(fileName, FileMode.Open)))
            {

                using (var reader = new JsonTextReader(textReader))
                {
                    while (reader.Read())
                    {
                        var item = new JsonSerializer().Deserialize(reader);
                        Debug.WriteLine(item);
                    }
                }
            }
        }

        public static void ReadEndless(Stream stream)
        {
            using (TextReader textReader = new StreamReader(stream))
            {

                using (var reader = new JsonTextReader(textReader))
                {
                    reader.SupportMultipleContent = true;
                    reader.CloseInput = false;
                    var serializer = new JsonSerializer();
                    //if (!reader.Read() )//|| reader.TokenType != JsonToken.StartArray)
                    //    throw new Exception("Expected start of array in the deserialized json string");

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.EndArray) break;
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            var item = serializer.Deserialize(reader);
                            Debug.WriteLine(item);
                        }
                    }

                    //while (reader.Read())
                    //{
                    //    var item = new JsonSerializer().Deserialize(reader);
                    //    Debug.WriteLine(item);
                    //}
                }
            }
        }

    }
}
