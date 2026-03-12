using Newtonsoft.Json;

namespace Logic.Model
{
    public class Patron
    {
        public string CardNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    public class KohaPatronEntity
    {
        public string cardnumber { get; set; } = "";
        public string firstname { get; set; } = "";
        public string surname { get; set; } = "";
        public string email { get; set; } = "";
    }
    public class KohaGetBiblioHttpModel
    {
        [JsonProperty("biblio_id")]
        public int BiblioId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; } = "";

        [JsonProperty("author")]
        public string Author { get; set; } = "";

        [JsonProperty("isbn")]
        public string Isbn { get; set; } = "";

        [JsonProperty("pages")]
        public string Pages { get; set; } = "";

        [JsonProperty("publication_year")]
        public string PublicationYear { get; set; } = "";

        [JsonProperty("abstract")]
        public string Abstract { get; set; } = "";

        [JsonProperty("volume")]
        public string Volume { get; set; } = "";

        [JsonProperty("volume_description")]
        public string VolumeDescription { get; set; } = "";

        public string GetTitleAndSubtitle() => string.IsNullOrWhiteSpace(Subtitle) ? Title?.Trim() : $"{Title?.TrimEnd(' ', ':')} : {Subtitle.Trim().TrimEnd('/').TrimEnd('\\')}";
        
        public string GetVolume()
        {
            if(!string.IsNullOrEmpty(Volume)) return Volume;
            if (!string.IsNullOrEmpty(VolumeDescription)) return VolumeDescription;
            return string.Empty;
        }
    }
}
