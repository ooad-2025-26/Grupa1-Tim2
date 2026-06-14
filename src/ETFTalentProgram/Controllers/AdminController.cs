using ETFTalentProgram.Constants;
using ETFTalentProgram.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETFTalentProgram.Controllers
{
    [Authorize(Roles = AppRoles.Administrator)]
    public class AdminController : Controller
    {
        private readonly ILogService _logService;

        public AdminController(ILogService logService)
        {
            _logService = logService;
        }

        public async Task<IActionResult> Dashboard()
        {
            await _logService.InfoAsync("ADMIN_DASHBOARD_PREGLEDAN", "Administrator je otvorio administratorski dashboard.");
            return View();
        }
    }
}
