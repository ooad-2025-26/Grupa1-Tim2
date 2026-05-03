using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETFTalentProgram.Models
{
    public class AkademskiPodatak
    {
        [Key]
        public long Id { get; set; }
        public string Predmet { get; set; }
        public string SifraPredmeta { get; set; }
        public int Ocjena { get; set; }
        public int ECTS { get; set; }
        public int GodinaPolaganja { get; set; }
        public int Semestar { get; set; }

        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        public AkademskiPodatak() { }
    }
}