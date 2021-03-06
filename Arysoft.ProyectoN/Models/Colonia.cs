using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Colonias")]
    public class Colonia
    {
        public Guid ColoniaID { get; set; }

        [Display(Name ="Población")]
        public Guid PoblacionID { get; set; }

        [Display(Name = "Sección")]
        public Guid? SeccionID { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(5)]
        [Display(Name = "Código postal")]
        public string CodigoPostal { get; set; }

        public StatusTipo Status { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // RELACIONES
                
        public Poblacion Poblacion { get; set; }

        public Seccion Seccion { get; set; }

        public ICollection<Calle> Calles { get; set; }

        public ICollection<Ubicacion> Ubicaciones { get; set; }

        // NOT MAPPED

        /// <summary>
        /// Obtiene o establece un valor que se pasa a las vistas y determina si el origen del modelo
        /// es desde un metodo de consulta o para dar de baja el registro
        /// </summary>
        [NotMapped]
        public string NpOrigen { get; set; }

        /// <summary>
        /// Obtiene o establece un valor qe se pasa a las vistas para determinar si la información
        /// se va a mostrar como solo lectura o para edición
        /// </summary>
        [NotMapped]
        public bool NpSoloLectura { get; set; }
    } // Colonia
}