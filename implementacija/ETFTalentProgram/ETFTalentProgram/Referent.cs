namespace ETFTalentProgram.Models
{
    public class Referent : Korisnik
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Odsjek { get; set; }

        public Referent() { }
    }
}