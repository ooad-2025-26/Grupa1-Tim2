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
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStudentRangService _studentRangService;
        private readonly ILogService _logService;

        public StudentController(ApplicationDbContext context, IStudentRangService studentRangService, ILogService logService)
        {
            _context = context;
            _studentRangService = studentRangService;
            _logService = logService;
        }

        // GET: Student
        [Authorize(Roles = AppRoles.Student)]
        public async Task<IActionResult> Index()
        {
            var student = await GetOrCreateCurrentStudentAsync();
            var profil = await GetOrCreateCurrentStudentProfileAsync(student);
            var najnovijiOglasi = await _context.Oglasi
                .Include(o => o.Firma)
                .Where(o => o.StatusOglasa == StatusOglasa.AKTIVAN)
                .OrderByDescending(o => o.DatumObjave)
                .Take(5)
                .ToListAsync();

            var model = new StudentPocetnaViewModel
            {
                Student = student,
                Profil = profil,
                NajnovijiOglasi = najnovijiOglasi,
                BrojPrijava = await _context.PrijaveOglasa.CountAsync(p => p.StudentId == student.Id),
                BrojPonuda = await _context.Ponude.CountAsync(p => p.PrimalacId == student.Id),
                BrojProjekata = CountProjectItems(profil.Projekti)
            };

            await _logService.InfoAsync("STUDENT_DASHBOARD_PREGLEDAN", $"Student ID {student.Id} je otvorio studentski dashboard.");
            return View(model);
        }

        // GET: Student/MojePrijave
        [Authorize(Roles = AppRoles.Student)]
        public async Task<IActionResult> MojePrijave()
        {
            var student = await GetOrCreateCurrentStudentAsync();
            var prijave = await _context.PrijaveOglasa
                .Include(p => p.Oglas)
                    .ThenInclude(o => o.Firma)
                .Where(p => p.StudentId == student.Id)
                .OrderByDescending(p => p.DatumPrijave)
                .ToListAsync();

            await _logService.InfoAsync("STUDENT_PRIJAVE_PREGLEDANE", $"Student ID {student.Id} je pregledao svoje prijave. Broj prijava: {prijave.Count}.");
            return View(prijave);
        }

        // GET: Student/RangLista
        [Authorize(Roles = $"{AppRoles.Student},{AppRoles.Firma}")]
        public async Task<IActionResult> RangLista()
        {
            var rangLista = await _studentRangService.GetRangListaAsync();
            await _logService.InfoAsync("RANG_STUDENATA_PREGLEDAN", $"Korisnik je pregledao rang listu studenata. Broj prikazanih studenata: {rangLista.Count}.");
            return View(rangLista);
        }

        // GET: Student/Details/5
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Student/Create
        [Authorize(Roles = AppRoles.Referent)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Create([Bind("Ime,Prezime,BrIndeksa,GodinaStudija,GodinaUpisa,ProsjekOcjena,Verificiran,Id,Email,Lozinka,Uloga,Status,DatumRegistracije,DatumZadnjePrijave")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                await _logService.InfoAsync("STUDENT_REFERENT_KREIRAN", $"Referent je kreirao studenta ID {student.Id}: {student.Email}.");
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Student/Edit/5
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Student/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Edit(long id, [Bind("Ime,Prezime,BrIndeksa,GodinaStudija,GodinaUpisa,ProsjekOcjena,Verificiran,Id,Email,Lozinka,Uloga,Status,DatumRegistracije,DatumZadnjePrijave")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                    await _logService.InfoAsync("STUDENT_REFERENT_AZURIRAN", $"Referent je azurirao studenta ID {student.Id}: {student.Email}.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
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
            return View(student);
        }

        // GET: Student/Delete/5
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student != null)
            {
                _context.Studenti.Remove(student);
                await _logService.WarningAsync("STUDENT_REFERENT_OBRISAN", $"Referent je obrisao studenta ID {student.Id}: {student.Email}.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(long id)
        {
            return _context.Studenti.Any(e => e.Id == id);
        }

        private async Task<Student> GetOrCreateCurrentStudentAsync()
        {
            var email = User.Identity?.Name ?? string.Empty;
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.Email == email);

            if (student != null)
            {
                return student;
            }

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
            await _context.SaveChangesAsync();

            return student;
        }

        private async Task<StudentProfil> GetOrCreateCurrentStudentProfileAsync(Student student)
        {
            var profil = await _context.StudentProfili.FirstOrDefaultAsync(p => p.StudentId == student.Id);
            if (profil != null)
            {
                return profil;
            }

            profil = new StudentProfil
            {
                StudentId = student.Id,
                Rang = 0,
                Biografija = string.Empty,
                Vjestine = string.Empty,
                PreferiraneTehnologije = string.Empty,
                Projekti = string.Empty,
                PreferiraneLokacije = string.Empty,
                DostupanOd = DateTime.Today,
                DatumAzuriranja = DateTime.UtcNow,
                StatusVerifikacije = StatusVerifikacije.NA_CEKANJU
            };

            _context.StudentProfili.Add(profil);
            await _context.SaveChangesAsync();

            return profil;
        }

        private static int CountProjectItems(string? value)
        {
            return (value ?? string.Empty)
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Length;
        }

        private static string GetNameFromEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            return atIndex > 0 ? email[..atIndex] : email;
        }
    }
}
