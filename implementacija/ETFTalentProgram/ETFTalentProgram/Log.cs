using System.ComponentModel.DataAnnotations;

namespace ETFTalentProgram.Models
{
    public class Log
    {
        [Key]
        public long Id { get; set; }
        public string TipAkcije { get; set; }
        public DateTime VrijemeAkcije { get; set; }
        public long? KorisnikId { get; set; }
        public string IpAdresa { get; set; }
        public string Detalji { get; set; }
        public NivoLoga Nivo { get; set; }

        public Log() { }
    }
}