using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.Website.Models
{
    [Table("Noticias")]
    public class Noticia
    {
        public Guid NoticiaID { get; set; }

        [Display(Name = "Título"), StringLength(200)]
        //[Required(ErrorMessage = "Es necesario el título de la noticia")]
        public string Titulo { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Resumen { get; set; }

        //[Required(ErrorMessage = "Es necesario el contenido de la noticia")]
        [DataType(DataType.MultilineText)]
        public string HTMLContent { get; set; }

        [Display(Name = "Url Amigable", Description = "Permite crear una dirección Url en base al Título, de forma amigable para el usuario.")]
        [StringLength(255)]
        public string FriendlyUrl { get; set; }

        [StringLength(150)]
        public string Autor { get; set; }

        [Display(Name = "Tiene galeria")]
        public BoolTipo TieneGaleria { get; set; }

        public int ContadorVisitas { get; set; }

        public IdiomaTipo Idioma { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Publicación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaPublicacion { get; set; }

        [StringLength(255)]
        [Display(Name = "Imagen principal")]
        public string ImagenArchivo { get; set; }

        public int MeGusta { get; set; }

        public NoticiaStatus Status { get; set; }

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

        // RELACIONES

        [ForeignKey("PropietarioID")]
        public ICollection<Archivo> Archivos { get; set; }

        [ForeignKey("PropietarioID")]
        public ICollection<Nota> Notas { get; set; }
    }

    public enum NoticiaStatus
    {
        [Display(Name = "(estatus)")]
        Ninguno,    // Noticia temporal, aun no ha sido guardada
        Nueva,      // Noticia nueva, aun no es visible al público
        Publicada,  // Noticia publicada, la puede ver el público
        Oculta,     // No visible
        Eliminada   // Marcada como eliminada, solo un administrador la puede eliminar realmente o volverla a habilitar
    }
}