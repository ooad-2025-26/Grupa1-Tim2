namespace ETFTalentProgram.Models
{
    public enum Uloga { STUDENT, FIRMA, REFERENT, ADMINISTRATOR }
    public enum Status { AKTIVAN, SUSPENDOVAN, OBRISAN }
    public enum VelicinaFirme { MALA, SREDNJA, VELIKA }
    public enum TipOglasa { POSAO, PRAKSA }
    public enum TipAngazmana { PUNO_RADNO_VRIJEME, POLA_RADNOG_VREMENA, REMOTE, HIBRIDNO }
    public enum StatusOglasa { AKTIVAN, ARHIVIRAN, OBRISAN }
    public enum StatusPrijave { NOVA, PREGLEDANA, PRIHVACENA, ODBIJENA }
    public enum StatusPonude { POSLANO, PRIHVACENO, ODBIJENO, NA_CEKANJU }
    public enum TipPonude { FIRMA_STUDENTU, STUDENT_FIRMI }
    public enum StatusVerifikacije { NA_CEKANJU, VERIFICIRAN, ODBIJEN }
    public enum TipNotifikacije { NOVA_PONUDA, NOVA_PRIJAVA, VERIFIKACIJA_ODOBRENA, VERIFIKACIJA_ODBIJENA, NOVA_REGISTRACIJA }
    public enum StatusIsporuke { NA_CEKANJU, POSLANO, GRESKA }
    public enum NivoLoga { INFO, WARNING, ERROR }
}