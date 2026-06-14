using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using ETFTalentProgram.Services;

namespace ETFTalentProgram.Controllers
{
    public class AkademskiPodatakController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public AkademskiPodatakController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: AkademskiPodatak
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AkademskiPodaci.Include(a => a.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AkademskiPodatak/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akademskiPodatak = await _context.AkademskiPodaci
                .Include(a => a.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (akademskiPodatak == null)
            {
                return NotFound();
            }

            return View(akademskiPodatak);
        }

        // GET: AkademskiPodatak/Create
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id");
            return View();
        }

        // POST: AkademskiPodatak/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Predmet,SifraPredmeta,Ocjena,ECTS,GodinaPolaganja,Semestar,StudentId")] AkademskiPodatak akademskiPodatak)
        {
            if (ModelState.IsValid)
            {
                _context.Add(akademskiPodatak);
                await _context.SaveChangesAsync();
                await _logService.InfoAsync("AKADEMSKI_PODATAK_KREIRAN", $"Kreiran akademski podatak ID {akademskiPodatak.Id} za studenta ID {akademskiPodatak.StudentId}.");
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", akademskiPodatak.StudentId);
            return View(akademskiPodatak);
        }

        // GET: AkademskiPodatak/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akademskiPodatak = await _context.AkademskiPodaci.FindAsync(id);
            if (akademskiPodatak == null)
            {
                return NotFound();
            }
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", akademskiPodatak.StudentId);
            return View(akademskiPodatak);
        }

        // POST: AkademskiPodatak/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Predmet,SifraPredmeta,Ocjena,ECTS,GodinaPolaganja,Semestar,StudentId")] AkademskiPodatak akademskiPodatak)
        {
            if (id != akademskiPodatak.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(akademskiPodatak);
                    await _context.SaveChangesAsync();
                    await _logService.InfoAsync("AKADEMSKI_PODATAK_AZURIRAN", $"Azuriran akademski podatak ID {akademskiPodatak.Id} za studenta ID {akademskiPodatak.StudentId}.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkademskiPodatakExists(akademskiPodatak.Id))
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
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", akademskiPodatak.StudentId);
            return View(akademskiPodatak);
        }

        // GET: AkademskiPodatak/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akademskiPodatak = await _context.AkademskiPodaci
                .Include(a => a.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (akademskiPodatak == null)
            {
                return NotFound();
            }

            return View(akademskiPodatak);
        }

        // POST: AkademskiPodatak/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var akademskiPodatak = await _context.AkademskiPodaci.FindAsync(id);
            if (akademskiPodatak != null)
            {
                _context.AkademskiPodaci.Remove(akademskiPodatak);
                await _logService.WarningAsync("AKADEMSKI_PODATAK_OBRISAN", $"Obrisan akademski podatak ID {akademskiPodatak.Id} za studenta ID {akademskiPodatak.StudentId}.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AkademskiPodatakExists(long id)
        {
            return _context.AkademskiPodaci.Any(e => e.Id == id);
        }
    }
}
