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

        [StringLength(150)]
        public string Candidato { get; set; }

        public StatusTipo Status { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // RELACIONES

        public ICollection<ResultadoCasilla> Casillas { get; set; }

    }
}