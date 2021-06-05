using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    /// <summary>
    /// Registro de ubicaciónes, validar contra ID: prefijo, calle y colonia, así como
    /// número exterior, letra y número interior para evitar duplicados.
    /// </summary>
    [Table("Ubicaciones")]
    public class Ubicacion
    {
        public Guid UbicacionID { get; set; }
        
        public Guid? CalleID { get; set; }

        public Guid? ColoniaID { get; set; }

        [Display(Name = "No. Ext.")]
        public int NumExterior { get; set; }

        [Display(Name = "Letra")]
        public string Letra { get; set; }

        [Display(Name = "No. Int.")]
        public string NumInterior { get; set; }

        [Display(Name = "Ubicación alterna")]
        public string Descripcion { get; set; }

        public float Latitud { get; set; }

        public float Longitud { get; set; }

        // RELACIONES
        
        public Calle Calle { get; set; }

        public Colonia Colonia { get; set; }

    } // Ubicación
}