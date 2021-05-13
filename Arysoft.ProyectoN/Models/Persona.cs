using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Personas")]
    public class Persona
    {
        public Guid PersonaID { get; set; }

        /// <summary>
        /// Obtiene o establece el sector donde la persona afiliada participa
        /// </summary>
        [Display(Name = "Sector")]
        public Guid? SectorBrigadaID { get; set; }

        /// <summary>
        /// Obtiene o establece la sección donde la persona vota
        /// </summary>
        [Display(Name = "Sección")]
        public Guid? SeccionID { get; set; }

        /// <summary>
        /// Obtiene o establece el afiliado que invito a la persona
        /// </summary>
        [Display(Name = "Movilizador")]
        public Guid? PersonaPromotorID { get; set; }

        public Guid? UbicacionViveID { get; set; }

        public Guid? UbicacionVotaID { get; set; }

        // public Guid? VotoID { get; set; }

        /// <summary>
        /// Identificador de la casilla donde la persona es Representante
        /// </summary>
        public Guid? CasillaID { get; set; }

        [StringLength(50)]
        public string Nombres { get; set; }

        [StringLength(50)]
        [Display(Name = "Apellido paterno")]
        public string ApellidoPaterno { get; set; }

        [StringLength(50)]
        [Display(Name = "Apellido materno")]
        public string ApellidoMaterno { get; set; }

        public SexoTipo Sexo { get; set; }

        // Domicilio

        [Display(Name = "Vota donde mismo")]
        public BoolTipo VotaEnDomicilio { get; set; }

        // Contacto

        [Display(Name = "Teléfono")]
        [StringLength(25)]
        [DataType(DataType.PhoneNumber, ErrorMessage = "El número de teléfono tiene un formato no valido")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "El número de teléfono no es válido.")]
        public string Telefono { get; set; }

        [Display(Name = "Celular")]
        [StringLength(25)]
        [DataType(DataType.PhoneNumber, ErrorMessage = "El número de celular tiene un formato no valido")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "El número de teléfono celular no es válido.")]
        public string Celular { get; set; }

        [Display(Name = "Correo electrónico")]
        [StringLength(256)]
        [DataType(DataType.EmailAddress, ErrorMessage = "El correo electrónico tiene un formato no valido")]
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "El correo electrónico tiene un formato no válido")]
        public string CorreoElectronico { get; set; }

        // Otros

        public AfinidadTipo Afinidad { get; set; }

        [Display(Name = "Votante seguro")]
        public BoolTipo VotanteSeguro { get; set; }

        [Display(Name = "Tiene barda")]
        public BoolTipo TieneBarda { get; set; }

        [Display(Name = "Tiene lona")]
        public BoolTipo TieneLona { get; set; }

        // Si es representante

        public RepresentanteTipo RepresentanteTipo { get; set; }

        /// <summary>
        /// Obtiene o establece si el representante, asistió el día D como representante de casilla
        /// </summary>
        public BoolTipo RepresentanteAsistencia { get; set; }

        /// <summary>
        /// Obtiene o establece si el representante, asistió a tomar capacitación o no
        /// </summary>
        [Display(Name = "Capacitación")]
        public BoolTipo RepresentanteCapacitacion { get; set; }

        // General 

        /// <summary>
        /// Obtiene o establece si los datos de la persona ya fueron verificados
        /// </summary>
        public BoolTipo Verificada { get; set; }

        // Nuevos campos

        [Display(Name = "Fecha de nacimiento")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Estado civil")]
        public EstadoCivilTipo EstadoCivil { get; set; }

        public BoolTipo Whatsapp { get; set; }

        [Display(Name = "Ocupación")]
        [StringLength(100)]
        public string Ocupacion { get; set; }

        // End Nuevos campos

        [Display(Name = "Alta")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime FechaAlta { get; set; }

        [Display(Name = "Status")]
        public StatusTipo Status { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // SOLO LECTURA

        [Display(Name = "Nombre completo")]
        public string NombreCompleto {
            get {
                string nombre = string.Empty;

                nombre += !string.IsNullOrEmpty(ApellidoPaterno) ? ApellidoPaterno : string.Empty;
                nombre += !string.IsNullOrEmpty(ApellidoMaterno) ? " " + ApellidoMaterno : string.Empty;
                nombre += !string.IsNullOrEmpty(nombre) ? " " + Nombres : Nombres;

                return nombre.Trim();
            }
        }

        // RELACIONES
                
        public Voto Voto {get; set;}

        [ForeignKey("PersonaPromotorID")]
        public Persona Promotor { get; set; }
        
        [ForeignKey("CasillaID")]
        public Casilla Casilla { get; set; }

        [ForeignKey("PropietarioID")]
        public ICollection<Nota> Notas { get; set; }

        // Domicilios

        [ForeignKey("UbicacionViveID")]
        public Ubicacion UbicacionVive { get; set; }

        [ForeignKey("UbicacionVotaID")]
        public Ubicacion UbicacionVota { get; set; }
                
        [ForeignKey("SectorBrigadaID")]
        public Sector Sector { get; set; }
        
        public Seccion Seccion { get; set; }

        [ForeignKey("PersonaPromotorID")]
        public ICollection<Persona> PersonasAfines { get; set; }

        public ICollection<AuditoriaPersona> AuditoriasRealizadas { get; set; }

        public ICollection<Auditoria> ResponsableAuditorias { get; set; }

        // NOT MAPPED

        [NotMapped]
        public bool SoloLectura { get; set; }

        [NotMapped]
        public bool EsResponsableSector { get; set; }

        [NotMapped]
        public string NmOrigen { get; set; }
    } // Persona

    [NotMapped]
    public class PersonaEditViewModel : Persona
    {

        // Domicilio donde vive

        public Guid CalleID { get; set; }

        public Guid ColoniaID { get; set; }

        [Display(Name = "No. Ext.")]
        public int NumExterior { get; set; }

        [Display(Name = "Letra")]
        public string Letra { get; set; }

        [Display(Name = "No. Int.")]
        public string NumInterior { get; set; }

        [Display(Name = "Ubicación alterna")]
        public string Descripcion { get; set; }

        public float Latitud { get; set; }

        public float Longitud { get; set; }

        // Domicilio donde vota

        public Guid VotaCalleID { get; set; }

        public Guid VotaColoniaID { get; set; }

        [Display(Name = "No. Ext.")]
        public int VotaNumExterior { get; set; }

        [Display(Name = "Letra")]
        public string VotaLetra { get; set; }

        [Display(Name = "No. Int.")]
        public string VotaNumInterior { get; set; }

        [Display(Name = "Ubicación alterna")]
        public string VotaDescripcion { get; set; }

        public float VotaLatitud { get; set; }

        public float VotaLongitud { get; set; }
                
    } // PersonaEditViewModel

    public class PersonaSearchModel
    {
        public string Buscar { get; set; }
        public string Orden { get; set; }
        public string Afinidad { get; set; }
        public string SectorID { get; set; }
        public string SeccionID { get; set; }
        public string PromotorID { get; set; }
        public string CalleID { get; set; }
        public string ColoniaID { get; set; }
        public string BardaLona { get; set; }
        public string SectorTipo { get; set; }
        public string VotanteSeguro { get; set; }
        public string Status { get; set; }
        public string Verificada { get; set; }
        public string FiltroEspecifico { get; set; }
        public string YaVoto { get; set; }
        public string BusquedaAvanzada { get; set; }
        public string LlamadaOrigen { get; set; }
        public int? Pagina { get; set; }
    } // PersonaSearchModel
}