using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Votos")]
    public class Voto
    {
        public Guid VotoID { get; set; }

        [Display(Name = "Casilla")]
        public Guid CasillaID { get; set; }

        [Display(Name = "Número INE")]
        public int NumeroINE { get; set; }

        [Display(Name = "Voto")]
        public BoolTipo YaVoto { get; set; }

        [Display(Name = "Hora")]
        public VotoHoraTipo VotoHora {get; set;}

        // RELACIONES

        public Persona Persona { get; set; }

        public Casilla Casilla { get; set; }
    }
}