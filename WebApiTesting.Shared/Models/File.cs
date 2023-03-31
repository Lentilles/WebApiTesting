using System.ComponentModel.DataAnnotations;

namespace WebApiTesting.Shared.Models
{
    public class File
    {
        [Key]
        public int id { get; set; }
        public string fileName { get; set; }
        public File(string fileName)
        {
            this.fileName = fileName;
        }
        public File()
        {
            fileName = "";
        }
    }
}
