using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Arysoft.Website.Models;

namespace Arysoft.Website.Areas.Admin.Models
{
    [NotMapped]
    public class PaginaIndexListViewModel
    {
        public Guid PaginaID { get; set; }
        public string Titulo { get; set; }
        public string EtiquetaMenu { get; set; }
        public string Resumen { get; set; }
        public string FriendlyUrl { get; set; }
        public string TargetUrl { get; set; }
        public PaginaTarget Target { get; set; }
        public BoolTipo TieneGaleria { get; set; }
        [Display(Name = "Visitas")]
        public int ContadorVisitas { get; set; }
        public IdiomaTipo Idioma { get; set; }
        public BoolTipo EsPrincipal { get; set; }
        public int MeGusta { get; set; }
        public PaginaStatus Status { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaActualizacion { get; set; }

        public string TituloPaginaPadre { get; set; }

        public int ContadorArchivos { get; set; }
        public int ContadorNotas { get; set; }

        public PaginaIndexListViewModel(Pagina pagina)
        {
            this.PaginaID = pagina.PaginaID;
            this.Titulo = pagina.Titulo;
            this.EtiquetaMenu = pagina.EtiquetaMenu;
            this.Resumen = pagina.Resumen;
            this.FriendlyUrl = pagina.FriendlyUrl;
            this.TargetUrl = pagina.TargetUrl;
            this.Target = pagina.Target;
            this.TieneGaleria = pagina.TieneGaleria;
            this.ContadorVisitas = pagina.ContadorVisitas;
            this.Idioma = pagina.Idioma;
            this.EsPrincipal = pagina.EsPrincipal;
            this.MeGusta = pagina.MeGusta;
            this.Status = pagina.Status;
            this.FechaActualizacion = pagina.FechaActualizacion;

            this.TituloPaginaPadre = pagina.PaginaPadre != null ? pagina.PaginaPadre.Titulo : "(es página padre)";

            this.ContadorArchivos = pagina.Archivos != null ? pagina.Archivos.Count : 0;
            this.ContadorNotas = pagina.Notas != null ? pagina.Notas.Count : 0;
        }
    } // PaginaIndexListViewModel

    [NotMapped]
    public class PaginaDetailsViewModel : Pagina 
    {
        /// <summary>
        /// Obtiene o establece un valor que se pasa a las vistas y determina si el origen del modelo
        /// es desde un metodo de consulta o para dar de baja el registro
        /// </summary>        
        public string Origen { get; set; }

        /// <summary>
        /// Obtiene o establece un valor qe se pasa a las vistas para determinar si la información
        /// se va a mostrar como solo lectura o para edición
        /// </summary>        
        public bool SoloLectura { get; set; }

        // CONSTRUCTORES

        public PaginaDetailsViewModel(Pagina pagina, string origen = "", bool? soloLectura = false)
        {
            this.PaginaID = pagina.PaginaID;
            this.PaginaPadreID = pagina.PaginaPadreID;
            this.Titulo = pagina.Titulo;
            this.IndiceMenu = pagina.IndiceMenu;
            this.EtiquetaMenu = pagina.EtiquetaMenu;
            this.Resumen = pagina.Resumen;
            this.HTMLContent = pagina.HTMLContent;
            this.FriendlyUrl = pagina.FriendlyUrl;
            this.TargetUrl = pagina.TargetUrl;
            this.Target = pagina.Target;
            this.TieneGaleria = pagina.TieneGaleria;
            this.ContadorVisitas = pagina.ContadorVisitas;
            this.FechaContador = pagina.FechaContador;
            this.Idioma = pagina.Idioma;
            this.EsPrincipal = pagina.EsPrincipal;
            this.HTMLHeadScript = pagina.HTMLHeadScript;
            this.HTMLFooterScript = pagina.HTMLFooterScript;
            this.MeGusta = pagina.MeGusta;
            this.Status = pagina.Status;
            
            this.FechaCreacion = pagina.FechaCreacion;
            this.FechaActualizacion = pagina.FechaActualizacion;
            this.UsuarioActualizacion = pagina.UsuarioActualizacion;

            this.PaginaPadre = pagina.PaginaPadre;
            this.Archivos = pagina.Archivos.OrderBy(a => a.Nombre).ToList();
            this.Notas = pagina.Notas.OrderByDescending(n => n.FechaCreacion).ToList();
            this.PaginasHijo = pagina.PaginasHijo.OrderBy(p => p.IndiceMenu).ToList();

            this.Origen = origen;
            this.SoloLectura = soloLectura ?? false;
        } // PaginaDetailsViewModel :: Constructor

    } // PaginaDetailsViewModel

    public class PaginaEditViewModel
    {
        public Guid PaginaID { get; set; }

        [Display(Name = "Página padre"), DisplayFormat(NullDisplayText = "(es página padre)")]
        public Guid? PaginaPadreID { get; set; }

        [Display(Name = "Título"), StringLength(150)]
        [Required(ErrorMessage = "Es necesario el título de la página")]
        public string Titulo { get; set; }

