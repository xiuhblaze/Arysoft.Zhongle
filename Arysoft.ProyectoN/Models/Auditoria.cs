using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Auditorias")]
    public class Auditoria
    {
        public Guid AuditoriaID { get; set; }

        public Guid? ResponsableID { get; set; }

        [DisplayFormat(DataFormatString = "{0:000}")]
        public int Folio { get; set; }

        [StringLength(50)]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(1000)]
        public string Descripcion { get; set; }

        [Display(Name = "Realización"), DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime? FechaRealizacion { get; set; }
        
        [Display(Name = "Status")]
        public AuditoriaStatusTipo Status { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // VIRTUAL

        [ForeignKey("ResponsableID")]
        public virtual Persona Responsable { get; set; }

        public virtual ICollection<AuditoriaPersona> PersonasAuditadas { get; set; }

        [ForeignKey("PropietarioID")]
        public virtual ICollection<Nota> Notas { get; set; }

        // NOT MAPPED

        [NotMapped]
        public bool SoloLectura { get; set; }
    } // Auditoria

    [Table("AuditoriaPersonas")]
    public class AuditoriaPersona
    {
        public Guid AuditoriaPersonaID { get; set; }

        public Guid AuditoriaID { get; set; }
        public Guid PersonaID { get; set; }

        [Display(Name = "Auditado"), DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime? FechaAuditoria { get; set; }

        [StringLength(1000)]
        public string Observaciones { get; set; }

        [Display(Name = "Votante seguro")]
        public BoolTipo VotanteSeguro { get; set; }
                
        public AuditoriaResultadoTipo ResultadoTipo { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // VIRTUAL

        public virtual Persona Persona { get; set; }

        public virtual Auditoria Auditoria { get; set; }

        // NOT MAPPED

        [NotMapped]
        public bool SoloLectura { get; set; }

    } // AuditoriaPersonas
}