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
}
