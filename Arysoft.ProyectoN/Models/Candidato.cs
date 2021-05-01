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
        /// Nombre de la coalición, solo si el tipo de candidato es Coalicion
        /// </summary>
        [StringLength(150)]
        [Display(Name = "Coalición")]
        public string Coalicion { get; set; }

        [StringLength(10)]
        [Display(Name = "Siglas")]
        public string SiglasCoalicion { get; set; }

        [StringLength(255)]
        [Display(Name = "Logotipo")]
        public string ArchivoCoalicion { get; set; }

        public StatusTipo Status { get; set; }

        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // RELACIONES

        public ICollection<Partido> Partidos { get; set; }
    }
}