        [Display(Name = "Indice", Description = "Indice para el orden con la etiqueta de menu")]
        [Range(0, 100, ErrorMessage = "El valor no puede ser menor que cero ni mayor a 100")]
        public int IndiceMenu { get; set; }

        [Display(Name = "Etiqueta para menú"), StringLength(30)]
        [Required(ErrorMessage = "Falta indicar la etiqueta para el menú")]
        public string EtiquetaMenu { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string __resumen { get; set; }

        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Es necesario el contenido de la página")]
        public string __content { get; set; }

        [Display(Name = "Url Amigable", Description = "Permite crear una dirección Url en base al Título, de forma amigable para el usuario.")]
        [StringLength(255)]
        public string FriendlyUrl { get; set; }

        [Display(Name = "Dirección URL", Description = "Direccion url a la cual, cuando se llame la página esta va a saltar (esta opción ignora el contenido de la página)."), StringLength(255)]
        public string TargetUrl { get; set; }

        public PaginaTarget Target { get; set; }

        [Display(Name = "Tiene galeria")]
        public BoolTipo TieneGaleria { get; set; }

        [Display(Name = "Visitas")]
        public int ContadorVisitas { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Conteo desde")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaContador { get; set; }

        public IdiomaTipo Idioma { get; set; }

        [Display(Name = "Es principal de sección")]
        public BoolTipo EsPrincipal { get; set; }

        [Display(Name = "Script encabezado")]
        [DataType(DataType.MultilineText)]
        public string __headScript { get; set; }

        [Display(Name = "Script final")]
        [DataType(DataType.MultilineText)]
        public string __footerScript { get; set; }

        [Display(Name = "Me gusta")]
        public int MeGusta { get; set; }

        public PaginaStatus Status { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaCreacion { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Actualización")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaActualizacion { get; set; }

        [StringLength(150)]
        [Display(Name = "Actualizado por")]
        public string UsuarioActualizacion { get; set; }

        /// <summary>
        /// Obtien el orden de las páginas hijo, para ser actualizado, si se encuentra vacio
        /// es que no se alteró el orden.
        /// </summary>
        public string OrdenPaginasHijo { get; set; }

        // RELACIONES

        public Pagina PaginaPadre { get; set; }

        public ICollection<Pagina> PaginasHijo { get; set; }

        public ICollection<Archivo> Archivos { get; set; }

        public ICollection<Nota> Notas { get; set; }

        // CONSTRUCTORES

        public PaginaEditViewModel()
        { }

        public PaginaEditViewModel(Pagina p)
        {
            this.PaginaID = p.PaginaID;
            this.PaginaPadreID = p.PaginaPadreID;
            this.Titulo = p.Titulo;
            this.IndiceMenu = p.IndiceMenu;
            this.EtiquetaMenu = p.EtiquetaMenu;
            this.__resumen = p.Resumen;
            this.__content = p.HTMLContent;
            this.FriendlyUrl = p.FriendlyUrl;
            this.TargetUrl = p.TargetUrl;
            this.Target = p.Target;
            this.TieneGaleria = p.TieneGaleria;
            this.ContadorVisitas = p.ContadorVisitas;
            this.FechaContador = p.FechaContador;
            this.Idioma = p.Idioma;
            this.EsPrincipal = p.EsPrincipal;
            this.__headScript = p.HTMLHeadScript;
            this.__footerScript = p.HTMLFooterScript;
            this.MeGusta = p.MeGusta;
            this.Status = p.Status;

            this.FechaCreacion = p.FechaCreacion;
            this.FechaActualizacion = p.FechaActualizacion;
            this.UsuarioActualizacion = p.UsuarioActualizacion;

            this.OrdenPaginasHijo = string.Empty;

            this.PaginaPadre = p.PaginaPadre;
            this.Archivos = p.Archivos.OrderBy(a => a.Nombre).ToList();
            this.Notas = p.Notas.OrderByDescending(n => n.FechaCreacion).ToList();
            this.PaginasHijo = p.PaginasHijo.OrderBy(ph => ph.IndiceMenu).ToList();
        } // PaginaEditViewModel : Constructor

        // METODOS

        public Pagina ObtenerPagina()
        {
            Pagina p = new Pagina
            {
                PaginaID = this.PaginaID,
                PaginaPadreID = this.PaginaPadreID,
                Titulo = this.Titulo,
                IndiceMenu = this.IndiceMenu,
                EtiquetaMenu = this.EtiquetaMenu,
                Resumen = this.__resumen,
                HTMLContent = this.__content,
                FriendlyUrl = this.FriendlyUrl,
                TargetUrl = this.TargetUrl,
                Target = this.Target,
                TieneGaleria = this.TieneGaleria,
                ContadorVisitas = this.ContadorVisitas,
                FechaContador = this.FechaContador,
                Idioma = this.Idioma,
                EsPrincipal = this.EsPrincipal,
                HTMLHeadScript = this.__headScript,
                HTMLFooterScript = this.__footerScript,
                MeGusta = this.MeGusta,
                Status = this.Status,
                FechaCreacion = this.FechaCreacion
            };

            return p;
        } // ObternerPagina

    } // PaginaEditViewModel
}