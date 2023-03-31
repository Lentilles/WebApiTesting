using WebApiTesting.Shared.CustomValidation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiTesting.Shared.Models
{
    public class Value
    {
        [Key]
        public int id { get; set; }
        [Required]
        public int fileNameId { get; set; }
        [Required]
        [DateTimeRange()]
        public DateTime date { get; set; }
        [Required]
        [Range(0, long.MaxValue, ErrorMessage = $"Time in seconds. Please input value between 0 and 9 223 372 036 854 775 807")]
        public long timeInSeconds { get; set; }
        [Required]
        [Range(0, float.MaxValue, ErrorMessage = $"Value. Please input value between 0 and 3,4 x 10^38")]
        public float value { get; set; }
        [ForeignKey(nameof(fileNameId))]
        public File file { get; set; }

        public Value()
        {
            file = new File();
        }

        public Value(DateTime date, long timeInSeconds, float value, File file)
        {
            this.date = date;
            this.timeInSeconds = timeInSeconds;
            this.value = value;
            this.file = file;
        }
    }
}
