using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("RepresentantesTemp")]
    public class RepresentanteTemp
    {
        public Guid ID { get; set; }

        [StringLength(10)]
        public string Seccion { get; set; }

        [StringLength(10)]
        public string Casilla { get; set; }

        [StringLength(50)]
        public string Nombres { get; set; }

        [StringLength(50)]
        public string ApellidoPaterno { get; set; }

        [StringLength(50)]
        public string ApellidoMaterno { get; set; }

        [StringLength(15)]
        public string Telefono { get; set; }

        [StringLength(100)]
        public string Calle { get; set; }

        [StringLength(10)]
        public string Numero { get; set; }

        [StringLength(10)]
        public string Interior { get; set; }

        [StringLength(10)]
        public string CP { get; set; }

        [StringLength(100)]
        public string Colonia { get; set; }

        [StringLength(50)]
        public string ClaveElector { get; set; }

        [StringLength(10)]
        public string SeccionVota { get; set; }

        [StringLength(10)]
        public string Puesto { get; set; }
    }
}