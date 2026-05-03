using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETFTalentProgram.Models
{
    public class FirmaProfil
    {
        [Key]
        public long Id { get; set; }
        public string KratakOpis { get; set; }
        public string PunOpis { get; set; }
        public string Lokacija { get; set; }
        public string Website { get; set; }
        public string KontaktEmail { get; set; }
        public string Logotip { get; set; }
        public string TehnologijeStack { get; set; }
        public DateTime DatumAzuriranja { get; set; }

        [ForeignKey("Firma")]
        public long FirmaId { get; set; }
        public Firma Firma { get; set; }

        public FirmaProfil() { }
    }
}