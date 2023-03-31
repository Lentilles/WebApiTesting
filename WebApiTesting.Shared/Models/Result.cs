using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiTesting.Shared.Models
{
    public class Result
    {
        [Key]
        public int id { get; set; }
        public int rowCount { get; set; }
        public long allTime { get; set; }
        public double avgTime { get; set; }
        public float medianValue { get; set; }
        public float avgValue { get; set; }
        public float maxValue { get; set; }
        public float minValue { get; set; }
        public int fileNameId { get; set; }
        public DateTime minimalDate { get; set; }

        [ForeignKey(nameof(fileNameId))]
        public File file { get; set; }

        public Result(int rowCount, long allTime, double avgTime, float medianValue, float avgValue, float maxValue, float minValue, DateTime minimalDate, File file)
        {
            this.rowCount = rowCount;
            this.allTime = allTime;
            this.avgTime = avgTime;
            this.medianValue = medianValue;
            this.avgValue = avgValue;
            this.maxValue = maxValue;
            this.minValue = minValue;
            this.minimalDate = minimalDate;
            this.file = file;
        }
        public Result()
        {
            file = new File();
        }


        public static List<Result> FilterByDate(List<Result> results, DateTime? startDate, DateTime? endDate)
        {
            startDate ??= new DateTime(2000, 01, 01);
            endDate ??= new DateTime(9999, 12, 31);

            return results.Where(x => x.minimalDate >= startDate && x.minimalDate <= endDate).ToList();
        }
        public static List<Result> FilterByAvgValue(List<Result> results, float? AvgValueFrom, float? AvgValueTo)
        {

            return results.Where(x => x.avgValue >= AvgValueFrom && x.avgValue <= AvgValueTo).ToList();
        }
        public static List<Result> FilterByAvgTime(List<Result> results, double? AvgTimeFrom, double? AvgTimeTo)
        {

            return results.Where(x => x.avgTime >= AvgTimeFrom && x.avgTime <= AvgTimeTo).ToList();
        }
    }
}
