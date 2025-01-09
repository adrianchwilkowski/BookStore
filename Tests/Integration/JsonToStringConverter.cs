using Infrastructure.Entities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Integration
{
    public class JsonToStringConverter
    {
        public static List<List<string>> ToJson(object? json, List<string> properties) 
        {
            var response = new List<List<string>>();
            int i=0;
            if (json is not null && json is System.Collections.IEnumerable enumerable)
            {
                foreach (var record in enumerable)
                {
                    if (record is JObject jObject)
                    {
                        response.Add(new List<string>());
                        foreach (var property in properties)
                        {
                            response[i].Add((string)jObject[property]);
                        }
                        i++;
                    }
                }
            }
            return response;
        }
    }
}
