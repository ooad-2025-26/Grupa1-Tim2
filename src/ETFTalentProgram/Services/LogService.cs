using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using Microsoft.EntityFrameworkCore;

namespace ETFTalentProgram.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task InfoAsync(string tipAkcije, string detalji)
        {
            return WriteAsync(tipAkcije, detalji, NivoLoga.INFO);
        }

        public Task WarningAsync(string tipAkcije, string detalji)
        {
            return WriteAsync(tipAkcije, detalji, NivoLoga.WARNING);
        }

        public Task ErrorAsync(string tipAkcije, string detalji)
        {
            return WriteAsync(tipAkcije, detalji, NivoLoga.ERROR);
        }

        private async Task WriteAsync(string tipAkcije, string detalji, NivoLoga nivo)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var email = httpContext?.User?.Identity?.Name;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Nepoznata IP adresa";
            var korisnikId = await ResolveKorisnikIdAsync(email);
            var userDetails = string.IsNullOrWhiteSpace(email)
                ? "Anonimni korisnik"
                : $"Korisnik: {email}, IdKorisnika: {(korisnikId?.ToString() ?? "nije pronadjen")}";

            _context.Logovi.Add(new Log
            {
                TipAkcije = tipAkcije,
                VrijemeAkcije = DateTime.UtcNow,
                KorisnikId = korisnikId,
                IpAdresa = ipAddress,
                Detalji = $"{userDetails}. {detalji}",
                Nivo = nivo
            });

            await _context.SaveChangesAsync();
        }

        private async Task<long?> ResolveKorisnikIdAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedEmail = email.Trim();

            var studentId = await _context.Studenti
                .Where(k => k.Email == normalizedEmail)
                .Select(k => (long?)k.Id)
                .FirstOrDefaultAsync();
            if (studentId.HasValue) return studentId;

            var firmaId = await _context.Firme
                .Where(k => k.Email == normalizedEmail)
                .Select(k => (long?)k.Id)
                .FirstOrDefaultAsync();
            if (firmaId.HasValue) return firmaId;

            var referentId = await _context.Referenti
                .Where(k => k.Email == normalizedEmail)
                .Select(k => (long?)k.Id)
                .FirstOrDefaultAsync();
            if (referentId.HasValue) return referentId;

            return await _context.Administratori
                .Where(k => k.Email == normalizedEmail)
                .Select(k => (long?)k.Id)
                .FirstOrDefaultAsync();
        }
    }
}
