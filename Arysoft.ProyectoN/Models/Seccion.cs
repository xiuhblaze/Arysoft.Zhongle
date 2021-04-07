using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    [Table("Secciones")]
    public class Seccion
    {
        public Guid SeccionID { get; set; }

        /// <summary>
        /// Identificador del sector al que pertenece
        /// </summary>
        [Display(Name = "Sector")]
        public Guid SectorID { get; set; }

        /// <summary>
        /// Número de sección
        /// </summary>
        [Display(Name = "Número")]
        public int Numero { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(500)]
        public string Descripcion { get; set; }

        [Display(Name = "Status")]
        public StatusTipo Status { get; set; }
        
        [Display(Name = "Actualizado por")]
        [StringLength(256)]
        public string UserNameActualizacion { get; set; }

        [Display(Name = "Última actualización")]
        [DataType(DataType.DateTime)]
        public DateTime FechaActualizacion { get; set; }

        // SOLO LECTURA

        /// <summary>
        /// Suma de los votantes de todas las casillas pertenecientes a la sección
        /// </summary>
        [Display(Name = "Total de votantes")]
        public int TotalVotantes
        {
            get 
            {
                if (Casillas != null && Casillas.Count > 0)
                {
                    return Casillas.Sum(c => c.NumeroVotantes);
                }
                return 0;
            }
        }

        // RELACIONES

        public Sector Sector { get; set; }

        public ICollection<Casilla> Casillas { get; set; }

        public ICollection<Colonia> Colonias { get; set; }
        
        public ICollection<Persona> Personas { get; set; }

        // NOT MAPED

        [NotMapped]
        public bool SoloLectura { get; set; }

        /// <summary>
        /// Obtiene o establece un valor que se pasa a las vistas y determina si el origen del modelo
        /// es desde un metodo de consulta o para dar de baja el registro
        /// </summary>
        [NotMapped]
        public string NpOrigen { get; set; }
    } // Seccion
}