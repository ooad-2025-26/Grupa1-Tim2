using ETFTalentProgram.Constants;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using ETFTalentProgram.Services;
using ETFTalentProgram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ETFTalentProgram.Controllers
{
    [Authorize(Roles = AppRoles.Referent)]
    public class VerifikacijaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifikacijaController(
            ApplicationDbContext context,
            ILogService logService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logService = logService;
            _userManager = userManager;
        }

        // GET: Verifikacija/Lista
        public async Task<IActionResult> Lista()
        {
            await EnsureIdentityProfilesForVerificationAsync();

            var model = new VerifikacijaProfilaIndexViewModel
            {
                StudentProfili = await _context.StudentProfili
                    .Include(p => p.Student)
                    .Where(p => p.Student.Status == Status.AKTIVAN)
                    .OrderBy(p => p.StatusVerifikacije)
                    .ThenByDescending(p => p.DatumAzuriranja)
                    .Select(p => new StudentProfilVerifikacijaViewModel
                    {
                        ProfilId = p.Id,
                        StudentId = p.StudentId,
                        ImePrezime = (p.Student.Ime + " " + p.Student.Prezime).Trim(),
                        Email = p.Student.Email,
                        BrojIndeksa = p.Student.BrIndeksa,
                        Rang = p.Rang,
                        StatusVerifikacije = p.StatusVerifikacije,
                        DatumAzuriranja = p.DatumAzuriranja
                    })
                    .ToListAsync(),
                FirmaProfili = await _context.FirmaProfili
                    .Include(p => p.Firma)
                    .Where(p => p.Firma.Status == Status.AKTIVAN)
                    .OrderBy(p => p.StatusVerifikacije)
                    .ThenByDescending(p => p.DatumAzuriranja)
                    .Select(p => new FirmaProfilVerifikacijaViewModel
                    {
                        ProfilId = p.Id,
                        FirmaId = p.FirmaId,
                        Naziv = p.Firma.Naziv,
                        Email = p.Firma.Email,
                        Lokacija = p.Lokacija,
                        Website = p.Website,
                        StatusVerifikacije = p.StatusVerifikacije,
                        DatumAzuriranja = p.DatumAzuriranja
                    })
                    .ToListAsync()
            };

            return View("Index", model);
        }

        // GET: Verifikacija
        public async Task<IActionResult> Index()
        {
            return await Lista();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifikujStudentProfil(long id)
        {
            var profil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profil == null)
            {
                return NotFound();
            }

            profil.StatusVerifikacije = StatusVerifikacije.VERIFICIRAN;
            profil.Student.Verificiran = true;
            profil.DatumAzuriranja = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _logService.InfoAsync("STUDENT_PROFIL_VERIFIKOVAN", $"Referent je verifikovao profil studenta ID {profil.StudentId}.");
            TempData["StatusMessage"] = "Studentski profil je verifikovan.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OdbijStudentProfil(long id)
        {
            var profil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profil == null)
            {
                return NotFound();
            }

            profil.StatusVerifikacije = StatusVerifikacije.ODBIJEN;
            profil.Student.Verificiran = false;
            profil.DatumAzuriranja = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _logService.WarningAsync("STUDENT_PROFIL_ODBIJEN", $"Referent je odbio profil studenta ID {profil.StudentId}.");
            TempData["StatusMessage"] = "Studentski profil je odbijen.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifikujFirmaProfil(long id)
        {
            var profil = await _context.FirmaProfili.FindAsync(id);
            if (profil == null)
            {
                return NotFound();
            }

            profil.StatusVerifikacije = StatusVerifikacije.VERIFICIRAN;
            profil.DatumAzuriranja = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _logService.InfoAsync("FIRMA_PROFIL_VERIFIKOVAN", $"Referent je verifikovao profil firme ID {profil.FirmaId}.");
            TempData["StatusMessage"] = "Profil firme je verifikovan.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OdbijFirmaProfil(long id)
        {
            var profil = await _context.FirmaProfili.FindAsync(id);
            if (profil == null)
            {
                return NotFound();
            }

            profil.StatusVerifikacije = StatusVerifikacije.ODBIJEN;
            profil.DatumAzuriranja = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _logService.WarningAsync("FIRMA_PROFIL_ODBIJEN", $"Referent je odbio profil firme ID {profil.FirmaId}.");
            TempData["StatusMessage"] = "Profil firme je odbijen.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Verifikacija/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verifikacija = await _context.Verifikacije
                .Include(v => v.Referent)
                .Include(v => v.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (verifikacija == null)
            {
                return NotFound();
            }

            return View(verifikacija);
        }

        // GET: Verifikacija/Create
        public IActionResult Create()
        {
            ViewData["ReferentId"] = new SelectList(_context.Referenti, "Id", "Id");
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id");
            return View();
        }

        // POST: Verifikacija/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DatumPodnosenja,DatumVerifikacije,StatusVerifikacije,Komentar,Dokumenti,StudentId,ReferentId")] Verifikacija verifikacija)
        {
            if (ModelState.IsValid)
            {
                _context.Add(verifikacija);
                await _context.SaveChangesAsync();
                await _logService.InfoAsync("VERIFIKACIJA_KREIRANA", $"Kreirana verifikacija ID {verifikacija.Id} za studenta ID {verifikacija.StudentId}.");
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReferentId"] = new SelectList(_context.Referenti, "Id", "Id", verifikacija.ReferentId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", verifikacija.StudentId);
            return View(verifikacija);
        }

        // GET: Verifikacija/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verifikacija = await _context.Verifikacije.FindAsync(id);
            if (verifikacija == null)
            {
                return NotFound();
            }
            ViewData["ReferentId"] = new SelectList(_context.Referenti, "Id", "Id", verifikacija.ReferentId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", verifikacija.StudentId);
            return View(verifikacija);
        }

        // POST: Verifikacija/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DatumPodnosenja,DatumVerifikacije,StatusVerifikacije,Komentar,Dokumenti,StudentId,ReferentId")] Verifikacija verifikacija)
        {
            if (id != verifikacija.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(verifikacija);
                    await _context.SaveChangesAsync();
                    await _logService.InfoAsync("VERIFIKACIJA_AZURIRANA", $"Azurirana verifikacija ID {verifikacija.Id} sa statusom {verifikacija.StatusVerifikacije}.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VerifikacijaExists(verifikacija.Id))
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
            ViewData["ReferentId"] = new SelectList(_context.Referenti, "Id", "Id", verifikacija.ReferentId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", verifikacija.StudentId);
            return View(verifikacija);
        }

        // GET: Verifikacija/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verifikacija = await _context.Verifikacije
                .Include(v => v.Referent)
                .Include(v => v.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (verifikacija == null)
            {
                return NotFound();
            }

            return View(verifikacija);
        }

        // POST: Verifikacija/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var verifikacija = await _context.Verifikacije.FindAsync(id);
            if (verifikacija != null)
            {
                _context.Verifikacije.Remove(verifikacija);
                await _logService.WarningAsync("VERIFIKACIJA_OBRISANA", $"Obrisana verifikacija ID {verifikacija.Id}.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VerifikacijaExists(long id)
        {
            return _context.Verifikacije.Any(e => e.Id == id);
        }

        private async Task EnsureIdentityProfilesForVerificationAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var changed = false;

            foreach (var user in users)
            {
                var email = user.Email ?? user.UserName;
                if (string.IsNullOrWhiteSpace(email))
                {
                    continue;
                }

                if (await _userManager.IsInRoleAsync(user, AppRoles.Student))
                {
                    changed |= await EnsureStudentProfileAsync(email);
                }

                if (await _userManager.IsInRoleAsync(user, AppRoles.Firma))
                {
                    changed |= await EnsureFirmaProfileAsync(email);
                }
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task<bool> EnsureStudentProfileAsync(string email)
        {
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.Email == email);
            var changed = false;

            if (student == null)
            {
                student = new Student
                {
                    Ime = GetNameFromEmail(email),
                    Prezime = string.Empty,
                    BrIndeksa = string.Empty,
                    GodinaStudija = 0,
                    GodinaUpisa = DateTime.Today.Year,
                    ProsjekOcjena = 0,
                    Verificiran = false,
                    Email = email,
                    Lozinka = string.Empty,
                    Uloga = Uloga.STUDENT,
                    Status = Status.AKTIVAN,
                    DatumRegistracije = DateTime.UtcNow,
                    DatumZadnjePrijave = DateTime.UtcNow
                };

                _context.Studenti.Add(student);
                changed = true;
            }
            else if (student.Verificiran)
            {
                var existingStatus = await _context.StudentProfili
                    .Where(p => p.StudentId == student.Id)
                    .Select(p => p.StatusVerifikacije)
                    .FirstOrDefaultAsync();

                if (existingStatus != StatusVerifikacije.VERIFICIRAN)
                {
                    student.Verificiran = false;
                    changed = true;
                }
            }

            var hasProfil = student.Id != 0
                && await _context.StudentProfili.AnyAsync(p => p.StudentId == student.Id);

            if (!hasProfil)
            {
                _context.StudentProfili.Add(new StudentProfil
                {
                    Student = student,
                    Rang = 0,
                    Biografija = string.Empty,
                    Vjestine = string.Empty,
                    PreferiraneTehnologije = string.Empty,
                    Projekti = string.Empty,
                    PreferiraneLokacije = string.Empty,
                    DostupanOd = DateTime.Today,
                    DatumAzuriranja = DateTime.UtcNow,
                    StatusVerifikacije = StatusVerifikacije.NA_CEKANJU
                });

                changed = true;
            }

            return changed;
        }

        private async Task<bool> EnsureFirmaProfileAsync(string email)
        {
            var firma = await _context.Firme.FirstOrDefaultAsync(f => f.Email == email);
            var changed = false;

            if (firma == null)
            {
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
                changed = true;
            }

            var hasProfil = firma.Id != 0
                && await _context.FirmaProfili.AnyAsync(p => p.FirmaId == firma.Id);

            if (!hasProfil)
            {
                _context.FirmaProfili.Add(new FirmaProfil
                {
                    Firma = firma,
                    KratakOpis = string.Empty,
                    PunOpis = string.Empty,
                    Lokacija = firma.Lokacija,
                    Website = firma.Website,
                    KontaktEmail = email,
                    Logotip = string.Empty,
                    TehnologijeStack = string.Empty,
                    DatumAzuriranja = DateTime.UtcNow,
                    StatusVerifikacije = StatusVerifikacije.NA_CEKANJU
                });

                changed = true;
            }

            return changed;
        }

        private static string GetNameFromEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            return atIndex > 0 ? email[..atIndex] : email;
        }
    }
}
