using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETFTalentProgram.Models
{
    public class StudentProfil
    {
        [Key]
        public long Id { get; set; }
        public double Rang { get; set; }
        public string Biografija { get; set; }
        public string Vjestine { get; set; }
        public string Projekti { get; set; }
        public string PreferiraneLokacije { get; set; }
        public string PreferiraneTehnologije { get; set; }
        public DateTime DostupanOd { get; set; }
        public DateTime DatumAzuriranja { get; set; }
        public StatusVerifikacije StatusVerifikacije { get; set; }

        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        public StudentProfil() { }
    }
}