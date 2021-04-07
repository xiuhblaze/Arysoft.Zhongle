using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Sectores")]
    public class Sector
    {
        public Guid SectorID { get; set; }

        [Display(Name = "Responsable")]
        [ForeignKey("Responsable")]
        public Guid? ResponsableID { get; set; }

        [StringLength(50)]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(500)]
        public string Descripcion { get; set; }

        [Display(Name = "Votos meta")]
        public int VotosMeta { get; set; }

        [Display(Name = "Status")]
        public StatusTipo Status { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // RELACIONES

        /// <summary>
        /// Persona responsable del sector
        /// </summary>
        [ForeignKey("ResponsableID")]
        public Persona Responsable { get; set; }

        /// <summary>
        /// Obtiene o establece el listado de Personas que apoyan en el sector
        /// </summary>
        [ForeignKey("SectorBrigadaID")]
        public ICollection<Persona> Personas { get; set; }

        /// <summary>
        /// Obtiene o establece la lista de Secciones pertenecientes al sector
        /// </summary>
        public ICollection<Seccion> Secciones { get; set; }

        // NOT MAPED

        /// <summary>
        /// Obtiene o establece un valor que se pasa a las vistas y determina si el origen del modelo
        /// es desde un metodo de consulta o para dar de baja el registro
        /// </summary>
        [NotMapped]
        public string NmOrigen { get; set; }

        /// <summary>
        /// Obtiene o establece un valor qe se pasa a las vistas para determinar si la información
        /// se va a mostrar como solo lectura o para edición
        /// </summary>
        [NotMapped]
        public bool SoloLectura { get; set; }
    }
}