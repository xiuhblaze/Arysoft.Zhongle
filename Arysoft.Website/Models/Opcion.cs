using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.Website.Models
{
    [Table("Opciones")]
    public class Opcion
    {
        public int OpcionID { get; set; }
        
        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(1000)]
        public string Valor { get; set; }

        public static void GenerarOpcionesDefault()
        {
            // HACK: Poner todas las constantes para la aplicación como los números de renglones por página y eso
        }
    }
}