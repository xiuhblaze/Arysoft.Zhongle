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

        public Guid PartidoID { get; set; }

        public Guid CasillaID { get; set; }

        public int Total { get; set; }

        // RELACION

        public Partido Partido { get; set; }

        public Casilla Casilla { get; set; }
    }
}