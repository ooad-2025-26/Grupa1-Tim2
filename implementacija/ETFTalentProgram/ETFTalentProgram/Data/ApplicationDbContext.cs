using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ETFTalentProgram.Models;

namespace ETFTalentProgram.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Student> Studenti { get; set; }
        public DbSet<Firma> Firme { get; set; }
        public DbSet<Referent> Referenti { get; set; }
        public DbSet<Administrator> Administratori { get; set; }
        public DbSet<StudentProfil> StudentProfili { get; set; }
        public DbSet<FirmaProfil> FirmaProfili { get; set; }
        public DbSet<AkademskiPodatak> AkademskiPodaci { get; set; }
        public DbSet<RangParametri> RangParametri { get; set; }
        public DbSet<Oglas> Oglasi { get; set; }
        public DbSet<PrijavaOglas> PrijaveOglasa { get; set; }
        public DbSet<Ponuda> Ponude { get; set; }
        public DbSet<EmailNotifikacija> EmailNotifikacije { get; set; }
        public DbSet<Log> Logovi { get; set; }
        public DbSet<Verifikacija> Verifikacije { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Korisnik>().ToTable("Korisnici");
            modelBuilder.Entity<Student>().ToTable("Studenti");
            modelBuilder.Entity<Firma>().ToTable("Firme");
            modelBuilder.Entity<Referent>().ToTable("Referenti");
            modelBuilder.Entity<Administrator>().ToTable("Administratori");
            modelBuilder.Entity<StudentProfil>().ToTable("StudentProfili");
            modelBuilder.Entity<FirmaProfil>().ToTable("FirmaProfili");
            modelBuilder.Entity<AkademskiPodatak>().ToTable("AkademskiPodaci");
            modelBuilder.Entity<RangParametri>().ToTable("RangParametri");
            modelBuilder.Entity<Oglas>().ToTable("Oglasi");
            modelBuilder.Entity<PrijavaOglas>().ToTable("PrijaveOglasa");
            modelBuilder.Entity<Ponuda>().ToTable("Ponude");
            modelBuilder.Entity<EmailNotifikacija>().ToTable("EmailNotifikacije");
            modelBuilder.Entity<Log>().ToTable("Logovi");
            modelBuilder.Entity<Verifikacija>().ToTable("Verifikacije");

            modelBuilder.Entity<PrijavaOglas>(entity =>
            {
                entity.HasOne(p => p.Student)
                    .WithMany()
                    .HasForeignKey(p => p.StudentId)
                    .OnDelete(DeleteBehavior.NoAction); 

                entity.HasOne(p => p.Oglas)
                    .WithMany()
                    .HasForeignKey(p => p.OglasId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Ponuda>()
                .HasOne(p => p.Posiljalac)
                .WithMany()
                .HasForeignKey(p => p.PosiljalacId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Ponuda>()
                .HasOne(p => p.Primalac)
                .WithMany()
                .HasForeignKey(p => p.PrimalacId)
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
        }


    }
}