using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.Website.Models
{
    public class NoticiasListViewModel
    {
        public Guid NoticiaID { get; set; }

        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string FriendlyUrl { get; set; }
        public string Autor { get; set; }
        public int ContadorVisitas { get; set; }
        public IdiomaTipo Idioma { get; set; }
        [Display(Name = "Publicado")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime FechaPublicacion { get; set; }
        [StringLength(255)]
        public string ImagenArchivo { get; set; }
        public int MeGusta { get; set; }

        public NoticiasListViewModel(Noticia noticia)
        {
            this.NoticiaID = noticia.NoticiaID;
            this.Titulo = noticia.Titulo;
            this.Resumen = noticia.Resumen;
            this.FriendlyUrl = noticia.FriendlyUrl;
            this.Autor = noticia.Autor;
            this.ContadorVisitas = noticia.ContadorVisitas;
            this.Idioma = noticia.Idioma;
            this.FechaPublicacion = noticia.FechaPublicacion;
            this.ImagenArchivo = noticia.ImagenArchivo;
            this.MeGusta = noticia.MeGusta;
        } // NoticiasListViewModel

    } // NoticiasListViewModels
}