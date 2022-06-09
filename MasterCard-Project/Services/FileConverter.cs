using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using CsvHelper;
using MasterCard_Project.Models;
using System.Collections;
using System.Text;

namespace MasterCard_Project.Services
{
    public class FileConverter
    {

        public static async Task<JToken> ParseFileAsJObject(IFormFile file)
        {

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                return await JToken.ReadFromAsync(new JsonTextReader(reader));
            }
        }

        public static Dictionary<string, object> JTokenToDictrionary(JToken token)
        {
            var dict = new Dictionary<string, object>();

            FillDictionaryFromJToken(dict, token, "$.");

            return dict;


        }

        public static void FillDictionaryFromJToken(Dictionary<string, object> dict, JToken token, string prefix)
        {

            switch (token.Type)
            {

                case JTokenType.Object:
                    foreach (var prop in token.Children<JProperty>())
                    {
                        FillDictionaryFromJToken(dict, prop.Value, prefix + prop.Name + ".");
                    }
                    break;
                case JTokenType.Array:
                    prefix = prefix.Remove(prefix.Length - 1);
                    var index = 0;
                    foreach (JToken value in token.Children())
                    {
                        
                        FillDictionaryFromJToken(dict, value, prefix + "[" + index + "]" + ".");
                        index++;
                    }
                    break;
                default:

                    dict.Add(prefix.Remove(prefix.Length - 1), ((JValue)token).Value);
                    break;
            }
        }

        public static async Task<byte[]> DictionaryToCsv(Dictionary<string, object> dict)
        {

            var records = new List<CsvRecord>();

            foreach (var item in dict)
            {
                records.Add(new CsvRecord { name = item.Key, value = item.Value });
            }
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream))
                using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<CSVRecordMap>();
                    await csv.WriteRecordsAsync(records);
                  
                }
                return memoryStream.ToArray();
            }

        }

        public static byte[] CsvToJson(IFormFile csvFile)
        {
            using (var streamReader = new StreamReader(csvFile.OpenReadStream()))
            using (var csv = new CsvReader(streamReader, System.Globalization.CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<CSVReadRecord>();
                var firstRecord = true;
                object baseObj = new object();
                foreach (var record in records)
                {
                    if (firstRecord)
                    {
                        if (record.name.Contains("["))
                        {
                            baseObj = new ArrayList();
                        }
                        else
                        {
                            baseObj = new Dictionary<string, object>();
                        }
                        firstRecord = false;
                    }
                    var santizedName = SantizeName(record.name);
                    var parent = getParent(baseObj, santizedName);
                    addProperty(parent, record);
                }
                var jsonString = JsonConvert.SerializeObject(baseObj);
                return Encoding.ASCII.GetBytes(jsonString);
            }
        }
        private static object getParent(object baseObject, string propertyName)
        {

            var parent = baseObject;
            var seperatedPath = getParentPath(propertyName);

            for (var i = 0; i < seperatedPath.Length - 1; i++)
            {
                if (i <= seperatedPath.Length - 2)
                {
                    parent = getPropertyOrCreate(parent, seperatedPath[i], seperatedPath[i + 1]);
                }
                else
                {
                    parent = getPropertyOrCreate(parent, seperatedPath[i], "");
                }
            }

            return parent;
        }

        private static void addProperty(object parent, CSVReadRecord record)
        {
            var value = string.IsNullOrEmpty(record.NewValue) ? record.Value : record.NewValue;

            if (parent is Dictionary<string, object>)
            {
              
                var path = record.name.Split('.');
                var propname = path[path.Length - 1];
                ((Dictionary<string, object>)parent).Add(propname, value);
            }

            else
            {
                ((ArrayList)parent).Add(value);
            }


        }

       
        private static string[] getParentPath(string fullPath)
        {
            var path = fullPath.Remove(0, 1);

            return path.Split('.');

        }

        private static object getPropertyOrCreate(object baseObject, string propertyName,string nextPropertyName)
        {
            if (baseObject is Dictionary<string, object>)
            {
                var dictBaseObject = (Dictionary<string, object>)baseObject;
                return GetPropertyOrCreate(dictBaseObject, propertyName, nextPropertyName);
            }
            else
            {
                var arrayBaseObject = (ArrayList)baseObject;
                return GetPropertyOrCreate(arrayBaseObject, propertyName, nextPropertyName);
              
            }

        }

        private static object GetPropertyOrCreate(Dictionary<string, object> dictBaseObject, string propertyName, string nextPropertyName) {

            if (dictBaseObject.ContainsKey(propertyName))
            {
                return dictBaseObject[propertyName];
            }
           
            if (propertyName.Contains('['))
            {
                var arrayName = removeIndexFromPropertyName(propertyName);
                var index = getIndexFromPropertyName(propertyName);
                if (dictBaseObject.ContainsKey(arrayName))
                {
                    var array = (ArrayList)dictBaseObject[arrayName];

                    if (array.Count > index)
                    {
                        return array[index];
                    }
                    addArrayElement(array, nextPropertyName);

                    return array[index];
                }

                var newArray = new ArrayList();
                addArrayElement(newArray,nextPropertyName);
                dictBaseObject[arrayName] = newArray;
                return newArray[index];

            }
            dictBaseObject[propertyName] = new Dictionary<string, object>();
            return dictBaseObject[propertyName];

        }

        private static object GetPropertyOrCreate(ArrayList arrayBaseObject, string propertyName, string nextPropertyName)
        {
            var index = getIndexFromPropertyName(propertyName);

            if (arrayBaseObject.Count > index)
            {
                return arrayBaseObject[index];
            }

            if (nextPropertyName.Contains('['))
            {
                arrayBaseObject.Add(new ArrayList());

                return arrayBaseObject[index];
            }
            arrayBaseObject.Add(new Dictionary<string, object>());
            return arrayBaseObject[index];


        }
        private static int getIndexFromPropertyName(string propertyName)
        {
            var startIndex = propertyName.IndexOf('[') + 1;
            var length = propertyName.LastIndexOf(']') - startIndex;

            return int.Parse(propertyName.Substring(startIndex, length));
        }

        private static string removeIndexFromPropertyName(string propertyName)
        {
            var startIndex = propertyName.IndexOf('[');
            return propertyName.Remove(startIndex);
        }

        private static void addArrayElement(ArrayList newArray,string nextPropertyName) {
            if (nextPropertyName.Contains('['))
            {
                newArray.Add(new ArrayList());
            }
            else
            {
                newArray.Add(new Dictionary<string, object>());
            }
        }

        //In our csv representation '.' is used to mark child property
        //We add a '.' between to array indexes to help with parsing.
        private static string SantizeName(string propertyName) {
            return propertyName.Replace("][", "].[");
        }
    }
    

}
