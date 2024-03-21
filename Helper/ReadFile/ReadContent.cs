using System.IO;

namespace Helper.ReadFile
{
    public static class ReadContent
    {
        public static string[] Read(string fileName)
        {
            string[] values = null;

            using (StreamReader sr = new StreamReader(fileName))
            {
                string content = sr.ReadToEnd();
                values = content.Split(',');
            }

            return values;
        }
    }
}
