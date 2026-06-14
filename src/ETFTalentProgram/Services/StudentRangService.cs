using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using ETFTalentProgram.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ETFTalentProgram.Services
{
    public class StudentRangService : IStudentRangService
    {
        private readonly ApplicationDbContext _context;

        public StudentRangService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<StudentRangViewModel>> GetRangListaAsync()
        {
            var studenti = await _context.Studenti
                .OrderBy(s => s.Prezime)
                .ThenBy(s => s.Ime)
                .ToListAsync();

            var studentIds = studenti.Select(s => s.Id).ToList();
            var profili = await _context.StudentProfili
                .Where(p => studentIds.Contains(p.StudentId))
                .ToDictionaryAsync(p => p.StudentId);

            var akademskiPodaci = await _context.AkademskiPodaci
                .Where(a => studentIds.Contains(a.StudentId))
                .ToListAsync();

            var akademskiPoStudentu = akademskiPodaci
                .GroupBy(a => a.StudentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var rangLista = new List<StudentRangViewModel>();

            foreach (var student in studenti)
            {
                profili.TryGetValue(student.Id, out var profil);
                akademskiPoStudentu.TryGetValue(student.Id, out var predmeti);
                predmeti ??= [];

                var brojProjekata = CountProjectItems(profil?.Projekti);
                var rang = CalculateRang(student, predmeti, brojProjekata);

                if (profil != null && Math.Abs(profil.Rang - rang) > 0.001)
                {
                    profil.Rang = rang;
                    profil.DatumAzuriranja = DateTime.UtcNow;
                }

                if (!student.Verificiran)
                {
                    continue;
                }

                rangLista.Add(new StudentRangViewModel
                {
                    StudentId = student.Id,
                    ImePrezime = $"{student.Ime} {student.Prezime}".Trim(),
                    Email = student.Email,
                    BrojIndeksa = student.BrIndeksa,
                    ProsjekOcjena = predmeti.Count != 0 ? Math.Round(predmeti.Average(p => p.Ocjena), 2) : student.ProsjekOcjena,
                    BrojPolozenihPredmeta = predmeti.Count,
                    UkupnoEcts = predmeti.Sum(p => p.ECTS),
                    BrojProjekata = brojProjekata,
                    Vjestine = profil?.Vjestine ?? string.Empty,
                    PreferiraneTehnologije = profil?.PreferiraneTehnologije ?? string.Empty,
                    Rang = rang
                });
            }

            await _context.SaveChangesAsync();

            return rangLista
                .OrderByDescending(s => s.Rang)
                .ThenBy(s => s.ImePrezime)
                .ToList();
        }

        private static double CalculateRang(Student student, IReadOnlyCollection<AkademskiPodatak> predmeti, int brojProjekata)
        {
            var prosjek = predmeti.Count != 0 ? predmeti.Average(p => p.Ocjena) : student.ProsjekOcjena;
            var ects = predmeti.Sum(p => p.ECTS);

            var score = (prosjek * 10) + (ects * 0.5) + (brojProjekata * 5);
            if (student.Verificiran)
            {
                score += 5;
            }

            return Math.Round(score, 2);
        }

        private static int CountProjectItems(string? value)
        {
            return (value ?? string.Empty)
                .Split(["\r\n", "\n", ";"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Length;
        }
    }
}
