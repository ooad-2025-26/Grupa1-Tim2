using System.ComponentModel.DataAnnotations;

namespace ETFTalentProgram.Models
{
    public abstract class Korisnik
    {
        [Key]
        public long Id { get; set; }
        public string Email { get; set; }
        public string Lozinka { get; set; }
        public Uloga Uloga { get; set; }
        public Status Status { get; set; }
        public DateTime DatumRegistracije { get; set; }
        public DateTime DatumZadnjePrijave { get; set; }

        public Korisnik() { }
    }
}