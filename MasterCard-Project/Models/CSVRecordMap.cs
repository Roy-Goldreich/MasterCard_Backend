using CsvHelper.Configuration;

namespace MasterCard_Project.Models
{
    public class CSVRecordMap : ClassMap<CsvRecord>
    {
        public CSVRecordMap() {
            Map(m => m.name).Index(0).Name("name");
            Map(m => m.value).Index(1).Name("Value");
            Map(m => m.NewValue).Index(2).Name("NewValue");
        }
    }
}
