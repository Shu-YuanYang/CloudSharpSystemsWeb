using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AuxiliaryClassLibrary.IO
{
    public class CSVFileHelper
    {
        // reference: https://stackoverflow.com/questions/18757097/writing-data-into-csv-file-in-c-sharp
        public static void AddToFile(string file_path, string[] lines) 
        {
            //before your loop
            var csv = new StringBuilder();

            //in your loop
            foreach (string line in lines) {
                //var newLine = string.Format("{0},{1}", first, second);
                csv.AppendLine(line);
            }

            //after your loop
            string current_directory = Directory.GetCurrentDirectory();
            string full_file_path = Path.Combine(new string[2] { current_directory, file_path });
            System.IO.FileInfo file = new System.IO.FileInfo(full_file_path);
            file.Directory!.Create(); // If the directory already exists, this method does nothing.
            File.AppendAllText(full_file_path, csv.ToString());
        }

    }


}
