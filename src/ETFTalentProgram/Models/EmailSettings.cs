// Models/EmailSettings.cs
namespace ETFTalentProgram.Models
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string KorisnickoIme { get; set; }
        public string Lozinka { get; set; }
        public string ImePosiljaoca { get; set; }
        public string EmailPosiljaoca { get; set; }
    }
}