using ETFTalentProgram.Constants;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using ETFTalentProgram.Services;
using ETFTalentProgram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ETFTalentProgram.Controllers
{
    [Authorize(Roles = $"{AppRoles.Student},{AppRoles.Firma},{AppRoles.Referent}")]
    public class StudentProfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public StudentProfilController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        [Authorize(Roles = AppRoles.Student)]
        public async Task<IActionResult> Index()
        {
            var profil = await GetOrCreateCurrentStudentProfileAsync();
            var model = await BuildViewModelAsync(profil, isReadOnly: false, canSendOffer: false);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Student)]
        public async Task<IActionResult> Update(StudentProfilViewModel model)
        {
            var profil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (profil == null || !IsCurrentStudent(profil.Student))
            {
                return Forbid();
            }

            profil.Student.Ime = NormalizeText(model.Ime);
            profil.Student.Prezime = NormalizeText(model.Prezime);
            profil.Student.BrIndeksa = NormalizeText(model.BrojIndeksa);
            profil.Student.GodinaStudija = Math.Clamp(model.GodinaStudija, 0, 10);
            profil.Student.GodinaUpisa = model.GodinaUpisa <= 0 ? DateTime.Today.Year : model.GodinaUpisa;
            profil.Student.ProsjekOcjena = Math.Clamp(model.ProsjekOcjena, 0, 10);
            profil.Student.Verificiran = false;
            profil.Biografija = NormalizeText(model.Biografija);
            profil.Vjestine = NormalizeCommaList(model.Vjestine);
            profil.PreferiraneTehnologije = profil.Vjestine;
            profil.Projekti = NormalizeProjects(model.Projekti);
            profil.PreferiraneLokacije = NormalizeCommaList(model.PreferiraneLokacije);
            profil.DostupanOd = model.DostupanOd == default ? DateTime.Today : model.DostupanOd;
            profil.Rang = CalculateRank(profil.Student, profil.Vjestine, profil.Projekti);
            profil.DatumAzuriranja = DateTime.UtcNow;
            profil.StatusVerifikacije = StatusVerifikacije.NA_CEKANJU;

            await _context.SaveChangesAsync();
            await _logService.InfoAsync("STUDENT_PROFIL_AZURIRAN", $"Azuriran profil studenta ID {profil.StudentId}.");
            TempData["StatusMessage"] = "Profil je uspješno sačuvan i poslan na ponovnu verifikaciju.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(long id)
        {
            var profil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profil == null)
            {
                return NotFound();
            }

            var isOwner = User.IsInRole(AppRoles.Student) && IsCurrentStudent(profil.Student);
            var canView = isOwner || User.IsInRole(AppRoles.Firma) || User.IsInRole(AppRoles.Referent);
            if (!canView)
            {
                return Forbid();
            }

            var model = await BuildViewModelAsync(
                profil,
                isReadOnly: true,
                canSendOffer: User.IsInRole(AppRoles.Firma));

            return View(model);
        }

        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profil == null)
            {
                return NotFound();
            }

            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Email", profil.StudentId);
            return View(profil);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Rang,Biografija,Vjestine,Projekti,PreferiraneLokacije,PreferiraneTehnologije,DostupanOd,DatumAzuriranja,StatusVerifikacije,StudentId")] StudentProfil profil)
        {
            if (id != profil.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(StudentProfil.Student));

            if (!ModelState.IsValid)
            {
                ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Email", profil.StudentId);
                return View(profil);
            }

            var existingProfil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProfil == null)
            {
                return NotFound();
            }

            existingProfil.Rang = profil.Rang;
            existingProfil.Biografija = NormalizeText(profil.Biografija);
            existingProfil.Vjestine = NormalizeCommaList(profil.Vjestine);
            existingProfil.Projekti = NormalizeProjects(profil.Projekti);
            existingProfil.PreferiraneLokacije = NormalizeCommaList(profil.PreferiraneLokacije);
            existingProfil.PreferiraneTehnologije = NormalizeCommaList(profil.PreferiraneTehnologije);
            existingProfil.DostupanOd = profil.DostupanOd == default ? DateTime.Today : profil.DostupanOd;
            existingProfil.DatumAzuriranja = DateTime.UtcNow;
            existingProfil.StatusVerifikacije = profil.StatusVerifikacije;
            existingProfil.Student.Verificiran = profil.StatusVerifikacije == StatusVerifikacije.VERIFICIRAN;

            await _context.SaveChangesAsync();
            await _logService.InfoAsync("STUDENT_PROFIL_REFERENT_AZURIRAN", $"Referent je azurirao profil studenta ID {existingProfil.StudentId} sa statusom {existingProfil.StatusVerifikacije}.");
            TempData["StatusMessage"] = "Studentski profil je azuriran.";

            return RedirectToAction("Index", "Verifikacija");
        }

        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Search(
            string? q,
            double? minimalniRang,
            string? tehnologija,
            string? predmet,
            string? projekat,
            bool samoZavrsnaGodina = true)
        {
            var profili = await _context.StudentProfili
                .Include(p => p.Student)
                .ToListAsync();

            var normalizedQuery = NormalizeText(q).ToLowerInvariant();
            var normalizedTechnology = NormalizeText(tehnologija).ToLowerInvariant();
            var normalizedSubject = NormalizeText(predmet).ToLowerInvariant();
            var normalizedProject = NormalizeText(projekat).ToLowerInvariant();
            var studentIds = profili.Select(p => p.StudentId).ToList();
            var akademskiPodaci = await _context.AkademskiPodaci
                .Where(a => studentIds.Contains(a.StudentId))
                .ToListAsync();

            var studentIdsSaPredmetom = akademskiPodaci
                .Where(a => string.IsNullOrEmpty(normalizedSubject)
                    || a.Predmet.ToLowerInvariant().Contains(normalizedSubject)
                    || a.SifraPredmeta.ToLowerInvariant().Contains(normalizedSubject))
                .Select(a => a.StudentId)
                .ToHashSet();

            var filtered = profili
                .Where(p => string.IsNullOrEmpty(normalizedQuery)
                    || (p.Student != null && $"{p.Student.Ime} {p.Student.Prezime}".ToLowerInvariant().Contains(normalizedQuery))
                    || (p.Vjestine ?? string.Empty).ToLowerInvariant().Contains(normalizedQuery)
                    || (p.Projekti ?? string.Empty).ToLowerInvariant().Contains(normalizedQuery))
                .Where(p => minimalniRang == null || p.Rang >= minimalniRang)
                .Where(p => !samoZavrsnaGodina || (p.Student != null && (p.Student.GodinaStudija >= 3 || p.Student.Verificiran)))
                .Where(p => string.IsNullOrEmpty(normalizedSubject) || studentIdsSaPredmetom.Contains(p.StudentId))
                .Where(p => string.IsNullOrEmpty(normalizedTechnology)
                    || (p.Vjestine ?? string.Empty).ToLowerInvariant().Contains(normalizedTechnology)
                    || (p.PreferiraneTehnologije ?? string.Empty).ToLowerInvariant().Contains(normalizedTechnology))
                .Where(p => string.IsNullOrEmpty(normalizedProject)
                    || (p.Projekti ?? string.Empty).ToLowerInvariant().Contains(normalizedProject))
                .OrderByDescending(p => p.Rang)
                .ThenBy(p => p.Student != null ? p.Student.Prezime : string.Empty)
                .ToList();

            var models = new List<StudentProfilViewModel>();
            foreach (var profil in filtered)
            {
                models.Add(await BuildViewModelAsync(profil, isReadOnly: true, canSendOffer: true));
            }

            ViewData["Query"] = q;
            ViewData["MinimalniRang"] = minimalniRang;
            ViewData["Tehnologija"] = tehnologija;
            ViewData["Predmet"] = predmet;
            ViewData["Projekat"] = projekat;
            ViewData["SamoZavrsnaGodina"] = samoZavrsnaGodina;

            await _logService.InfoAsync(
                "PRETRAGA_STUDENATA",
                $"Firma je pretrazila studente. Query='{NormalizeText(q)}', tehnologija='{NormalizeText(tehnologija)}', predmet='{NormalizeText(predmet)}', projekat='{NormalizeText(projekat)}', minimalniRang='{minimalniRang}', samoZavrsnaGodina='{samoZavrsnaGodina}', brojRezultata={filtered.Count}.");

            return View(models);
        }

        private async Task<StudentProfil> GetOrCreateCurrentStudentProfileAsync()
        {
            var email = User.Identity?.Name ?? string.Empty;
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.Email == email);

            if (student == null)
            {
                student = new Student
                {
                    Ime = email.Contains('@') ? email[..email.IndexOf('@')] : "Student",
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
                await _context.SaveChangesAsync();
            }

            var profil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.StudentId == student.Id);

            if (profil != null)
            {
                return profil;
            }

            profil = new StudentProfil
            {
                StudentId = student.Id,
                Student = student,
                Rang = CalculateRank(student, string.Empty, string.Empty),
                Biografija = string.Empty,
                Vjestine = string.Empty,
                PreferiraneTehnologije = string.Empty,
                Projekti = string.Empty,
                PreferiraneLokacije = string.Empty,
                DostupanOd = DateTime.Today,
                DatumAzuriranja = DateTime.UtcNow,
                StatusVerifikacije = student.Verificiran
                    ? StatusVerifikacije.VERIFICIRAN
                    : StatusVerifikacije.NA_CEKANJU
            };

            _context.StudentProfili.Add(profil);
            await _context.SaveChangesAsync();

            return profil;
        }

        private async Task<StudentProfilViewModel> BuildViewModelAsync(StudentProfil profil, bool isReadOnly, bool canSendOffer)
        {
            if (profil.Student == null)
            {
                await _context.Entry(profil).Reference(p => p.Student).LoadAsync();
            }

            var ects = await _context.AkademskiPodaci
                .Where(a => a.StudentId == profil.StudentId)
                .SumAsync(a => (int?)a.ECTS) ?? 0;

            return StudentProfilViewModel.From(profil, ects, isReadOnly, canSendOffer);
        }

        private bool IsCurrentStudent(Student? student)
        {
            return student != null
                && string.Equals(student.Email, User.Identity?.Name, StringComparison.OrdinalIgnoreCase);
        }

        private static double CalculateRank(Student student, string? vjestine, string? projekti)
        {
            var skillCount = CountCommaItems(vjestine);
            var projectCount = CountProjectItems(projekti);
            var academicScore = Math.Clamp(student.ProsjekOcjena, 0, 10) * 0.6;
            var skillScore = Math.Min(skillCount, 10) / 10.0 * 2;
            var projectScore = Math.Min(projectCount, 5) / 5.0 * 2;

            return Math.Round(academicScore + skillScore + projectScore, 1);
        }

        private static int CountCommaItems(string? value)
        {
            return NormalizeCommaList(value)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Length;
        }

        private static int CountProjectItems(string? value)
        {
            return NormalizeProjects(value)
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Length;
        }

        private static string NormalizeText(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static string NormalizeCommaList(string? value)
        {
            return string.Join(", ", (value ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static string NormalizeProjects(string? value)
        {
            return string.Join(Environment.NewLine, (value ?? string.Empty)
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
