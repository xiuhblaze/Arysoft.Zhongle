using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("ResultadosCasillas")]
    public class ResultadoCasilla
    {
        public Guid ResultadoCasillaID { get; set; }

        public Guid CasillaID { get; set; }

        public Guid CandidatoID { get; set; }

        public int Votos { get; set; }

        public BoolTipo Nulos { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // RELACION

        public Casilla Casilla { get; set; }

        public Candidato Candidato { get; set; }
    }
}