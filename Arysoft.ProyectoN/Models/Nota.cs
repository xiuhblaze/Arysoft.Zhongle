using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Notas")]
    public class Nota
    {
        public Guid NotaID { get; set; }
        public Guid PropietarioID { get; set; }

        [StringLength(1000), Required(ErrorMessage = "Es necesario el texto de la nota")]
        public string Texto { get; set; }

        [StringLength(150), Required(ErrorMessage = "Es necesario indicar el autor")]
        public string Autor { get; set; }
        
        [Display(Name = "Publicación")]
        [DataType(DataType.DateTime)]
        public DateTime FechaPublicacion { get; set; }
        
        public PropietarioTipo PropietarioTipo { get; set; }

        // SOLO LECTURA

        public string VistaFormatoHTML
        {
            get
            {
                return "<strong>[" + FechaPublicacion.ToShortDateString() + "]</strong> - " + Autor + ":<br />" + Texto.Replace(Environment.NewLine, "<br />");
            }
        }

        public string VistaAttrbTitle
        {
            get
            {
                return "[" + FechaPublicacion.ToShortDateString() + "] "
                    + Texto + "\n"
                    + Autor.ToLower(); // System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Autor.ToLower()); // https://stackoverflow.com/questions/748411/is-there-a-capitalizefirstletter-method                
            }
        }
    } // Nota
}