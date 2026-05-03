namespace ETFTalentProgram.Models
{
    public class Firma : Korisnik
    {
        public string Naziv { get; set; }
        public string OpisFirme { get; set; }
        public string Lokacija { get; set; }
        public string Website { get; set; }
        public string KontaktEmail { get; set; }
        public string IndustrijskiSektor { get; set; }
        public VelicinaFirme VelicinaFirme { get; set; }
        public int GodinaOsnivanja { get; set; }

        public Firma() { }
    }
}