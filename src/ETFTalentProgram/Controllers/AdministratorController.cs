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
    public class AdministratorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public AdministratorController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Administrator
        public async Task<IActionResult> Index()
        {
            return View(await _context.Administratori.ToListAsync());
        }

        // GET: Administrator/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrator = await _context.Administratori
                .FirstOrDefaultAsync(m => m.Id == id);
            if (administrator == null)
            {
                return NotFound();
            }

            return View(administrator);
        }

        // GET: Administrator/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Administrator/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ime,Prezime,NivoOvlastenja,Id,Email,Lozinka,Uloga,Status,DatumRegistracije,DatumZadnjePrijave")] Administrator administrator)
        {
            if (ModelState.IsValid)
            {
                _context.Add(administrator);
                await _context.SaveChangesAsync();
                await _logService.InfoAsync("ADMINISTRATOR_KREIRAN", $"Kreiran administrator ID {administrator.Id}: {administrator.Email}.");
                return RedirectToAction(nameof(Index));
            }
            return View(administrator);
        }

        // GET: Administrator/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrator = await _context.Administratori.FindAsync(id);
            if (administrator == null)
            {
                return NotFound();
            }
            return View(administrator);
        }

        // POST: Administrator/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Ime,Prezime,NivoOvlastenja,Id,Email,Lozinka,Uloga,Status,DatumRegistracije,DatumZadnjePrijave")] Administrator administrator)
        {
            if (id != administrator.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(administrator);
                    await _context.SaveChangesAsync();
                    await _logService.InfoAsync("ADMINISTRATOR_AZURIRAN", $"Azuriran administrator ID {administrator.Id}: {administrator.Email}.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdministratorExists(administrator.Id))
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
            return View(administrator);
        }

        // GET: Administrator/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrator = await _context.Administratori
                .FirstOrDefaultAsync(m => m.Id == id);
            if (administrator == null)
            {
                return NotFound();
            }

            return View(administrator);
        }

        // POST: Administrator/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var administrator = await _context.Administratori.FindAsync(id);
            if (administrator != null)
            {
                _context.Administratori.Remove(administrator);
                await _logService.WarningAsync("ADMINISTRATOR_OBRISAN", $"Obrisan administrator ID {administrator.Id}: {administrator.Email}.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdministratorExists(long id)
        {
            return _context.Administratori.Any(e => e.Id == id);
        }
    }
}
