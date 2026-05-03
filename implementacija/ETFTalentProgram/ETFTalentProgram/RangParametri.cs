using System.ComponentModel.DataAnnotations;

namespace ETFTalentProgram.Models
{
    public class RangParametri
    {
        [Key]
        public long Id { get; set; }
        public double TezinaProsjecneOcjene { get; set; }
        public double TezinaECTS { get; set; }
        public double TezinaBrojVjestina { get; set; }
        public double TezinaBrojProjekata { get; set; }
        public int Verzija { get; set; }
        public DateTime DatumPrimjene { get; set; }

        public RangParametri() { }
    }
}