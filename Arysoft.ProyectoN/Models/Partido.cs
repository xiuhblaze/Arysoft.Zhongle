using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Partidos")]
    public class Partido
    {
        public Guid PartidoID { get; set; }

        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(10)]
        public string Siglas { get; set; }

        [StringLength(255)]
        [Display(Name = "Logotipo")]
        public string ArchivoLogotipo { get; set; }

        public StatusTipo Status { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // RELACIONES

        public ICollection<Candidato> Candidatos { get; set; }

        // NOT MAPPED

        /// <summary>
        /// Obtiene o establece un valor que se pasa a las vistas y determina si el origen del modelo
        /// es desde un metodo de consulta o para dar de baja el registro
        /// </summary>
        [NotMapped]
        public string Origen { get; set; }

        /// <summary>
        /// Obtiene o establece un valor qe se pasa a las vistas para determinar si la información
        /// se va a mostrar como solo lectura o para edición
        /// </summary>
        [NotMapped]
        public bool SoloLectura { get; set; }
    }
}