using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CloudDriveShell.Infrastructure.Utils
{
    public class FileHelper
    {
        public static bool SerializeToFile(object serializeObject, string filePath, Encoding encoding)
        {
            if (File.Exists(filePath) == true)
            {
                File.Delete(filePath);
            }
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                XmlTextWriter writer = new XmlTextWriter(fileStream, encoding);
                writer.Formatting = Formatting.Indented;
                XmlSerializer xmlSerializer = new XmlSerializer(serializeObject.GetType());
                xmlSerializer.Serialize(writer, serializeObject);
                fileStream.Flush();
                fileStream.Close();
                return true;
            }
        }

        public static bool SerializeToFile(object serializeObject, string filePath)
        {
            return SerializeToFile(serializeObject, filePath, Encoding.UTF8);
        }

        public static object DeSerializeFromFile(string filePath, Type targetType)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.Position = 0;
                XmlSerializer xmlSerializer = new XmlSerializer(targetType);
                object result = xmlSerializer.Deserialize(fileStream);
                fileStream.Close();
                return result;
            }
        }

        public static void GenFile(string filePath, string sContent)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine(sContent.ToString());
                sw.WriteLine();
                sw.Close();
            }
        }

        public static string ConvertToHumanSize(long size)
        {
            string sizeFormat;
            try
            {
                if (size > 1000000000000)
                {
                    sizeFormat = (size / 1099511627776.0).ToString("f") + "TB";
                }
                else if (size > 1000000000)
                {
                    sizeFormat = (size / (1024.0 * 1024.0 * 1024.0)).ToString("f") + "GB";
                }
                else if (size > 1000000)
                {
                    sizeFormat = (size / (1024.0 * 1024.0)).ToString("f") + "MB";
                }
                else if (size > 1000)
                {
                    sizeFormat = (size / 1024.0).ToString("f") + "KB";
                }
                else
                {
                    sizeFormat = size + "B";
                }
            }
            catch (Exception)
            {
                sizeFormat = size.ToString();
            }

            return sizeFormat;
        }

        public static string RemovePathUnSupportChart(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
            {
                path = path.Replace(invalidFileNameChar.ToString(), string.Empty);
            }
            return path;
        }
    }
}
