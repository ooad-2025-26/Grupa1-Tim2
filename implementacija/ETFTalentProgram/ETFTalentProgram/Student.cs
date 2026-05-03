namespace ETFTalentProgram.Models
{
    public class Student : Korisnik
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string BrIndeksa { get; set; }
        public int GodinaStudija { get; set; }
        public int GodinaUpisa { get; set; }
        public double ProsjekOcjena { get; set; }
        public bool Verificiran { get; set; }

        public Student() { }
    }
}