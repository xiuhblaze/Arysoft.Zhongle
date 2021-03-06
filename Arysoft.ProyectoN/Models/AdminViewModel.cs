using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Arysoft.ProyectoN.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Rol")]
        public string Name { get; set; }

        [Display(Name = "Descripción")]
        public string Description { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Correo electrónico")]
        [EmailAddress]
        public string Email { get; set; }

        // Add the Address Info:
        [StringLength(50)]
        public string Nombres { get; set; }

        [StringLength(50)]
        [Display(Name = "Apellido paterno")]
        public string ApellidoPaterno { get; set; }

        [StringLength(50)]
        [Display(Name = "Apellido materno")]
        public string ApellidoMaterno { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^([A-Z][A,E,I,O,U,X][A-Z]{2})(\d{2})((01|03|05|07|08|10|12)(0[1-9]|[12]\d|3[01])|02(0[1-9]|[12]\d)|(04|06|09|11)(0[1-9]|[12]\d|30))([M,H])(AS|BC|BS|CC|CS|CH|CL|CM|DF|DG|GT|GR|HG|JC|MC|MN|MS|NT|NL|OC|PL|QT|QR|SP|SL|SR|TC|TS|TL|VZ|YN|ZS|NE)([B,C,D,F,G,H,J,K,L,M,N,Ñ,P,Q,R,S,T,V,W,X,Y,Z]{3})([0-9,A-Z][0-9])$", ErrorMessage = "La CURP tiene un formato no valido.")]
        public string CURP { get; set; }

        // Colecciones

        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
}