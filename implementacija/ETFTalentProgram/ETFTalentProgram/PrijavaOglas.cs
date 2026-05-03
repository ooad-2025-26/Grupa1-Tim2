using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETFTalentProgram.Models
{
    public class PrijavaOglas
    {
        [Key]
        public long Id { get; set; }
        public DateTime DatumPrijave { get; set; }
        public string PropratniTekst { get; set; }
        public StatusPrijave StatusPrijave { get; set; }
        public DateTime? DatumOdgovora { get; set; }

        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("Oglas")]
        public long OglasId { get; set; }
        public Oglas Oglas { get; set; }

        public PrijavaOglas() { }
    }
}