using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("PersonasIne")]
    public class PersonaIne
    {
        public Guid PersonaIneID { get; set; }

        [StringLength(50)]
        public string Nombres { get; set; }

        [StringLength(50)]
        public string PrimerApellido { get; set; }

        [StringLength(50)]
        public string SegundoApellido { get; set; }

        public int? SeccionNumero { get; set; }

        [StringLength(50)]
        public string Casilla { get; set; }

        public int? NumeroINE { get; set; }

        public BoolTipo Registrado { get; set; }

        [StringLength(1000)]
        public string LogError { get; set; }


    } // PersonaIne
}