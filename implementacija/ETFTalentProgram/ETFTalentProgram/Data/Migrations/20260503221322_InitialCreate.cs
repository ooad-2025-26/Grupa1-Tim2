using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETFTalentProgram.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailNotifikacije",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipNotifikacije = table.Column<int>(type: "int", nullable: false),
                    Sadrzaj = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailPrimaoca = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumKreiranja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumSlanja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusIsporuke = table.Column<int>(type: "int", nullable: false),
                    BrojPokusaja = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailNotifikacije", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Korisnici",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lozinka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uloga = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DatumRegistracije = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumZadnjePrijave = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnici", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Logovi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipAkcije = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VrijemeAkcije = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KorisnikId = table.Column<long>(type: "bigint", nullable: true),
                    IpAdresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Detalji = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nivo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logovi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RangParametri",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TezinaProsjecneOcjene = table.Column<double>(type: "float", nullable: false),
                    TezinaECTS = table.Column<double>(type: "float", nullable: false),
                    TezinaBrojVjestina = table.Column<double>(type: "float", nullable: false),
                    TezinaBrojProjekata = table.Column<double>(type: "float", nullable: false),
                    Verzija = table.Column<int>(type: "int", nullable: false),
                    DatumPrimjene = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RangParametri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Administratori",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NivoOvlastenja = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administratori", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Administratori_Korisnici_Id",
                        column: x => x.Id,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Firme",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpisFirme = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lokacija = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KontaktEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndustrijskiSektor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VelicinaFirme = table.Column<int>(type: "int", nullable: false),
                    GodinaOsnivanja = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Firme_Korisnici_Id",
                        column: x => x.Id,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ponude",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TekstPoruke = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumSlanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TipPonude = table.Column<int>(type: "int", nullable: false),
                    DatumOdgovora = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PosiljalacId = table.Column<long>(type: "bigint", nullable: false),
                    PrimalacId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ponude", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ponude_Korisnici_PosiljalacId",
                        column: x => x.PosiljalacId,
                        principalTable: "Korisnici",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ponude_Korisnici_PrimalacId",
                        column: x => x.PrimalacId,
                        principalTable: "Korisnici",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Referenti",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Odsjek = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referenti_Korisnici_Id",
                        column: x => x.Id,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Studenti",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrIndeksa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GodinaStudija = table.Column<int>(type: "int", nullable: false),
                    GodinaUpisa = table.Column<int>(type: "int", nullable: false),
                    ProsjekOcjena = table.Column<double>(type: "float", nullable: false),
                    Verificiran = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Studenti_Korisnici_Id",
                        column: x => x.Id,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FirmaProfili",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KratakOpis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PunOpis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lokacija = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KontaktEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logotip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TehnologijeStack = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumAzuriranja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirmaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmaProfili", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirmaProfili_Firme_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Oglasi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naslov = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tehnologije = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RokPrijave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumObjave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipOglasa = table.Column<int>(type: "int", nullable: false),
                    TipAngazmana = table.Column<int>(type: "int", nullable: false),
                    StatusOglasa = table.Column<int>(type: "int", nullable: false),
                    Lokacija = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinRang = table.Column<double>(type: "float", nullable: false),
                    MinProsjek = table.Column<double>(type: "float", nullable: false),
                    Kompenzacija = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirmaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oglasi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oglasi_Firme_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AkademskiPodaci",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Predmet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SifraPredmeta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ocjena = table.Column<int>(type: "int", nullable: false),
                    ECTS = table.Column<int>(type: "int", nullable: false),
                    GodinaPolaganja = table.Column<int>(type: "int", nullable: false),
                    Semestar = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkademskiPodaci", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkademskiPodaci_Studenti_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfili",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rang = table.Column<double>(type: "float", nullable: false),
                    Biografija = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Vjestine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Projekti = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferiraneLokacije = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferiraneTehnologije = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DostupanOd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumAzuriranja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusVerifikacije = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfili", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfili_Studenti_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Verifikacije",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatumPodnosenja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumVerifikacije = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusVerifikacije = table.Column<int>(type: "int", nullable: false),
                    Komentar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dokumenti = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    ReferentId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verifikacije", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Verifikacije_Referenti_ReferentId",
                        column: x => x.ReferentId,
                        principalTable: "Referenti",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Verifikacije_Studenti_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrijaveOglasa",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatumPrijave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PropratniTekst = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusPrijave = table.Column<int>(type: "int", nullable: false),
                    DatumOdgovora = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    OglasId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrijaveOglasa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrijaveOglasa_Oglasi_OglasId",
                        column: x => x.OglasId,
                        principalTable: "Oglasi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrijaveOglasa_Studenti_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Studenti",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AkademskiPodaci_StudentId",
                table: "AkademskiPodaci",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmaProfili_FirmaId",
                table: "FirmaProfili",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Oglasi_FirmaId",
                table: "Oglasi",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ponude_PosiljalacId",
                table: "Ponude",
                column: "PosiljalacId");

            migrationBuilder.CreateIndex(
                name: "IX_Ponude_PrimalacId",
                table: "Ponude",
                column: "PrimalacId");

            migrationBuilder.CreateIndex(
                name: "IX_PrijaveOglasa_OglasId",
                table: "PrijaveOglasa",
                column: "OglasId");

            migrationBuilder.CreateIndex(
                name: "IX_PrijaveOglasa_StudentId",
                table: "PrijaveOglasa",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfili_StudentId",
                table: "StudentProfili",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Verifikacije_ReferentId",
                table: "Verifikacije",
                column: "ReferentId");

            migrationBuilder.CreateIndex(
                name: "IX_Verifikacije_StudentId",
                table: "Verifikacije",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administratori");

            migrationBuilder.DropTable(
                name: "AkademskiPodaci");

            migrationBuilder.DropTable(
                name: "EmailNotifikacije");

            migrationBuilder.DropTable(
                name: "FirmaProfili");

            migrationBuilder.DropTable(
                name: "Logovi");

            migrationBuilder.DropTable(
                name: "Ponude");

            migrationBuilder.DropTable(
                name: "PrijaveOglasa");

            migrationBuilder.DropTable(
                name: "RangParametri");

            migrationBuilder.DropTable(
                name: "StudentProfili");

            migrationBuilder.DropTable(
                name: "Verifikacije");

            migrationBuilder.DropTable(
                name: "Oglasi");

            migrationBuilder.DropTable(
                name: "Referenti");

            migrationBuilder.DropTable(
                name: "Studenti");

            migrationBuilder.DropTable(
                name: "Firme");

            migrationBuilder.DropTable(
                name: "Korisnici");
        }
    }
}
