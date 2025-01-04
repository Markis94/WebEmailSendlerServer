using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using WebEmailSendler.Models;

namespace WebEmailSendler.Services
{
    public class FileService
    {

        public List<EmailCsvData> ReadEmailDataFromCsv(string csvData)
        {
            var emailDataList = new List<EmailCsvData>();
            if(csvData == null) 
                return emailDataList;

            var parseString = csvData.Split(";");
            var data = parseString[1].Split(",")[1];
            var bytes = Convert.FromBase64String(data);
            var contents = new MemoryStream(bytes);
            var conf = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null,
                IgnoreBlankLines = true,
                Delimiter = ";"
            };
            using var reader = new StreamReader(contents);
            using (var csv = new CsvReader(reader, conf))
            {
                emailDataList.AddRange(csv.GetRecords<EmailCsvData>());
            }
            return emailDataList;
        }

        public string GenerateEmailBody(SendParameters? parameters, string htmlMesage)
        {
            try
            {
                if (parameters == null)
                    return string.Empty;
                htmlMesage = htmlMesage.Replace("{l}", parameters.Lschet).Replace("{s}", parameters.Sum).Replace("{t}", parameters.Text);
                var endBody = "</body>";
                htmlMesage = htmlMesage.Insert(htmlMesage.LastIndexOf(endBody), 
                    $"<div style=\"display: flex;\n\rjustify-content: center;\"><small style=\"font-size: 10px; visibility: hidden;\">Уведомление № {Guid.NewGuid()}</small></div>\n");
                return htmlMesage;
            }
            catch(Exception ex)
            {
                throw new Exception("Не удалось сформировать файл", ex);
            }
        }
    }
}
