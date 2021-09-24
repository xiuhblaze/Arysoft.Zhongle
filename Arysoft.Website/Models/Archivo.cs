using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.Website.Models
{
    [Table("Archivos")]
    public class Archivo
    {
        public Guid ArchivoID { get; set; }        
        public Guid PropietarioID { get; set; }

        public int Indice { get; set; }

        [StringLength(255)]
        public string Nombre { get; set; }

        [Display(Name = "Descripción"), StringLength(1000)]
        public string Descripcion { get; set; }

        [StringLength(200)]
        public string Tags { get; set; }

        public BoolTipo EnGaleria { get; set; }

        [Display(Name = "Estatus")]
        public StatusTipo Status { get; set; }

        [DataType(DataType.DateTime), Display(Name = "Alta"), DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime FechaCreacion { get; set; }

        [DataType(DataType.DateTime), Display(Name = "Actualización"), DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime FechaActualizacion { get; set; }

        [StringLength(256)]
        public string UsuarioActualizacion { get; set; }
    }
}