namespace WebEmailSendler.Models
{
    public class Reports
    {
        public string Email { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Дата отправки")]
        public string Date { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Статус")]
        public string IsSuccess { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Ошибка")]
        public string Error { get; set; }

    }

    public class SearchEmailReport
    {
        public string Email { get; set; }
        public int Count { get; set; }
        public int TaskSendId  { get; set; }
        public string TaskSendName { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
}
