using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.Website.Models
{
    public class ContactSendViewModel
    {
        [StringLength(50)]
        public string Nombres { get; set; }

        [Display(Name = "Apellido")]
        [StringLength(50)]
        public string PrimerApellido { get; set; }

        [StringLength(100)]
        public string Empresa { get; set; }

        [Display(Name = "Correo electrónico")]
        [DataType(DataType.EmailAddress)]
        [StringLength(255)]
        public string EMail { get; set; }

        [StringLength(50)]
        public string Puesto { get; set; }

        [Display(Name = "Teléfono")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(15)]
        public string Telefono { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(250)]
        public string Domicilio { get; set; }

        [DataType(DataType.MultilineText)]
        public string Mensaje { get; set; }
    } // ContactSendViewModel
}