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
    public class RangParametriController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public RangParametriController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: RangParametri
        public async Task<IActionResult> Index()
        {
            return View(await _context.RangParametri.ToListAsync());
        }

        // GET: RangParametri/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rangParametri = await _context.RangParametri
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rangParametri == null)
            {
                return NotFound();
            }

            return View(rangParametri);
        }

        // GET: RangParametri/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RangParametri/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TezinaProsjecneOcjene,TezinaECTS,TezinaBrojVjestina,TezinaBrojProjekata,Verzija,DatumPrimjene")] RangParametri rangParametri)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rangParametri);
                await _context.SaveChangesAsync();
                await _logService.InfoAsync("RANG_PARAMETRI_KREIRANI", $"Kreirani parametri rangiranja ID {rangParametri.Id}, verzija {rangParametri.Verzija}.");
                return RedirectToAction(nameof(Index));
            }
            return View(rangParametri);
        }

        // GET: RangParametri/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rangParametri = await _context.RangParametri.FindAsync(id);
            if (rangParametri == null)
            {
                return NotFound();
            }
            return View(rangParametri);
        }

        // POST: RangParametri/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TezinaProsjecneOcjene,TezinaECTS,TezinaBrojVjestina,TezinaBrojProjekata,Verzija,DatumPrimjene")] RangParametri rangParametri)
        {
            if (id != rangParametri.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rangParametri);
                    await _context.SaveChangesAsync();
                    await _logService.InfoAsync("RANG_PARAMETRI_AZURIRANI", $"Azurirani parametri rangiranja ID {rangParametri.Id}, verzija {rangParametri.Verzija}.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RangParametriExists(rangParametri.Id))
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
            return View(rangParametri);
        }

        // GET: RangParametri/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rangParametri = await _context.RangParametri
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rangParametri == null)
            {
                return NotFound();
            }

            return View(rangParametri);
        }

        // POST: RangParametri/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var rangParametri = await _context.RangParametri.FindAsync(id);
            if (rangParametri != null)
            {
                _context.RangParametri.Remove(rangParametri);
                await _logService.WarningAsync("RANG_PARAMETRI_OBRISANI", $"Obrisani parametri rangiranja ID {rangParametri.Id}, verzija {rangParametri.Verzija}.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RangParametriExists(long id)
        {
            return _context.RangParametri.Any(e => e.Id == id);
        }
    }
}
