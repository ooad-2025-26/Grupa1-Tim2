using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETFTalentProgram.Models
{
    public class Ponuda
    {
        [Key]
        public long Id { get; set; }
        public string TekstPoruke { get; set; }
        public DateTime DatumSlanja { get; set; }
        public StatusPonude Status { get; set; }
        public TipPonude TipPonude { get; set; }
        public DateTime? DatumOdgovora { get; set; }

        [ForeignKey("Posiljalac")]
        public long PosiljalacId { get; set; }
        public Korisnik Posiljalac { get; set; }

        [ForeignKey("Primalac")]
        public long PrimalacId { get; set; }
        public Korisnik Primalac { get; set; }

        public Ponuda() { }
    }
}