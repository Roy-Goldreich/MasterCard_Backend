using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MasterCard_Project.Services;

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

            return File(csvFile, "text/csv",file.FileName.Split('.')[0]+".csv");
        }

        [HttpPost]
        [Route("/convert/csv_to_json")]
        public IActionResult csvToJSON(IFormFile file)
        {
            if (!Validations.Validations.IsConversionRequestValid(file, ".csv"))
            {
                return BadRequest("Expecting CSV File.");
            }
            return File(FileConverter.CsvToJson(file), "application/json", file.FileName.Split('.')[0] + ".json");
        }

    }
}
