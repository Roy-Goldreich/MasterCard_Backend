using Newtonsoft.Json.Linq;

namespace MasterCard_Project.Models
{
    public class CsvRecord
    {
        public string name { get; set; }

        //Was dynamic
        public dynamic value { get; set; }

        public dynamic NewValue { get; set; }
    }   
}
