using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETFTalentProgram.Models
{
    public class Oglas
    {
        [Key]
        public long Id { get; set; }
        public string Naslov { get; set; }
        public string Opis { get; set; }
        public string Tehnologije { get; set; }
        public DateTime RokPrijave { get; set; }
        public DateTime DatumObjave { get; set; }
        public TipOglasa TipOglasa { get; set; }
        public TipAngazmana TipAngazmana { get; set; }
        public StatusOglasa StatusOglasa { get; set; }
        public string Lokacija { get; set; }
        public double MinRang { get; set; }
        public double MinProsjek { get; set; }
        public string Kompenzacija { get; set; }

        [ForeignKey("Firma")]
        public long FirmaId { get; set; }
        public Firma Firma { get; set; }

        public Oglas() { }
    }
}