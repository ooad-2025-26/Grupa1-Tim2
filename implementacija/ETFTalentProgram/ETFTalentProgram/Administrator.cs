namespace ETFTalentProgram.Models
{
    public class Administrator : Korisnik
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public int NivoOvlastenja { get; set; }

        public Administrator() { }
    }
}