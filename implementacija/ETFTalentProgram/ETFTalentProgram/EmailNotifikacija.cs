using System.ComponentModel.DataAnnotations;

namespace ETFTalentProgram.Models
{
    public class EmailNotifikacija
    {
        [Key]
        public long Id { get; set; }
        public TipNotifikacije TipNotifikacije { get; set; }
        public string Sadrzaj { get; set; }
        public string EmailPrimaoca { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public DateTime? DatumSlanja { get; set; }
        public StatusIsporuke StatusIsporuke { get; set; }
        public int BrojPokusaja { get; set; }

        public EmailNotifikacija() { }
    }
}