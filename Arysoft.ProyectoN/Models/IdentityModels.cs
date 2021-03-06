using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Conventions;

//using Arysoft.ProyectoN.Models;

namespace Arysoft.ProyectoN.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        string curp;

        [StringLength(50)]
        public string Nombres { get; set; }

        [StringLength(50)]
        [Display(Name = "Apellido paterno")]
        public string ApellidoPaterno { get; set; }

        [StringLength(50)]
        [Display(Name = "Apellido materno")]
        public string ApellidoMaterno { get; set; }

        [StringLength(50)]        
        public string CURP {
            get { return curp; }
            set { curp = value != null ? value.ToUpperInvariant() : value; }
        }

        [Display(Name = "Nombre")]
        public string NombreCompleto
        {
            get
            {
                string nombre = Nombres;
                nombre += string.IsNullOrWhiteSpace(ApellidoPaterno) ? "" : " " + ApellidoPaterno;
                nombre += string.IsNullOrWhiteSpace(ApellidoMaterno) ? "" : " " + ApellidoMaterno;
                return nombre;
            }
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    //[NotMapped]
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name) : base(name) { }

        [Display(Name = "Descripción")]
        public string Description { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Calle> Calles { get; set; }
        public DbSet<Casilla> Casillas { get; set; }
        public DbSet<Colonia> Colonias { get; set; }
        public DbSet<Nota> Notas { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Poblacion> Poblaciones { get; set; }
        public DbSet<Seccion> Secciones { get; set; }
        public DbSet<Sector> Sectores { get; set; }
        public DbSet<Ubicacion> Ubicaciones { get; set; }
        public DbSet<AuditoriaPersona> AuditoriaPersonas { get; set; }
        public DbSet<Voto> Votos { get; set; }
        public DbSet<ResultadoCasilla> ResultadoCasillas { get; set; }
        public DbSet<Partido> Partidos { get; set; }

        public DbSet<PersonaTmp> PersonasTmp { get; set; }
        public DbSet<PersonaIne> PersonasIne { get; set; }

        //public DbSet<Representante> Representantes { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        static ApplicationDbContext()
        {
            // Set the database intializer which is run once during application start
            // This seeds the database with admin user credentials and admin role

            //TODO: Esto lo quité para que no este generando la base de datos siempre - https://stackoverflow.com/questions/14064434/ef5-getting-this-error-message-model-compatibility-cannot-be-checked-because-th
            //Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Relación muchos a muchos sin modelo especificado Calles <-> Colonias
            modelBuilder.Entity<Calle>()
                .HasMany(c => c.Colonias).WithMany(cl => cl.Calles)
                .Map(t => t.MapLeftKey("CalleID")
                    .MapRightKey("ColoniaID")
                    .ToTable("CalleColonia"));

            //modelBuilder.Entity<Persona>()
            //    .HasOptional(p => p.Voto)
            //    .WithOptionalPrincipal(p => p.Persona);
            
            modelBuilder.Entity<Casilla>()
                .HasMany(c => c.Representantes)
                .WithOptional(p => p.Casilla)
                .Map(m => m.MapKey("CasillaID"));

            modelBuilder.Entity<Casilla>()
                .HasMany(c => c.Votantes)
                .WithRequired(v => v.Casilla)
                .Map(m => m.MapKey("CasillaID"));

            // Persona - Voto: Zero to one - Zero to one

            modelBuilder.Entity<Persona>()
                .HasOptional(p => p.Voto)
                .WithOptionalPrincipal()
                    .Map(p => p.MapKey("PersonaID"));

            modelBuilder.Entity<Voto>()
                .HasOptional(v => v.Persona)
                .WithOptionalPrincipal()
                    .Map(v => v.MapKey("VotoID"));

            //modelBuilder.Entity<Persona>()
            //    .HasOptional(p => p.Voto)
            //    .WithOptionalPrincipal(v => v.Persona);
                        

        } // OnModelCreating        
    }
}