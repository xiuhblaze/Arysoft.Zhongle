using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Candidatos")]
    public class Candidato
    {
        public Guid CandidatoID { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(1000)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [StringLength(255)]
        [Display(Name = "Fotografía")]
        public string ArchivoFotografia { get; set; }

        [Display(Name = "Votos")]
        public int VotosTotal { get; set; }

        public CampannaTipo Campanna { get; set; }

        public CandidatoTipo Tipo { get; set; }

        /// <summary>
        /// Nombre de la coalición, solo si el tipo de candidato <see cref="CandidatoTipo"/> es Coalicion
        /// </summary>
        [StringLength(150)]
        [Display(Name = "Coalición")]
        public string Coalicion { get; set; }

        [StringLength(10)]
        [Display(Name = "Siglas")]
        public string SiglasCoalicion { get; set; }

        /// <summary>
        /// Archivo que contiene el logotipo de la coalición
        /// </summary>
        [StringLength(255)]
        [Display(Name = "Logotipo")]
        public string ArchivoCoalicion { get; set; }

        /// <summary>
        /// Color en hexadecimal a utilizar en los textos referentes al candidato
        /// </summary>
        [StringLength(10)]
        public string Color { get; set; }

        public StatusTipo Status { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // RELACIONES

        public ICollection<Partido> Partidos { get; set; }

        // NOT MAPPED

        /// <summary>
        /// Obtiene o establece un valor que se pasa a las vistas y determina si el origen del modelo
        /// es desde un metodo de consulta o para dar de baja el registro
        /// </summary>
        [NotMapped]
        public string Origen { get; set; }

        /// <summary>
        /// Obtiene o establece un valor qe se pasa a las vistas para determinar si la información
        /// se va a mostrar como solo lectura o para edición
        /// </summary>
        [NotMapped]
        public bool SoloLectura { get; set; }
    }
}