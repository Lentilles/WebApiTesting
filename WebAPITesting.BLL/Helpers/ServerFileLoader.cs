using IO = System.IO;
namespace WebAPITesting.BLL.Helpers
{
    public class ServerFileLoader
    {
        public static async Task<string> WriteFileOnServer(Stream stream, string filename)
        {
            string fileNamePath = $"Files\\{filename}";
            int countOfCopies = 1;

            if (IO.File.Exists(fileNamePath))
            {
                var pathForCopy = $"Files\\Versions\\{filename}";
                while (IO.File.Exists(pathForCopy))
                {
                    pathForCopy = $"Files\\Versions\\{filename}";
                    var indexForInsert = pathForCopy.Length - 4;
                    pathForCopy = pathForCopy.Insert(indexForInsert, $"[{countOfCopies}]");
                    countOfCopies++;
                }
                IO.File.Copy(fileNamePath, pathForCopy);
                IO.File.Delete(fileNamePath);
            }
            using (FileStream fileStream = new FileStream(fileNamePath, FileMode.Create, FileAccess.ReadWrite))
            {
                await stream.CopyToAsync(fileStream);
            }

            return fileNamePath;
        }
    }
}
