using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ETFTalentProgram.Constants;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using ETFTalentProgram.Services;
using ETFTalentProgram.ViewModels;

namespace ETFTalentProgram.Controllers
{
    public class FirmaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStudentRangService _studentRangService;
        private readonly ILogService _logService;
        private readonly IEmailService _emailService;

        public FirmaController(ApplicationDbContext context, IStudentRangService studentRangService, ILogService logService, IEmailService emailService)
        {
            _context = context;
            _studentRangService = studentRangService;
            _logService = logService;
            _emailService = emailService;
        }

        // GET: Firma
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Index()
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            var profil = await GetOrCreateCurrentFirmaProfileAsync(firma);
            var najnovijiOglasi = await _context.Oglasi
                .Where(o => o.FirmaId == firma.Id)
                .OrderByDescending(o => o.DatumObjave)
                .Take(5)
                .ToListAsync();

            var model = new FirmaPocetnaViewModel
            {
                Firma = firma,
                Profil = profil,
                NajnovijiOglasi = najnovijiOglasi,
                BrojOglasa = await _context.Oglasi.CountAsync(o => o.FirmaId == firma.Id),
                BrojAktivnihOglasa = await _context.Oglasi.CountAsync(o => o.FirmaId == firma.Id && o.StatusOglasa == StatusOglasa.AKTIVAN),
                BrojPrijava = await _context.PrijaveOglasa.CountAsync(p => p.Oglas.FirmaId == firma.Id)
            };

