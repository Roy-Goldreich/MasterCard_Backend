using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MasterCard_Project.DTO
{
    public class JsonUpload
    {

        [DataType(DataType.Upload)]
        [Required(ErrorMessage ="Must upload json file")]
        public IFormFile Json { get; set; }

    }
}
