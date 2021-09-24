using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.Website.Models
{
    [Table("Notas")]
    public class Nota
    {
        public Guid NotaID { get; set; }
        public Guid PropietarioID { get; set; }

        [StringLength(1000)]
        public string Texto { get; set; }

        [StringLength(150)]
        public string Autor { get; set; }

        public StatusTipo Status { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaCreacion { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Actualización")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaActualizacion { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(150)]
        public string UsuarioActualizacion { get; set; }
    } // Nota
}