using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arysoft.ProyectoN.Models
{
    public class CalleListViewModel
    {
        public Guid CalleID { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        public StatusTipo Status { get; set; }

        public List<string> Colonias { get; set; }

        public CalleListViewModel(Calle calle)
        {
            this.CalleID = calle.CalleID;
            this.Nombre = calle.Nombre;
            this.Status = calle.Status;
            this.Colonias = new List<string>();

            if (calle.Colonias != null)
            {
                foreach (var colonia in calle.Colonias)
                {
                    Colonias.Add(colonia.Nombre);
                }
            }
        } // CalleListViewModel
    }
}