            await _logService.InfoAsync("FIRMA_DASHBOARD_PREGLEDAN", $"Firma ID {firma.Id} je otvorila dashboard firme.");
            return View(model);
        }

        // GET: Firma/MojiOglasi
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> MojiOglasi()
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            var oglasi = await _context.Oglasi
                .Where(o => o.FirmaId == firma.Id)
                .OrderByDescending(o => o.DatumObjave)
                .ToListAsync();

            ViewData["BrojPrijavaPoOglasu"] = await _context.PrijaveOglasa
                .Where(p => p.Oglas.FirmaId == firma.Id)
                .GroupBy(p => p.OglasId)
                .Select(g => new { OglasId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.OglasId, x => x.Broj);

            await _logService.InfoAsync("FIRMA_OGLASI_PREGLEDANI", $"Firma ID {firma.Id} je pregledala svoje oglase. Broj oglasa: {oglasi.Count}.");
            return View(oglasi);
        }

        // GET: Firma/RangLista
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> RangLista()
        {
            var rangLista = await _studentRangService.GetRangListaAsync();
            await _logService.InfoAsync("RANG_STUDENATA_PREGLEDAN", $"Firma je pregledala rang listu studenata. Broj prikazanih studenata: {rangLista.Count}.");
            return View(rangLista);
        }

        // POST: Firma/KontaktirajStudenta/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> KontaktirajStudenta(long id)
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            var vecKontaktiran = await _context.Ponude.AnyAsync(p =>
                p.PosiljalacId == firma.Id &&
                p.PrimalacId == student.Id &&
                p.TipPonude == TipPonude.FIRMA_STUDENTU &&
                p.Status == StatusPonude.POSLANO);

            if (vecKontaktiran)
            {
                await _logService.WarningAsync("STUDENT_VEC_KONTAKTIRAN", $"Firma ID {firma.Id} je pokusala ponovo kontaktirati studenta ID {student.Id}.");
                TempData["StatusMessage"] = "Student je vec kontaktiran.";
                return RedirectToAction(nameof(RangLista));
            }

            var nazivFirme = string.IsNullOrWhiteSpace(firma.Naziv) ? firma.Email : firma.Naziv;
            _context.Ponude.Add(new Ponuda
            {
                PosiljalacId = firma.Id,
                PrimalacId = student.Id,
                TekstPoruke = $"{nazivFirme} zeli stupiti u kontakt sa studentom {student.Ime} {student.Prezime}.",
                DatumSlanja = DateTime.UtcNow,
                Status = StatusPonude.POSLANO,
                TipPonude = TipPonude.FIRMA_STUDENTU
            });

            await _emailService.PosaljiAsync(
            student.Email,
            "Ponuda za saradnju – ETF Talent Program",
            $"""
            <h2>Odabrani ste za saradnju sa firmom {firma.Naziv}!</h2>
            <p>Poštovani/a {student.Ime},</p>
            <p>firma {firma.Naziv} željela bi stupiti u kontakt s vama. Ponuda za posao/praksu je poslana kroz sistem. Firmu također možete direktno kontaktirati i putem sljedećeg emaila: {firma.KontaktEmail}</p>
            <br>
            <p>Sretno!</p>
            <br>
            <p>ETF Talent Program tim</p>
            """,
            firma.KontaktEmail
        );

            await _context.SaveChangesAsync();
            await _logService.InfoAsync("STUDENT_KONTAKTIRAN", $"Firma ID {firma.Id} je kontaktirala studenta ID {student.Id} kroz ponudu.");
            TempData["StatusMessage"] = "Student je kontaktiran kroz ponudu. Email obavijest poslana.";

            return RedirectToAction(nameof(RangLista));
        }

        // GET: Firma/Uredi_oglas/5
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Uredi_oglas(long id)
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            var oglas = await _context.Oglasi
                .FirstOrDefaultAsync(o => o.Id == id && o.FirmaId == firma.Id);

            if (oglas == null)
            {
                return NotFound();
            }

            return View(oglas);
        }

        // POST: Firma/Uredi_oglas/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Uredi_oglas(long id, [Bind("Id,Naslov,Opis,Tehnologije,RokPrijave,DatumObjave,TipOglasa,TipAngazmana,StatusOglasa,Lokacija,MinRang,MinProsjek,Kompenzacija")] Oglas oglas)
        {
            if (id != oglas.Id)
            {
                return NotFound();
            }

            var firma = await GetOrCreateCurrentFirmaAsync();
            var existingOglas = await _context.Oglasi
                .FirstOrDefaultAsync(o => o.Id == id && o.FirmaId == firma.Id);

            if (existingOglas == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Oglas.Firma));

            if (!ModelState.IsValid)
            {
                oglas.FirmaId = firma.Id;
                return View(oglas);
            }

            existingOglas.Naslov = oglas.Naslov;
            existingOglas.Opis = oglas.Opis;
            existingOglas.Tehnologije = oglas.Tehnologije;
            existingOglas.RokPrijave = oglas.RokPrijave;
            existingOglas.DatumObjave = oglas.DatumObjave == default ? existingOglas.DatumObjave : oglas.DatumObjave;
            existingOglas.TipOglasa = oglas.TipOglasa;
            existingOglas.TipAngazmana = oglas.TipAngazmana;
            existingOglas.StatusOglasa = oglas.StatusOglasa;
            existingOglas.Lokacija = oglas.Lokacija;
            existingOglas.MinRang = oglas.MinRang;
            existingOglas.MinProsjek = oglas.MinProsjek;
            existingOglas.Kompenzacija = oglas.Kompenzacija?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();
            await _logService.InfoAsync("FIRMA_OGLAS_AZURIRAN", $"Firma ID {firma.Id} je azurirala oglas ID {existingOglas.Id}: {existingOglas.Naslov}.");
            TempData["StatusMessage"] = "Oglas je uspješno ažuriran.";

            return RedirectToAction(nameof(MojiOglasi));
        }

        // POST: Firma/Skini_oglas/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Skini_oglas(long id)
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            var oglas = await _context.Oglasi
                .FirstOrDefaultAsync(o => o.Id == id && o.FirmaId == firma.Id);

            if (oglas == null)
            {
                return NotFound();
            }

            oglas.StatusOglasa = StatusOglasa.ARHIVIRAN;
            await _context.SaveChangesAsync();
            await _logService.WarningAsync("FIRMA_OGLAS_ARHIVIRAN", $"Firma ID {firma.Id} je skinula oglas ID {oglas.Id}: {oglas.Naslov}.");

            TempData["StatusMessage"] = "Oglas je skinut sa aktivnih oglasa.";
            return RedirectToAction(nameof(MojiOglasi));
        }

        // GET: Firma/Prijave/5
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Prijave(long id)
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            var oglas = await _context.Oglasi
                .Include(o => o.Firma)
                .FirstOrDefaultAsync(o => o.Id == id && o.FirmaId == firma.Id);

            if (oglas == null)
            {
                return NotFound();
            }

            var prijave = await _context.PrijaveOglasa
                .Include(p => p.Student)
                .Where(p => p.OglasId == oglas.Id)
                .OrderByDescending(p => p.DatumPrijave)
                .ToListAsync();

            ViewData["Oglas"] = oglas;
            ViewData["StudentProfili"] = await _context.StudentProfili
                .Where(p => prijave.Select(x => x.StudentId).Contains(p.StudentId))
                .ToDictionaryAsync(p => p.StudentId);

            await _logService.InfoAsync("FIRMA_PRIJAVE_OGLASA_PREGLEDANE", $"Firma ID {firma.Id} je pregledala prijave za oglas ID {oglas.Id}. Broj prijava: {prijave.Count}.");
            return View(prijave);
        }

        // POST: Firma/PromijeniStatusPrijave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> PromijeniStatusPrijave(long id, StatusPrijave status)
        {
            if (status != StatusPrijave.PRIHVACENA && status != StatusPrijave.ODBIJENA && status != StatusPrijave.PREGLEDANA)
            {
                return BadRequest();
            }

            var firma = await GetOrCreateCurrentFirmaAsync();
            var prijava = await _context.PrijaveOglasa
                .Include(p => p.Oglas)
                .FirstOrDefaultAsync(p => p.Id == id && p.Oglas.FirmaId == firma.Id);

            if (prijava == null)
            {
                return NotFound();
            }

            prijava.StatusPrijave = status;
            prijava.DatumOdgovora = status == StatusPrijave.PREGLEDANA ? prijava.DatumOdgovora : DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _logService.InfoAsync("STATUS_PRIJAVE_PROMIJENJEN", $"Firma ID {firma.Id} je promijenila status prijave ID {prijava.Id} na {status}.");
            TempData["StatusMessage"] = $"Status prijave je promijenjen u {status}.";

            return RedirectToAction(nameof(Prijave), new { id = prijava.OglasId });
        }

        // GET: Firma/Details/5
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firma = await _context.Firme
                .FirstOrDefaultAsync(m => m.Id == id);
            if (firma == null)
            {
                return NotFound();
            }

            return View(firma);
        }

        // GET: Firma/Create
        [Authorize(Roles = AppRoles.Referent)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Firma/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Create([Bind("Naziv,OpisFirme,Lokacija,Website,KontaktEmail,IndustrijskiSektor,VelicinaFirme,GodinaOsnivanja,Id,Email,Lozinka,Uloga,Status,DatumRegistracije,DatumZadnjePrijave")] Firma firma)
        {
            if (ModelState.IsValid)
            {
                _context.Add(firma);
                await _context.SaveChangesAsync();
                await _logService.InfoAsync("FIRMA_REFERENT_KREIRANA", $"Referent je kreirao firmu ID {firma.Id}: {firma.Email}.");
                return RedirectToAction(nameof(Index));
            }
            return View(firma);
        }

        // GET: Firma/Edit/5
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firma = await _context.Firme.FindAsync(id);
            if (firma == null)
            {
                return NotFound();
            }
            return View(firma);
        }

        // POST: Firma/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Edit(long id, [Bind("Naziv,OpisFirme,Lokacija,Website,KontaktEmail,IndustrijskiSektor,VelicinaFirme,GodinaOsnivanja,Id,Email,Lozinka,Uloga,Status,DatumRegistracije,DatumZadnjePrijave")] Firma firma)
        {
            if (id != firma.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(firma);
                    await _context.SaveChangesAsync();
                    await _logService.InfoAsync("FIRMA_REFERENT_AZURIRANA", $"Referent je azurirao firmu ID {firma.Id}: {firma.Email}.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FirmaExists(firma.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(firma);
        }

        // GET: Firma/Delete/5
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firma = await _context.Firme
                .FirstOrDefaultAsync(m => m.Id == id);
            if (firma == null)
            {
                return NotFound();
            }

            return View(firma);
        }

        // POST: Firma/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var firma = await _context.Firme.FindAsync(id);
            if (firma != null)
            {
                _context.Firme.Remove(firma);
                await _logService.WarningAsync("FIRMA_REFERENT_OBRISANA", $"Referent je obrisao firmu ID {firma.Id}: {firma.Email}.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FirmaExists(long id)
        {
            return _context.Firme.Any(e => e.Id == id);
        }

        // GET: Firma/Profil_firme
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Profil_firme()
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            var firmaProfil = await GetOrCreateCurrentFirmaProfileAsync(firma);
            PopulateFirmaProfileViewData(firma);

            return View(firmaProfil);
        }

        // POST: Firma/Profil_firme
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Profil_firme(
    [Bind("Id,KratakOpis,PunOpis,Lokacija,Website,KontaktEmail,Logotip,TehnologijeStack,DatumAzuriranja,FirmaId")] FirmaProfil firmaProfil,
    string naziv,
    string opisFirme,
    string industrijskiSektor,
    VelicinaFirme velicinaFirme,
    int godinaOsnivanja)
        {
            // FIX 1: Uklanjamo Logotip iz validacije da nas ne blokira ako je prazan
            ModelState.Remove("Logotip");
            ModelState.Remove(nameof(firmaProfil.Firma));

            if (ModelState.IsValid)
            {
                try
                {
                    var firma = await GetOrCreateCurrentFirmaAsync();
                    var existingProfil = await _context.FirmaProfili.FirstOrDefaultAsync(f => f.Id == firmaProfil.Id && f.FirmaId == firma.Id);
                    if (existingProfil == null)
                    {
                        return Forbid();
                    }

                    // Ažuriranje podataka osnovne tabele Firma
                    firma.Naziv = NormalizeText(naziv);
                    firma.OpisFirme = NormalizeText(opisFirme);
                    firma.Lokacija = NormalizeText(firmaProfil.Lokacija);
                    firma.Website = NormalizeText(firmaProfil.Website);
                    firma.KontaktEmail = NormalizeText(firmaProfil.KontaktEmail);
                    firma.IndustrijskiSektor = NormalizeText(industrijskiSektor);
                    firma.VelicinaFirme = velicinaFirme;
                    firma.GodinaOsnivanja = godinaOsnivanja <= 0 ? DateTime.Today.Year : godinaOsnivanja;

                    // Ažuriranje podataka u FirmaProfil
                    existingProfil.KratakOpis = firmaProfil.KratakOpis;
                    existingProfil.PunOpis = firmaProfil.PunOpis;
                    existingProfil.Lokacija = firmaProfil.Lokacija;
                    existingProfil.Website = firmaProfil.Website;
                    existingProfil.KontaktEmail = firmaProfil.KontaktEmail;
                    existingProfil.Logotip = firmaProfil.Logotip;
                    existingProfil.TehnologijeStack = firmaProfil.TehnologijeStack;
                    existingProfil.DatumAzuriranja = DateTime.UtcNow;

                    // FIX 2: Automatski postavljamo status na NA_CEKANJU čim se desi izmjena
                    existingProfil.StatusVerifikacije = StatusVerifikacije.NA_CEKANJU;

                    await _context.SaveChangesAsync();
                    await _logService.InfoAsync("FIRMA_PROFIL_AZURIRAN", $"Firma ID {firma.Id} je azurirala profil firme i poslala ga na ponovnu verifikaciju.");

                    TempData["StatusMessage"] = "Profil firme je uspješno ažuriran i poslan na ponovnu verifikaciju.";

                    // FIX 3: Redirekcija na Index (Dashboard) umjesto na Profil_firme (Edit prozor)
                    // Tako ćeš odmah na početnoj strani vidjeti promjenu podataka i žuti status "Na čekanju"
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FirmaProfilExists(firmaProfil.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            PopulateFirmaProfileViewData(await GetOrCreateCurrentFirmaAsync());
            return View(firmaProfil);
        }

        private bool FirmaProfilExists(long id)
        {
            return _context.FirmaProfili.Any(e => e.Id == id);
        }

        // GET: Firma/Objavi_oglas
        [Authorize(Roles = AppRoles.Firma)]
        public IActionResult Objavi_oglas()
        {
            return View(new Oglas
            {
                DatumObjave = DateTime.Today,
                RokPrijave = DateTime.Today.AddDays(30),
                StatusOglasa = StatusOglasa.AKTIVAN
            });
        }

        // POST: Firma/Objavi_oglas
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Objavi_oglas([Bind("Id,Naslov,Opis,Tehnologije,RokPrijave,DatumObjave,TipOglasa,TipAngazmana,StatusOglasa,Lokacija,MinRang,MinProsjek,Kompenzacija,FirmaId")] Oglas oglas)
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            oglas.FirmaId = firma.Id;
            oglas.Kompenzacija = oglas.Kompenzacija?.Trim() ?? string.Empty;
            ModelState.Remove(nameof(Oglas.Firma));

            if (ModelState.IsValid)
            {
                oglas.DatumObjave = oglas.DatumObjave == default ? DateTime.Today : oglas.DatumObjave;
                oglas.RokPrijave = oglas.RokPrijave == default ? DateTime.Today.AddDays(30) : oglas.RokPrijave;
                _context.Add(oglas);
                await _context.SaveChangesAsync();
                await _logService.InfoAsync("FIRMA_OGLAS_OBJAVLJEN", $"Firma ID {firma.Id} je objavila oglas ID {oglas.Id}: {oglas.Naslov}.");
                TempData["StatusMessage"] = "Oglas je uspješno objavljen.";
                return RedirectToAction(nameof(Index));
            }
            return View(oglas);
        }

        private async Task<Firma> GetOrCreateCurrentFirmaAsync()
        {
            var email = User.Identity?.Name ?? string.Empty;
            var firma = await _context.Firme.FirstOrDefaultAsync(f => f.Email == email);

            if (firma != null)
            {
                return firma;
            }

            firma = new Firma
            {
                Naziv = GetNameFromEmail(email),
                OpisFirme = string.Empty,
                Lokacija = string.Empty,
                Website = string.Empty,
                KontaktEmail = email,
                IndustrijskiSektor = string.Empty,
                VelicinaFirme = VelicinaFirme.MALA,
                GodinaOsnivanja = DateTime.Today.Year,
                Email = email,
                Lozinka = string.Empty,
                Uloga = Uloga.FIRMA,
                Status = Status.AKTIVAN,
                DatumRegistracije = DateTime.UtcNow,
                DatumZadnjePrijave = DateTime.UtcNow
            };

            _context.Firme.Add(firma);
            await _context.SaveChangesAsync();

            return firma;
        }

        private async Task<FirmaProfil> GetOrCreateCurrentFirmaProfileAsync(Firma firma)
        {
            var profil = await _context.FirmaProfili.FirstOrDefaultAsync(f => f.FirmaId == firma.Id);
            if (profil != null)
            {
                return profil;
            }

            profil = new FirmaProfil
            {
                FirmaId = firma.Id,
                KratakOpis = string.Empty,
                PunOpis = string.Empty,
                Lokacija = firma.Lokacija,
                Website = firma.Website,
                KontaktEmail = firma.KontaktEmail,
                Logotip = string.Empty,
                TehnologijeStack = string.Empty,
                DatumAzuriranja = DateTime.UtcNow,
                StatusVerifikacije = StatusVerifikacije.NA_CEKANJU
            };

            _context.FirmaProfili.Add(profil);
            await _context.SaveChangesAsync();

            return profil;
        }

        private static string GetNameFromEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            return atIndex > 0 ? email[..atIndex] : email;
        }

        private void PopulateFirmaProfileViewData(Firma firma)
        {
            ViewData["Naziv"] = firma.Naziv;
            ViewData["OpisFirme"] = firma.OpisFirme;
            ViewData["IndustrijskiSektor"] = firma.IndustrijskiSektor;
            ViewData["VelicinaFirme"] = firma.VelicinaFirme;
            ViewData["GodinaOsnivanja"] = firma.GodinaOsnivanja;
        }

        private static string NormalizeText(string? value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}
