using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MasterCard_Project.Services;
using System;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using MasterCard_Project.Validations;

namespace MasterCard_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversionsController : ControllerBase
    {
        [HttpPost]
        [Route("/convert/json_to_csv")]
        public async Task<IActionResult> jsonToCSV(IFormFile file)
        {
            if (!Validations.Validations.IsConversionRequestValid(file, ".json")){
                return BadRequest("Expecting JSON File.");
            }
            var deserialized = await FileConverter.ParseFileAsJObject(file);

            var flattened = FileConverter.JTokenToDictrionary(deserialized);

            var csvFile = await FileConverter.DictionaryToCsv(flattened);

            return File(csvFile, "text/csv", "array.csv");
        }

        [HttpPost]
        [Route("/convert/csv_to_json")]
        public async Task<IActionResult> csvToJSON(IFormFile file)
        {
            if (!Validations.Validations.IsConversionRequestValid(file, ".csv"))
            {
                return BadRequest("Expecting CSV File.");
            }
            return File(FileConverter.CsvToJson(file), "application/json", "result.json");
        }

    }
}
