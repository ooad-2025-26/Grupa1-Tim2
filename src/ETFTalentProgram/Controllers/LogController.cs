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

namespace ETFTalentProgram.Controllers
{
    [Authorize(Roles = AppRoles.Administrator)]
    public class LogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public LogController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Log
        public async Task<IActionResult> Index(string? tipAkcije)
        {
            var tipoviAkcija = await _context.Logovi
                .Select(log => log.TipAkcije)
                .Distinct()
                .OrderBy(tip => tip)
                .ToListAsync();

            ViewData["TipoviAkcija"] = new SelectList(tipoviAkcija, tipAkcije);
            ViewData["OdabraniTipAkcije"] = tipAkcije;

            var query = _context.Logovi.AsQueryable();
            if (!string.IsNullOrWhiteSpace(tipAkcije))
            {
                query = query.Where(log => log.TipAkcije == tipAkcije);
            }

            var logovi = await query
                .OrderByDescending(log => log.VrijemeAkcije)
                .ToListAsync();

            var filterDetalji = string.IsNullOrWhiteSpace(tipAkcije)
                ? "bez filtera"
                : $"filter TipAkcije={tipAkcije}";
            await _logService.InfoAsync("LOGOVI_PREGLEDANI", $"Administrator je pregledao listu logova ({filterDetalji}). Broj prikazanih logova: {logovi.Count}.");
            return View(logovi);
        }

        // GET: Log/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _context.Logovi
                .FirstOrDefaultAsync(m => m.Id == id);
            if (log == null)
            {
                return NotFound();
            }

            await _logService.InfoAsync("LOG_DETALJI_PREGLEDANI", $"Administrator je pregledao detalje loga ID {log.Id}.");
            return View(log);
        }

        // GET: Log/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Log/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TipAkcije,VrijemeAkcije,KorisnikId,IpAdresa,Detalji,Nivo")] Log log)
        {
            if (ModelState.IsValid)
            {
                _context.Add(log);
                await _context.SaveChangesAsync();
                await _logService.InfoAsync("LOG_RUCNO_KREIRAN", $"Administrator je rucno kreirao log ID {log.Id}: {log.TipAkcije}.");
                return RedirectToAction(nameof(Index));
            }
            return View(log);
        }

        // GET: Log/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _context.Logovi.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }
            return View(log);
        }

        // POST: Log/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TipAkcije,VrijemeAkcije,KorisnikId,IpAdresa,Detalji,Nivo")] Log log)
        {
            if (id != log.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(log);
                    await _context.SaveChangesAsync();
                    await _logService.WarningAsync("LOG_AZURIRAN", $"Administrator je azurirao log ID {log.Id}: {log.TipAkcije}.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LogExists(log.Id))
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
            return View(log);
        }

        // GET: Log/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _context.Logovi
                .FirstOrDefaultAsync(m => m.Id == id);
            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        // POST: Log/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var log = await _context.Logovi.FindAsync(id);
            if (log != null)
            {
                _context.Logovi.Remove(log);
                await _logService.WarningAsync("LOG_OBRISAN", $"Administrator je obrisao log ID {log.Id}: {log.TipAkcije}.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LogExists(long id)
        {
            return _context.Logovi.Any(e => e.Id == id);
        }
    }
}
