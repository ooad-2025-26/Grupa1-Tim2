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
    public class ReferentKorisniciController : Controller
    {
        private static readonly string[] ManageableRoles =
        [
            AppRoles.Student,
            AppRoles.Firma,
            AppRoles.Referent,
            AppRoles.Administrator
        ];

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogService _logService;

        public ReferentKorisniciController(
      ApplicationDbContext context,
      UserManager<ApplicationUser> userManager,
      ILogService logService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users
                .OrderBy(user => user.Email)
                .ToList();

            var model = new List<UserManagementIndexViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserManagementIndexViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Roles = string.Join(", ", roles),
                    EmailConfirmed = user.EmailConfirmed,
                    DatumRegistracije = user.DatumRegistracije,
                    DatumZadnjePrijave = user.DatumZadnjePrijave
                });
            }

            return View(model);
        }

        public IActionResult Create()
        {
            PopulateRoles();
            return View(new UserManagementCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserManagementCreateViewModel model)
        {
            if (!ManageableRoles.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Odabrana uloga nije validna.");
            }

            if (!ModelState.IsValid)
            {
                PopulateRoles(model.Role);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                DatumRegistracije = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, model.Role);
            }

            if (!result.Succeeded)
            {
                AddErrors(result);
                PopulateRoles(model.Role);
                return View(model);
            }

            await _logService.InfoAsync("KORISNIK_KREIRAN", $"Referent je kreirao korisnika {model.Email} sa ulogom {model.Role}.");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserManagementEditViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? AppRoles.Student,
                EmailConfirmed = user.EmailConfirmed
            };

            PopulateRoles(model.Role);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserManagementEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ManageableRoles.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Odabrana uloga nije validna.");
            }

            if (!ModelState.IsValid)
            {
                PopulateRoles(model.Role);
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = model.Email;
            user.UserName = model.Email;
            user.EmailConfirmed = model.EmailConfirmed;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var existingRoles = await _userManager.GetRolesAsync(user);
                if (existingRoles.Any())
                {
                    result = await _userManager.RemoveFromRolesAsync(user, existingRoles);
                }
            }

            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, model.Role);
            }

            if (result.Succeeded && !string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            }

            if (!result.Succeeded)
            {
                AddErrors(result);
                PopulateRoles(model.Role);
                return View(model);
            }

            await _logService.InfoAsync("KORISNIK_AZURIRAN", $"Referent je azurirao korisnika {model.Email} sa ulogom {model.Role}.");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserManagementIndexViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = string.Join(", ", roles),
                EmailConfirmed = user.EmailConfirmed,
                DatumRegistracije = user.DatumRegistracije,
                DatumZadnjePrijave = user.DatumZadnjePrijave
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var email = user.Email ?? user.UserName ?? user.Id;
            await MarkDomainUserStatusAsync(email, Status.OBRISAN);
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(new UserManagementIndexViewModel
                {
                    Id = user.Id,
                    Email = email,
                    EmailConfirmed = user.EmailConfirmed,
                    DatumRegistracije = user.DatumRegistracije,
                    DatumZadnjePrijave = user.DatumZadnjePrijave
                });
            }
            await _logService.WarningAsync("KORISNIK_OBRISAN", $"Referent je obrisao korisnika {email}.");
            return RedirectToAction(nameof(Index));
        }

        private async Task MarkDomainUserStatusAsync(string email, Status status)
        {
            var studenti = await _context.Studenti
                .Where(s => s.Email == email)
                .ToListAsync();

            foreach (var student in studenti)
            {
                student.Status = status;
                student.Verificiran = false;
            }

            var firme = await _context.Firme
                .Where(f => f.Email == email)
                .ToListAsync();

            foreach (var firma in firme)
            {
                firma.Status = status;
            }

            var referenti = await _context.Referenti
                .Where(r => r.Email == email)
                .ToListAsync();

            foreach (var referent in referenti)
            {
                referent.Status = status;
            }

            var administratori = await _context.Administratori
                .Where(a => a.Email == email)
                .ToListAsync();

            foreach (var administrator in administratori)
            {
                administrator.Status = status;
            }

            await _context.SaveChangesAsync();
        }


        private void PopulateRoles(string? selectedRole = null)
        {
            ViewData["Roles"] = new SelectList(ManageableRoles, selectedRole);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
