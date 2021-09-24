using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Arysoft.Website.Areas.Admin.Models
{
    public static class Comun
    {
        // CONSTANTES

        public const string TITULO_APP = "Zhongle Admin";
        public const int ELEMENTOS_PAGINA = 25;
        public const string SESION_PERSONAS_FILTRO = "SESION_PERSONAS_FILTRO";

        public static readonly string[] FILES_ALLOWED_EXTENSIONS = { ".jpg", ".gif", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".rar"  };
        public static readonly string[] FILES_IMAGES_EXTENSIONS = { ".jpg", ".gif", ".png" };
        public static readonly string[] PREPOSITIONS_ES = { " a ", " ante ", " bajo ", " cabe" , " con ",
            " contra ", " de ", " desde ", " durante ", " en ", " entre ", " hacia" , " hasta ",
            " mediante" , " para ", " por ", " según ", " sin ", " so ", " sobre ", " tras ", 
            " versus ", " vía "};

        // METODOS

        /// <summary>
        /// A partir del nombre del archivo y su extensión, determina si es una imagen o no.
        /// </summary>
        /// <param name="filename">Nombre del archivo con su extensión</param>
        /// <returns></returns>
        public static bool EsImagen(string filename)
        {
            string fextension = System.IO.Path.GetExtension(filename);
            return Array.Exists(FILES_IMAGES_EXTENSIONS, e => e == fextension);
        } // EsImagen
    }
}