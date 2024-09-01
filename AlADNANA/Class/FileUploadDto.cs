using System.ComponentModel.DataAnnotations;

namespace AlADNANA.Class
{
    public class FileUploadDto
    {

        [Required]
        public string ExcelFile { get; set; }

        [Required]
        public string ImageFile { get; set; }

         public string ExcelFileName { get; set; }

         public string ImageFileName { get; set; }
    }
}
