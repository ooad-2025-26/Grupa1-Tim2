using ETFTalentProgram.Constants;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using ETFTalentProgram.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ETFTalentProgram.Controllers
{
    [Authorize(Roles = $"{AppRoles.Firma},{AppRoles.Referent}")]
    public class FirmaProfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public FirmaProfilController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: FirmaProfil
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.FirmaProfili.Include(f => f.Firma);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FirmaProfil/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firmaProfil = await _context.FirmaProfili
                .Include(f => f.Firma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (firmaProfil == null)
            {
                return NotFound();
            }

            return View(firmaProfil);
        }

        // GET: FirmaProfil/Create
        [Authorize(Roles = AppRoles.Firma)]
        public IActionResult Create()
        {
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id");
            return View();
        }

        // POST: FirmaProfil/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,KratakOpis,PunOpis,Lokacija,Website,KontaktEmail,Logotip,TehnologijeStack,DatumAzuriranja,FirmaId")] FirmaProfil firmaProfil)
        {
            if (ModelState.IsValid)
            {
                _context.Add(firmaProfil);
                await _context.SaveChangesAsync();
                await _logService.InfoAsync("FIRMA_PROFIL_KREIRAN", $"Kreiran profil firme ID {firmaProfil.FirmaId}.");
                return RedirectToAction(nameof(Index));
            }
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id", firmaProfil.FirmaId);
            return View(firmaProfil);
        }

        // GET: FirmaProfil/Edit - Gets current user's profile
        public async Task<IActionResult> Edit(long? id)
        {
            FirmaProfil? firmaProfil;

            if (User.IsInRole(AppRoles.Referent))
            {
                if (id == null)
                {
                    return NotFound();
                }

                firmaProfil = await _context.FirmaProfili
                    .Include(f => f.Firma)
                    .FirstOrDefaultAsync(f => f.Id == id);
            }
            else
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return NotFound();
                }

                var firma = await _context.Firme.FirstOrDefaultAsync(f => f.Email == User.Identity.Name);
                if (firma == null)
                {
                    return NotFound();
                }

                firmaProfil = await _context.FirmaProfili.FirstOrDefaultAsync(f => f.FirmaId == firma.Id);
            }

            if (firmaProfil == null)
            {
                return NotFound();
            }

            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Email", firmaProfil.FirmaId);
            return View(firmaProfil);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,KratakOpis,PunOpis,Lokacija,Website,KontaktEmail,Logotip,TehnologijeStack,DatumAzuriranja,FirmaId,StatusVerifikacije")] FirmaProfil firmaProfil)
        {
            // FIX 1: Ignorišemo validaciju za Logotip i navigacijsku osobinu Firma
            ModelState.Remove("Logotip");
            ModelState.Remove("Firma");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProfil = await _context.FirmaProfili.FindAsync(firmaProfil.Id);
                    if (existingProfil == null)
                    {
                        return NotFound();
                    }

                    if (!User.IsInRole(AppRoles.Referent) && existingProfil.FirmaId != firmaProfil.FirmaId)
                    {
                        return Forbid();
                    }

                    existingProfil.KratakOpis = firmaProfil.KratakOpis;
                    existingProfil.PunOpis = firmaProfil.PunOpis;
                    existingProfil.Lokacija = firmaProfil.Lokacija;
                    existingProfil.Website = firmaProfil.Website;
                    existingProfil.KontaktEmail = firmaProfil.KontaktEmail;
                    existingProfil.Logotip = firmaProfil.Logotip;
                    existingProfil.TehnologijeStack = firmaProfil.TehnologijeStack;
                    existingProfil.DatumAzuriranja = DateTime.UtcNow;
                    existingProfil.FirmaId = firmaProfil.FirmaId;

                    if (User.IsInRole(AppRoles.Referent))
                    {
                        existingProfil.StatusVerifikacije = firmaProfil.StatusVerifikacije;
                    }
                    else
                    {
                        // FIX 2: Ako izmjenu vrši firma, status MORA ići na čekanje
                        existingProfil.StatusVerifikacije = StatusVerifikacije.NA_CEKANJU;
                    }

                    await _context.SaveChangesAsync();
                    await _logService.InfoAsync("FIRMA_PROFIL_AZURIRAN", $"Azuriran profil firme ID {existingProfil.FirmaId} sa statusom {existingProfil.StatusVerifikacije}.");
                    TempData["StatusMessage"] = "Profil je uspješno ažuriran.";

                    if (User.IsInRole(AppRoles.Referent))
                    {
                        return RedirectToAction("Index", "Verifikacija");
                    }

                    // FIX 3: Redirekcija na Dashboard firme umjesto nazad na Edit formu
                    return RedirectToAction("Index", "Firma");
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
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Email", firmaProfil.FirmaId);
            return View(firmaProfil);
        }

        // GET: FirmaProfil/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firmaProfil = await _context.FirmaProfili
                .Include(f => f.Firma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (firmaProfil == null)
            {
                return NotFound();
            }

            return View(firmaProfil);
        }

        // POST: FirmaProfil/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var firmaProfil = await _context.FirmaProfili.FindAsync(id);
            if (firmaProfil != null)
            {
                _context.FirmaProfili.Remove(firmaProfil);
                await _logService.WarningAsync("FIRMA_PROFIL_OBRISAN", $"Obrisan profil firme ID {firmaProfil.FirmaId}.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FirmaProfilExists(long id)
        {
            return _context.FirmaProfili.Any(e => e.Id == id);
        }
    }
}
