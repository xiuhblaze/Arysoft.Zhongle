using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Casillas")]
    public class Casilla
    {
        public Guid CasillaID { get; set; }
                
        [Display(Name = "Sección")]
        public Guid? SeccionID { get; set; }

        [Display(Name = "Ubicación")]
        public Guid? UbicacionID { get; set; }
        
        [Display(Name = "Afiliado responsable")]
        public Guid? PersonaResponsableID { get; set; }

        public CasillaTipo Tipo { get; set; }
        
        [Display(Name = "Nombre del dueño")]
        [StringLength(150)]
        public string PersonaDuenno { get; set; }

        [Display(Name = "Número de votantes")]
        public int NumeroVotantes { get; set; }

        [Display(Name = "Status")]
        public StatusTipo Status { get; set; }

        [Display(Name = "Actualizado por")]
		[StringLength(256)]
		public string UserNameActualizacion { get; set; }

		[Display(Name = "Última actualización")]
		[DataType(DataType.DateTime)]
		public DateTime FechaActualizacion { get; set; }

        // SOLO LECTURA

        public int VotosSeguros
        {
            get {
                int total = 0;

                if (Votantes != null)
                {
                    //foreach (Voto v in Votantes)
                    //{
                    //    if (v.Persona != null)
                    //    {
                    //        if (v.Persona.VotanteSeguro == BoolTipo.Si)
                    //        {
                    //            total++;
                    //        }
                    //    }
                    //}
                    total = Votantes.Where(v => v.Persona.VotanteSeguro == BoolTipo.Si).Count();
                }
                return total;
            }
        } // VotosSeguros

        public int PorcentajeVotos
        {
            get {
                double porcentaje = 0;

                if (NumeroVotantes > 0)
                {
                    porcentaje = (VotosSeguros * 100) / NumeroVotantes;
                }

                return (int)porcentaje;
            }
        } // PorcentajeVotos

        public int YaVotaron
        {
            get
            {
                int total = 0;

                if (Votantes != null)
                {
                    //foreach (Voto item in Votantes)
                    //{
                    //    if (item.YaVoto == BoolTipo.Si)
                    //    {
                    //        total++;
                    //    }
                    //}
                    total = Votantes.Where(v => v.YaVoto == BoolTipo.Si).Count();
                }

                return total;
            }
        } // YaVotaron

        public int VotantesRestantes
        {
            get {
                int restan = 0;

                if (NumeroVotantes > 0)
                {
                    restan = NumeroVotantes - YaVotaron;
                }
                return restan;
            }
        } // VotantesRestantes
        
        // RELACIONES

        public Seccion Seccion { get; set; }

        public Ubicacion Ubicacion { get; set; }

        [ForeignKey("PersonaResponsableID")]
        public Persona PersonaResponsable { get; set; }

        [ForeignKey("CasillaID")]
        public ICollection<Persona> Representantes { get; set; }

        [ForeignKey("CasillaID")]
        public ICollection<Voto> Votantes { get; set; }

        [ForeignKey("PropietarioID")]
        public ICollection<Nota> Notas { get; set; }

        public ICollection<ResultadoCasilla> Resultados { get; set; }

        // NOT MAPPED

        [NotMapped]
        public bool SoloLectura { get; set; }

        [NotMapped]
        public string NmOrigen { get; set; }
    }

    [NotMapped]
    public class CasillaEditModel : Casilla
    {
        public Guid CalleID { get; set; }

        public Guid ColoniaID { get; set; }

        [Display(Name = "No. Ext.")]
        public int NumExterior { get; set; }

        [Display(Name = "Letra")]
        public string Letra { get; set; }

        [Display(Name = "No. Int.")]
        public string NumInterior { get; set; }

        [Display(Name = "Ubicación alterna")]
        public string Descripcion { get; set; }

        public float Latitud { get; set; }

        public float Longitud { get; set; }

    } // CasillaEditModel

    [NotMapped]
    public class CasillaResultadosViewModel
    {
        public Casilla Casilla { get; set; }

        public ICollection<Partido> Partidos { get; set; }
    } // CasillaResultadosViewModel
}
