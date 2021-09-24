using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.Website.Models
{
    [Table("Paginas")]
    public class Pagina
    {
        public Guid PaginaID { get; set; }

        [Display(Name = "Página padre"), DisplayFormat(NullDisplayText = "(es página padre)")]
        public Guid? PaginaPadreID { get; set; }

        [Display(Name = "Título"), StringLength(150)]
        public string Titulo { get; set; }

        [Display(Name = "Indice", Description = "Indice para el orden con la etiqueta de menu")]
        [Range(0, 100, ErrorMessage = "El valor no puede ser menor que cero ni mayor a 100")]
        public int IndiceMenu { get; set; }

        [Display(Name = "Etiqueta para menú"), StringLength(30)]
        public string EtiquetaMenu { get; set; }

        [StringLength(1000)]
        public string Resumen { get; set; }
                
        public string HTMLContent { get; set; }

        [Display(Name = "Url Amigable", Description = "Permite crear una dirección Url en base al Título, de forma amigable para el usuario.")]
        [StringLength(255)]
        public string FriendlyUrl { get; set; }

        [Display(Name = "Dirección URL", Description = "Direccion url a la cual el enlace va a saltar."), StringLength(255)]
        public string TargetUrl { get; set; }

        public PaginaTarget Target { get; set; }

        [Display(Name = "Tiene galeria")]
        public BoolTipo TieneGaleria { get; set; }

        [Display(Name = "Visitas")]
        public int ContadorVisitas { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Conteo desde")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaContador { get; set; }

        public IdiomaTipo Idioma { get; set; }

        [Display(Name = "Es página principal")]
        public BoolTipo EsPrincipal { get; set; }

        [Display(Name = "Script encabezado")]
        public string HTMLHeadScript { get; set; }

        [Display(Name = "Script final")]
        public string HTMLFooterScript { get; set; }

        [Display(Name = "Me gusta")]
        public int MeGusta { get; set; }

        public PaginaStatus Status { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaCreacion { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Actualización")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaActualizacion { get; set; }

        [StringLength(150)]
        [Display(Name = "Actualizado por")]
        public string UsuarioActualizacion { get; set; }

        // RELACIONES

        [ForeignKey("PaginaPadreID")]
        public Pagina PaginaPadre { get; set; }

        [ForeignKey("PaginaPadreID")]
        public ICollection<Pagina> PaginasHijo { get; set; }

        [ForeignKey("PropietarioID")]
        public ICollection<Archivo> Archivos { get; set; }

        [ForeignKey("PropietarioID")]
        public ICollection<Nota> Notas { get; set; }
    } // Pagina

    public enum PaginaStatus
    {
        Ninguno,    // Página temporal, aun no ha sido guardada
        Nueva,      // Página nueva, aun no es visible al público
        Publicada,  // La página ha sido publicada y es visible al público
        Oculta,     // Oculta al público
        Eliminada   // Página eliminada, solo un administrador puede habilitarla nuevamente
    }

    public enum PaginaTarget
    {
        [Display(Name = "(seleccionar target)")]
        Ninguno,
        Blank,
        Parent,
        Self,
        Top
    }
}