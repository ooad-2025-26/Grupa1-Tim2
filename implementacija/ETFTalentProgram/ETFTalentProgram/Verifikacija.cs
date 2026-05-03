using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETFTalentProgram.Models
{
    public class Verifikacija
    {
        [Key]
        public long Id { get; set; }
        public DateTime DatumPodnosenja { get; set; }
        public DateTime? DatumVerifikacije { get; set; }
        public StatusVerifikacije StatusVerifikacije { get; set; }
        public string Komentar { get; set; }
        public string Dokumenti { get; set; }

        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("Referent")]
        public long? ReferentId { get; set; }
        public Referent Referent { get; set; }

        public Verifikacija() { }
    }
}