using Microsoft.AspNetCore.Http;

namespace MasterCard_Project.Validations
{
    public class Validations
    {
        public static bool IsConversionRequestValid(IFormFile file,string expectedExtension) {
            
            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || (extension != expectedExtension)) {
                return false;
            }

            return true;
        
        }


    }
}
