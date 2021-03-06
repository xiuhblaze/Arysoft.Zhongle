using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Poblaciones")]
    public class Poblacion
    {
        public Guid PoblacionID { get; set; }

        [Display(Name = "Municipio")]
        public Guid MunicipioID { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        public StatusTipo Status { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // SOLO LECTURA

        public string Municipio
        {
            get
            {
                return "Zapotlán El Grande";
            }
        }

    } // Poblacion
}