using System;
using System.Linq;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using Arysoft.ProyectoN.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace Arysoft.ProyectoN.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {

            var SeccionesList = new SelectList(db.Secciones.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Numero), "SeccionID", "Numero").ToList();

            SeccionesList.Insert(0, new SelectListItem { Text = "(seleccionar)", Value = Guid.Empty.ToString() });
            ViewBag.SeccionID = SeccionesList;

            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [Authorize]
        public ActionResult MigrarRepresentantes()
        {
            // 1.Verificar que tenga un nombre y un apellido
            // 2.Verificar que no exista en el sistema
            // Existe
            //      2.1.Agregarlo como Representante de casilla
            // No Existe
            //      2.1.Agregarlo como persona
            //      2.2.Agregarlo como representante de casilla
            List<Persona> personas = new List<Persona>();

            foreach (var item in db.RepresentantesTemp.ToList())
            {
                if (!string.IsNullOrEmpty(item.Nombres)
                    && (!string.IsNullOrEmpty(item.ApellidoPaterno) || !string.IsNullOrEmpty(item.ApellidoMaterno)))
                {
                    item.Nombres = item.Nombres.PrepareToCompareString();
                    item.ApellidoPaterno = item.ApellidoPaterno.PrepareToCompareString();
                    item.ApellidoMaterno = item.ApellidoMaterno.PrepareToCompareString();

                    // Buscando si existe la persona
                    var persona = db.Personas
                        .Include(p => p.UbicacionVive)
                        .Include(p => p.UbicacionVota)
                        .Where(p => p.Nombres.Equals(item.Nombres)
                            && p.ApellidoPaterno.Equals(item.ApellidoPaterno)
                            && p.ApellidoMaterno.Equals(item.ApellidoMaterno))
                        .FirstOrDefault();

                    // Obteniendo la sección donde aparece su INE
                    Seccion seccionIne = null;
                    if (int.TryParse(item.SeccionVota.Trim(), out int seccionVota))
                    {
                        seccionIne = db.Secciones
                            .Where(s => s.Numero == seccionVota)
                            .FirstOrDefault();
                    }
                    Guid SeccionID = seccionIne != null ? seccionIne.SeccionID : Guid.Empty;

                    // Obteniendo la sección donde va a ser representante de casilla
                    Seccion seccion = null;
                    if (int.TryParse(item.Seccion.Trim(), out int seccionRep))
                    {
                        seccion = db.Secciones
                            .Where(s => s.Numero == seccionRep)
                            .FirstOrDefault();
                    }

                    // Obtener casilla donde va a ser representante
                    CasillaTipo casillaTipo = CasillaTipo.Ninguno;
                    switch (item.Casilla)
                    {
                        case "B1": casillaTipo = CasillaTipo.Basica; break;
                        case "C1": casillaTipo = CasillaTipo.Contigua; break;
                        case "C2": casillaTipo = CasillaTipo.ContiguaII; break;
                        case "C3": casillaTipo = CasillaTipo.ContiguaIII; break;
                        case "C4": casillaTipo = CasillaTipo.ContiguaIV; break;
                        case "C5": casillaTipo = CasillaTipo.ContiguaV; break;
                        case "C6": casillaTipo = CasillaTipo.ContiguaVI; break;
                        case "C7": casillaTipo = CasillaTipo.ContiguaVII; break;
                        case "C8": casillaTipo = CasillaTipo.ContiguaVIII; break;
                        case "C9": casillaTipo = CasillaTipo.ContiguaIX; break;
                        case "C10": casillaTipo = CasillaTipo.ContiguaX; break;
                        case "C11": casillaTipo = CasillaTipo.ContiguaXI; break;
                        case "C12": casillaTipo = CasillaTipo.ContiguaXII; break;
                        case "E1": casillaTipo = CasillaTipo.Extraordinaria; break;
                        case "S1": casillaTipo = CasillaTipo.Especial; break;
                    }
                    Casilla casilla = db.Casillas
                        .Where(c => c.SeccionID == seccion.SeccionID && c.Tipo == casillaTipo)
                        .FirstOrDefault();
                    Guid CasillaID = casilla != null ? casilla.CasillaID : Guid.Empty;

                    // Determinar si el representante es Titular o Suplente
                    RepresentanteTipo representanteTipo = RepresentanteTipo.Ninguno;
                    if (string.Compare(item.Puesto.PrepareToCompareString(),"TITULAR") == 0) { representanteTipo = RepresentanteTipo.Principal; }
                    else if (string.Compare(item.Puesto.PrepareToCompareString(), "SUPLENTE") == 0) { representanteTipo = RepresentanteTipo.Suplente; }

                    // Armar la UbicacionAlterna

                    string ubicacionAlterna = string.Empty;
                    if (!string.IsNullOrEmpty(item.Calle)) { ubicacionAlterna = item.Calle; }
                    if (!string.IsNullOrEmpty(item.Numero)) { ubicacionAlterna += " " + item.Numero; }
                    if (!string.IsNullOrEmpty(item.Interior)) { ubicacionAlterna += " " + item.Interior; }
                    if (!string.IsNullOrEmpty(item.Colonia)) { ubicacionAlterna += " " + item.Colonia; }
                    if (!string.IsNullOrEmpty(item.CP)) { ubicacionAlterna += " " + item.CP; }
                    ubicacionAlterna = ubicacionAlterna.Trim();
                    Ubicacion u = new Ubicacion
                    {
                        UbicacionID = Guid.NewGuid(),
                        CalleID = Guid.Empty,
                        ColoniaID = Guid.Empty,
                        NumExterior = 0,
                        Letra = "(r)", //string.Empty,
                        NumInterior = string.Empty,
                        Descripcion = ubicacionAlterna,
                        Latitud = 0,
                        Longitud = 0
                    };
                    db.Ubicaciones.Add(u); // BDD

                    Guid sectorBrigadaID = new Guid("982ff706-94cc-4ad4-ad88-1518b57e4867"); // Id de Produccion
                    //Guid sectorBrigadaID = new Guid("0b9eadd3-afa9-4643-9988-9c18c1ec02de"); // Id de Desarrollo

                    if (persona == null) // no existe, es nueva persona
                    {
                        persona = new Persona
                        {
                            PersonaID = item.ID, // Para poder localizarlo y borrarlo si algo sale mal.
                            SectorBrigadaID = sectorBrigadaID,
                            SeccionID = SeccionID,
                            UbicacionViveID = u.UbicacionID,
                            CasillaID = CasillaID,
                            Nombres = item.Nombres,
                            ApellidoPaterno = item.ApellidoPaterno,
                            ApellidoMaterno = item.ApellidoMaterno,
                            Sexo = SexoTipo.Ninguno,
                            VotaEnDomicilio = BoolTipo.Si,
                            Telefono = string.Empty,
                            Celular = !string.IsNullOrEmpty(item.Telefono) ? item.Telefono.PrepareToCompareString() : string.Empty,
                            CorreoElectronico = string.Empty,
                            Afinidad = AfinidadTipo.Simpatizante,
                            VotanteSeguro = BoolTipo.Si,
                            TieneBarda = BoolTipo.No,
                            TieneLona = BoolTipo.No,
                            RepresentanteTipo = representanteTipo,
                            RepresentanteAsistencia = BoolTipo.Ninguno,
                            RepresentanteCapacitacion = BoolTipo.Ninguno,
                            Verificada = BoolTipo.Ninguno,
                            FechaNacimiento = null,
                            EstadoCivil = EstadoCivilTipo.Ninguno,
                            Whatsapp = BoolTipo.Ninguno,
                            Ocupacion = "(representante-new)", //string.Empty,
                            FechaAlta = DateTime.Now,
                            Status = StatusTipo.Activo,
                            UserNameActualizacion = "xiuhblaze@gmail.com",
                            FechaActualizacion = DateTime.Now
                        };                        
                        db.Personas.Add(persona);
                    }
                    else 
                    {
                        //persona.SectorBrigadaID = sectorBrigadaID;
                        persona.SeccionID = SeccionID;
                        if (persona.UbicacionVive != null)
                        {
                            persona.UbicacionVive.Descripcion = ubicacionAlterna;
                            db.Entry(persona.UbicacionVive).State = EntityState.Modified; // BDD
                        }
                        persona.CasillaID = CasillaID;
                        persona.Telefono = !string.IsNullOrEmpty(item.Telefono) ? item.Telefono.PrepareToCompareString() : string.Empty;
                        persona.VotanteSeguro = BoolTipo.Si;
                        persona.RepresentanteTipo = representanteTipo;
                        persona.RepresentanteAsistencia = BoolTipo.Ninguno;
                        persona.RepresentanteCapacitacion = BoolTipo.Ninguno;
                        persona.Ocupacion = "(representante-up)";
                        persona.Status = StatusTipo.Activo;
                        persona.UserNameActualizacion = "xiuhblaze@gmail.com";
                        persona.FechaActualizacion = DateTime.Now;
                        db.Entry(persona).State = EntityState.Modified; // BDD
                    }

                    personas.Add(persona);

                    //db.SaveChanges(); // BDD
                }
            }

            // return View(db.RepresentantesTemp.ToList());
            return View(personas);
        } // MigrarRepresentantes

        // METODOS AJAX
        ////////////////////////////////////////////////////////////////////////////////

        public async Task<ActionResult> VotosRealizados() {
            int totalVotos = await db.Casillas
                .Where(c => c.Status == StatusTipo.Activo).SumAsync(c => c.NumeroVotantes);

            int totalVotosRealizados = await db.Votos
                .Where(v => v.YaVoto == BoolTipo.Si).CountAsync();

            int totalVotosSeguros = await db.Personas
                .Where(p => p.VotanteSeguro == BoolTipo.Si && p.Status == StatusTipo.Activo).CountAsync();

            int totalVotosSegurosRealizados = await db.Personas
                .Where(p => p.VotanteSeguro == BoolTipo.Si 
                    && p.Status == StatusTipo.Activo
                    && (p.Voto != null && p.Voto.YaVoto == BoolTipo.Si)
                    ).CountAsync();

            return Json(new {
                TotalVotos = totalVotos,                
                TotalVotosRealizados = totalVotosRealizados,
                TotalVotosSeguros = totalVotosSeguros,
                TotalVotosSegurosRealizados = totalVotosSegurosRealizados
            }, JsonRequestBehavior.AllowGet);

        } // VotosRealizados

        public async Task<ActionResult> VotosRealizadosPorSector()
        {
            var sectores = await (db.Sectores
                .Include(s => s.Secciones.Select(c => c.Casillas.Select(cs => cs.Votantes)))
                .Where(s => s.Status == StatusTipo.Activo)
                .OrderBy(s => s.Nombre)).ToListAsync();

            var tabla = new List<object>();

            foreach (var sector in sectores)
            {
                int totalVotos = 0;
                int totalVotosRealizados = 0;
                int totalVotosSeguros = 0;
                int totalVotosSegurosRealizados = 0;

                foreach (var seccion in sector.Secciones)
                {
                    totalVotos += seccion.Casillas
                        .Where(c => c.Status == StatusTipo.Activo).Sum(c => c.NumeroVotantes);

                    foreach (var casilla in seccion.Casillas)
                    {
                        totalVotosRealizados += casilla.Votantes
                            .Where(v => v.YaVoto == BoolTipo.Si)
                            .Count();

                        totalVotosSeguros += casilla.Votantes
                            .Where(v => v.Persona != null 
                                && v.Persona.VotanteSeguro == BoolTipo.Si
                                && v.Persona.Status == StatusTipo.Activo
                                ).Count();

                        totalVotosSegurosRealizados += casilla.Votantes
                            .Where(v => v.YaVoto == BoolTipo.Si 
                                && v.Persona != null
                                && v.Persona.VotanteSeguro == BoolTipo.Si
                                && v.Persona.Status == StatusTipo.Activo
                                ).Count();
                    }
                }

                tabla.Add(new {
                    sector = sector.Nombre,
                    totalVotos,
                    totalVotosRealizados,
                    totalVotosSeguros,
                    totalVotosSegurosRealizados
                });
            }

            return Json(tabla, JsonRequestBehavior.AllowGet);

        } // VotosRealizadosPorSector

        public async Task<ActionResult> VotosTotalPorHora()
        {
            var tabla = new List<object>();

            foreach (VotoHoraTipo hora in Enum.GetValues(typeof(VotoHoraTipo)))
            {
                if (hora != VotoHoraTipo.Ninguno)
                {
                    int totalVotosNoSeguros = await db.Votos.Where(v => v.VotoHora == hora && v.YaVoto == BoolTipo.Si).CountAsync();
                    int totalVotosSeguros = await db.Votos
                        .Where(v => (v.Persona != null && v.Persona.Status == StatusTipo.Activo && v.Persona.VotanteSeguro == BoolTipo.Si)
                            && v.VotoHora == hora
                            && v.YaVoto == BoolTipo.Si).CountAsync();

                    totalVotosNoSeguros = totalVotosNoSeguros - totalVotosSeguros;

                    tabla.Add(new
                    {
                        hora = hora.GetDisplayName(),
                        votosSeguros = totalVotosSeguros,
                        votosNoSeguros = totalVotosNoSeguros
                    });
                }
            }

            return Json(tabla, JsonRequestBehavior.AllowGet);
        } // VotosTotalPorHora

        public async Task<ActionResult> VotosCasillaPorHora(Guid id)
        {
            var tabla = new List<object>();

            if (id != null)
            {
                foreach (VotoHoraTipo hora in Enum.GetValues(typeof(VotoHoraTipo)))
                {
                    if (hora != VotoHoraTipo.Ninguno)
                    {
                        int totalVotosNoSeguros = await db.Votos.Where(v => v.VotoHora == hora
                                                                && v.YaVoto == BoolTipo.Si
                                                                && v.CasillaID == id).CountAsync();
                        int totalVotosSeguros = await db.Votos
                            .Where(v => (v.Persona != null && v.Persona.Status == StatusTipo.Activo && v.Persona.VotanteSeguro == BoolTipo.Si)
                                && v.VotoHora == hora
                                && v.YaVoto == BoolTipo.Si
                                && v.CasillaID == id).CountAsync();

                        totalVotosNoSeguros = totalVotosNoSeguros - totalVotosSeguros;

                        tabla.Add(new
                        {
                            hora = hora.GetDisplayName(),
                            votosSeguros = totalVotosSeguros,
                            votosNoSeguros = totalVotosNoSeguros
                        });
                    }
                }
            }

            return Json(tabla, JsonRequestBehavior.AllowGet);
        } // VotosCasillaPorHora

        public async Task<ActionResult> ResultadosTotales()
        {
            var tabla = new List<object>();
            var partidos = await db.Partidos.Where(p => p.Status == StatusTipo.Activo).ToListAsync();
            
            //foreach (Partido partido in partidos)
            //{
            //    int total = await db.ResultadoCasillas.Where(r => r.PartidoID == partido.PartidoID).SumAsync(r => r.Total);

            //    tabla.Add(new
            //    {
            //        Partido = partido.Siglas,
            //        Total = total
            //    });
            //}

            return Json(tabla, JsonRequestBehavior.AllowGet);
        } // ResultadosTotales

        public async Task<ActionResult> ResultadosPorCasilla(Guid id)
        {
            var tabla = new List<object>();

            Casilla casilla = await db.Casillas.FindAsync(id);

            var candidatos = await db.Candidatos.Where(c => c.Status == StatusTipo.Activo).ToListAsync();
            //var partidos = await db.Partidos.Where(p => p.Status == StatusTipo.Activo).ToListAsync();

            foreach (Candidato candidato in candidatos)
            {
                int total = 0;

                if (casilla.Resultados != null)
                {
                    total = casilla.Resultados.Where(r => r.CandidatoID == candidato.CandidatoID).Sum(r => r.Votos);
                }

                tabla.Add(new
                {
                    Partido = candidato.SiglasCoalicion,
                    Total = total
                });
            }

            return Json(tabla, JsonRequestBehavior.AllowGet);
        } // ResultadosPorCasilla

        public async Task<ActionResult> ObtenerBingo(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return Content("notid");
            }

            Casilla casilla = await db.Casillas
                .Include(c => c.Seccion)
                .Include(c => c.Votantes)
                .Include(c => c.Votantes.Select(v => v.Persona))
                .FirstOrDefaultAsync(c => c.CasillaID == id);

            if (casilla == null)
            {
                return Content("notfound");
            }

            CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            return PartialView("_listaBingo", casillaEM);
        } // ObtenerBingo

        public async Task<ActionResult> ObtenerPersonasFaltantes(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return Content("notid");
            }

            Casilla casilla = await db.Casillas
                .Include(c => c.Seccion)
                .Include(c => c.Seccion.Sector)
                .Include(c => c.Votantes)
                .Include(c => c.Votantes.Select(v => v.Persona))
                //.Include(c => c.Votantes.Select(v => v.Persona.UbicacionVive))
                //.Include(c => c.Votantes.Select(v => v.Persona.UbicacionVive.Calle))
                //.Include(c => c.Votantes.Select(v => v.Persona.UbicacionVive.Colonia))
                //.Include(c => c.Votantes.Select(v => v.Persona.UbicacionVota))
                //.Include(c => c.Votantes.Select(v => v.Persona.UbicacionVota.Calle))
                //.Include(c => c.Votantes.Select(v => v.Persona.UbicacionVota.Colonia))
                .FirstOrDefaultAsync(c => c.CasillaID == id);

            if (casilla == null)
            {
                return Content("notfound");
            }

            CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            return PartialView("_listaPersonasFaltantes", casillaEM);

        } // ObtenerPersonasFaltantes

        //public JsonResult ObtenerVotos()
        //{
        //    int votosSi = 0;
        //    int votosNo = 0;
        //    int votosNull = 0;
        //    //int votosTotal = 0;

        //    var query = (from c in db.Casillas
        //                where c.Estatus == CasillaEstatus.Activa && c.Centro.Estatus == CentroEstatus.Activo
        //                select c).ToList();

        //    if (query != null)
        //    {
        //        votosSi = query.Sum(c => c.TotalVotosSI);
        //        votosNo = query.Sum(c => c.TotalVotosNO);
        //        votosNull = query.Sum(c => c.TotalVotosNULL);
        //    }

        //    return Json(new
        //    {
        //        votosSI = votosSi,
        //        votosNO = votosNo,
        //        votosNULL = votosNull
        //    }, JsonRequestBehavior.AllowGet);
        //} // GET: ObtenerVotos

        //[HttpPost]
        //public JsonResult Ubicaciones()
        //{

        //    var centro = (from c in db.Centros where c.Estatus == CentroEstatus.Activo select c).ToList();
        //    List<Object> centros = new List<Object>();

        //    if (centro != null && centro.Count > 0)
        //    {
        //        foreach (Centro item in centro)
        //        {
        //            centros.Add(new
        //            {
        //                nombre = item.Nombre,
        //                descripcion = item.Descripcion,
        //                ubicacion = item.Ubicacion,
        //                latitud = item.Latitud,
        //                longitud = item.Longitud,
        //                casillas = item.Casillas == null ? 0 : item.Casillas.Count,
        //                boletas = item.TotalBoletas,
        //                votosSi = item.TotalVotosSI,
        //                votosNo = item.TotalVotosNO,
        //                votosNull = item.TotalVotosNULL,
        //                votosTotales = item.TotalVotos
        //            });
        //        }
        //    }
        //    else
        //    {
        //        return Json(new
        //        {
        //            mensaje = "nodata"
        //        });
        //    }

        //    return Json(centros);
        //} // Ubicaciones
    }
}
