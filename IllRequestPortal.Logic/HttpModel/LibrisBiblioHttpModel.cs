namespace Logic.Model
{
    public class LibrisBiblioLookupResult
    {
        public string Id { get; set; } = "";
        public int? BiblioId { get; set; }

        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string ContainerTitle { get; set; } = "";

        public string PublicationYear { get; set; } = "";
        public string Edition { get; set; } = "";
        public string IsbnIssn { get; set; } = "";

        public string Volume { get; set; } = "";
        public string Issue { get; set; } = "";
        public string Pages { get; set; } = "";

        public string Publisher { get; set; } = "";
        public string MaterialType { get; set; } = "";

        public bool ExistsInLocalCatalog { get; set; }

        public bool IsEmpty() =>
            string.IsNullOrEmpty(Title) &&
            string.IsNullOrEmpty(Author) &&
            string.IsNullOrEmpty(ContainerTitle) &&
            string.IsNullOrEmpty(PublicationYear) &&
            string.IsNullOrEmpty(Edition) &&
            string.IsNullOrEmpty(Volume);
    }
}
