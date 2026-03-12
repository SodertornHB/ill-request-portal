namespace IllRequestPortal.Web.ViewModel
{
    public class LookupBibliographicRecordResponse
    {
        public int BiblioId { get; set; }
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string PublicationYear { get; set; } = "";
        public string Edition { get; set; } = "";
        public string Volume { get; set; } = "";
    }
}
