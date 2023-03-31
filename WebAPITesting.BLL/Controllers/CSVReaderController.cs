using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using IO = System.IO;

using WebApiTesting.DAL.Data;
using WebAPITesting.DAL.Scripts;
using WebApiTesting.Shared.Models;
using Models = WebApiTesting.Shared.Models;
using WebAPITesting.BLL.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace WebAPITesting.BLL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CSVReaderController : Controller
    {
        const string DateFormat = "yyyy-MM-dd_HH-mm-ss";
        CultureInfo provider = new CultureInfo("ru-RU");

        #region Api methods
        [HttpPost("UploadCSV")]
        public async Task<IActionResult> UploadFileAsync(IFormFile uploadedFile)
        {
            Regex regex = new Regex(@"\.[cC][sS][vV]$");
            string filename = uploadedFile.FileName;
            if (!regex.Match(filename).Success)
            {
                return BadRequest("This file isnt in CSV. Please upload CSV file.");
            }


            using (Stream stream = uploadedFile.OpenReadStream())
            {
                filename = await ServerFileLoader.WriteFileOnServer(stream, filename);
            }

            string errorStarter = $"\nPlease enter valid values at rows.column:\n";
            string errorMessage = "";
            int countOfErrors = 0;

            List<Value> values = new List<Value>();
            values = DataToValues( filename, out errorMessage, out countOfErrors);

            if(values.Count > 10000 && values.Count < 1)
            {
                IO.File.Delete(filename);
                return BadRequest("CSV file has more then 10 000 rows or less then 1 row. Please input correct CSV file.");
            }

            if(countOfErrors > 0)
            {
                IO.File.Delete(filename);
                return BadRequest(errorStarter + errorMessage);
            }

            await IndicatorsDBLoader.LoadToDBAsync(values, CalculateResults(values), filename);

            return Ok();
        }

        [HttpGet("GetResultsInJson")]
        public IActionResult GetResultsInJson(string? filename, string? StartDateTimeFrom, string? StartDateTimeTo, float? AvgValueFrom, float? AvgValueTo, double? AvgTimeFrom, double? AvgTimeTo)
        {
            List<Result> results = new List<Result>();
            const string path = "Files\\";

            StartDateTimeFrom ??= "2001-01-01_00-00-00";
            StartDateTimeTo ??= "9999-12-31_23-59-59";
            AvgValueFrom ??= 0;
            AvgValueTo ??= float.MaxValue;
            AvgTimeFrom ??= 0;
            AvgTimeTo ??= float.MaxValue;


            using (IndicatorsContext context = new IndicatorsContext())
            {
                if (filename != null)
                {
                    filename = path + filename;

                    var file = context.files.Where(x => x.fileName == filename).First();

                    if (file == null)
                    {
                        return BadRequest("File doesnt exist. Please enter correct file name");
                    }
                    results.AddRange(context.results.Where(x => x.fileNameId == file.id));
                }
                else
                {
                    results = context.results.ToList();
                }

                var DateTimeFrom = DateTime.ParseExact(StartDateTimeFrom, DateFormat, provider);
                var DateTimeTo = DateTime.ParseExact(StartDateTimeTo, DateFormat, provider);

                results = Result.FilterByDate(results, DateTimeFrom, DateTimeTo);
                results = Result.FilterByAvgValue(results, AvgValueFrom, AvgValueTo);
                results = Result.FilterByAvgTime(results, AvgTimeFrom, AvgTimeTo);
            }
            return Ok(results);
        }

        [HttpGet("GetValuesInJsonByFileName")]
        public IActionResult GetValuesInJsonByFileName(string? filename)
        {
            List<Value> Values = new List<Value>();
            const string path = "Files\\";

            using (IndicatorsContext context = new IndicatorsContext())
            {
                if (filename != null)
                {
                    filename = path + filename;

                    var file = context.files.Where(x => x.fileName == filename).First();

                    if (file == null)
                    {
                        return BadRequest("File doesnt exist. Please enter correct file name");
                    }
                    Values = context.values.Where(x => x.fileNameId == file.id).ToList();
                }
            }
            return Ok(Values);
        }

        #endregion

        #region Private methods
        private List<Value> DataToValues(string filename, out string errorMessage, out int countOfErrors)
        {
            var content =  IO.File.ReadAllLines(filename);

            int rowNumber = 1;
            List<Value> values = new List<Value>();
            Models.File file = new Models.File()
            {
                fileName = filename
            };

            errorMessage = "";
            countOfErrors = 0;

            foreach (var rows in content)
            {
                var infoInRow = rows.Split(";");
                var date = DateTime.ParseExact(infoInRow[0], DateFormat, provider);
                var time = long.Parse(infoInRow[1]);
                var indicator = float.Parse(infoInRow[2]);

                Value value = new Value()
                {
                    date = date,
                    timeInSeconds = time,
                    value = indicator,
                    file = file
                };
                values.Add(value);

                List<ValidationResult> errors = ValidateValueModel(value);

                if (!errors.IsNullOrEmpty())
                {
                    errorMessage += GenerateErrorString(errors, rowNumber);
                    countOfErrors++;
                }
                rowNumber++;
            }
            return values;
        }
        private List<ValidationResult> ValidateValueModel(Value value)
        {
            var context = new ValidationContext(value);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(value, context, results, true))
            {
                return results;
            }
            return results;
        }
        private string GenerateErrorString(List<ValidationResult> errors, int rowNumber)
        {
            int countErrorsInRow = errors.Count() - 1;
            string errorMessage = string.Empty;
            foreach (var error in errors)
            {
                errorMessage += $"{rowNumber}.{3 - countErrorsInRow} {error.ErrorMessage}";
                if (countErrorsInRow > 0)
                {
                    errorMessage += ";\n";
                }
                else
                {
                    errorMessage += ".\n";
                }
                countErrorsInRow--;
            }
            return errorMessage;
        }
        private Result CalculateResults(List<Value> values)
        {
            TimeSpan allTime;
            DateTime minDateTime = values.First().date;
            DateTime maxDateTime = values.First().date;

            int index;

            float minValue = values.First().value;
            float maxValue = values.First().value;
            float medianValue;
            float avgValue;

            double avgTime;

            List<float> floats = new List<float>();
            List<long> times = new List<long>();

            foreach (var value in values)
            {
                if (value.date < minDateTime)
                {
                    minDateTime = value.date;
                }
                if (value.date > maxDateTime)
                {
                    maxDateTime = value.date;
                }
                if (minValue > value.value)
                {
                    minValue = value.value;
                }
                if (maxValue < value.value)
                {
                    maxValue = value.value;
                }
                times.Add(value.timeInSeconds);
                floats.Add(value.value);
            }

            floats.Sort();

            avgValue = floats.Average();
            avgTime = times.Average();

            if (floats.Count > 1)
                index = (int)Math.Ceiling(floats.Count / 2.0d);
            else
                index = 0;

            medianValue = floats[index];
            allTime = maxDateTime.TimeOfDay - minDateTime.TimeOfDay;
            Result result = new Result()
            {
                rowCount = values.Count(),
                allTime = allTime.Ticks,
                minimalDate = minDateTime,
                maxValue = maxValue,
                minValue = minValue,
                medianValue = medianValue,
                avgTime = avgTime,
                avgValue = avgValue,
                file = values.First().file
            };

            return result;
        }
        #endregion
    }
}