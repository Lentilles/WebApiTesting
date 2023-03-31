using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiTesting.DAL.Data;
using WebApiTesting.Shared.Models;

namespace WebAPITesting.DAL.Scripts
{
    public class IndicatorsDBLoader
    {
        public static async Task LoadToDBAsync(List<Value> values, Result result, string filename)
        {
            using (IndicatorsContext context = new IndicatorsContext())
            {
                bool fileExist = context.files.Where(x => x.fileName == filename).Count() > 0;

                if (!fileExist)
                {
                    context.results.Add(result);
                    await context.values.AddRangeAsync(values);
                }
                else
                {
                    var file = context.files.Where(x => x.fileName == filename).First();
                    var fileid = file.id;

                    var valuesFromDb = context.values.Where(x => x.fileNameId == fileid).ToList();

                    if (valuesFromDb.Count < values.Count)
                    {
                        List<Value> tempValues = new List<Value>();
                        tempValues.AddRange(values);

                        for (int i = 0; i < valuesFromDb.Count; i++)
                        {
                            valuesFromDb[i].date = tempValues[i].date;
                            valuesFromDb[i].timeInSeconds = tempValues[i].timeInSeconds;
                            valuesFromDb[i].value = tempValues[i].value;
                            tempValues.RemoveAt(i);
                        }
                        foreach (var value in tempValues)
                        {
                            value.file = file;
                        }
                        await context.values.AddRangeAsync(tempValues);
                    }
                    else
                    {
                        for (int i = 0; i < values.Count; i++)
                        {
                            valuesFromDb[i].date = values[i].date;
                            valuesFromDb[i].timeInSeconds = values[i].timeInSeconds;
                            valuesFromDb[i].value = values[i].value;
                        }
                    }
                    context.UpdateRange(valuesFromDb);

                    Result? dbResult = await context.results.Where(x => x.fileNameId == fileid).FirstOrDefaultAsync();
                    Result calculatedResult = result;
                    if (dbResult != null)
                    {

                        dbResult.medianValue = calculatedResult.medianValue;
                        dbResult.maxValue = calculatedResult.maxValue;
                        dbResult.minValue = calculatedResult.minValue;
                        dbResult.rowCount = calculatedResult.rowCount;
                        dbResult.allTime = calculatedResult.allTime;
                        dbResult.minimalDate = calculatedResult.minimalDate;

                        context.Update(dbResult);
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
