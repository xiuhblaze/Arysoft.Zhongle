using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Arysoft.ProyectoN.Models;

using PagedList;

namespace Arysoft.ProyectoN.Controllers
{
    [Authorize]
    public class PersonaController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Persona
        [Authorize(Roles = "Admin, Editor, Auditor, Consultant, SectorEditor")]
        public async Task<ActionResult> Index(string buscar, string filtro, string orden, string afinidad, string SectorID, string SeccionID, string PromotorID,
            string CalleID, string ColoniaID, string bardaLona, string sectorTipo, string votanteSeguro, string status, string verificada,
            string filtroEspecifico, string yaVoto, string busquedaAvanzada, string llamadaOrigen,
            int? pagina)
        {
            Guid sectorID = (SectorID != null && SectorID != Guid.Empty.ToString()) ? new Guid(SectorID) : Guid.Empty;
            Guid seccionID = (SeccionID != null && SeccionID != Guid.Empty.ToString()) ? new Guid(SeccionID) : Guid.Empty;
            Guid calleID = (CalleID != null && CalleID != Guid.Empty.ToString()) ? new Guid(CalleID) : Guid.Empty;
            Guid coloniaID = (ColoniaID != null && ColoniaID != Guid.Empty.ToString()) ? new Guid(ColoniaID) : Guid.Empty;
            Guid promotorID = (PromotorID != null && PromotorID != Guid.Empty.ToString()) ? new Guid(PromotorID) : Guid.Empty;

            if (User.IsInRole("SectorEditor"))
            {
                //HACK: Obligar el filtro por Sector, de acuerdo al que estará asignado el usuario                
                sectorID = User.Identity.GetSectorId();
                sectorTipo = string.IsNullOrEmpty(sectorTipo) ? "0" : sectorTipo;
            }

            ViewBag.Orden = orden;
            ViewBag.OrdenNombre = string.IsNullOrEmpty(orden) ? "nombre_desc" : "";
            ViewBag.OrdenBrigada = orden == "brigada" ? "brigada_desc" : "brigada";
            ViewBag.OrdenSector = orden == "sector" ? "sector_desc" : "sector";
            ViewBag.OrdenSeccion = orden == "seccion" ? "seccion_desc" : "seccion";
            ViewBag.OrdenColonia = orden == "colonia" ? "colonia_desc" : "colonia";
            ViewBag.OrdenCalle = orden == "calle" ? "calle_desc" : "calle";

            if (buscar != null)
            {
                pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            db.Database.CommandTimeout = 180;

            var personas = db.Personas
                .Include(p => p.Seccion)
                .Include(p => p.Seccion.Sector)
                .Include(p => p.PersonasAfines)
                .Include(p => p.UbicacionVive)
                .Include(p => p.UbicacionVota)
                .Include(p => p.Voto)
                .Include(p => p.Voto.Casilla)
                .Include(p => p.Notas)                
                .Where(p => p.Status == StatusTipo.Activo || p.Status == StatusTipo.Baja);
            var personasList = new List<Persona>();

            //if (!int.TryParse(buscar, out int seccion)) { seccion = 0; } else { buscar = string.Empty; }

            // Busqueda BASE DE DATOS
            if (string.IsNullOrEmpty(status))
            {
                status = StatusTipo.Activo.ToString();
            }

            if (Request.IsAjaxRequest() && llamadaOrigen == "to-estatal") //HACK: xBlaze: Para encontrar los de la segunda etapa - QUITAR LUEGO!!
            {
                votanteSeguro = BoolTipo.Si.ToString();
            }

            if (!string.IsNullOrEmpty(buscar)
                || sectorID != Guid.Empty //!string.IsNullOrEmpty(SectorID)
                || seccionID != Guid.Empty //!string.IsNullOrEmpty(SeccionID)
                || calleID != Guid.Empty
                || coloniaID != Guid.Empty
                || promotorID != Guid.Empty
                || !string.IsNullOrEmpty(afinidad)
                || (!string.IsNullOrEmpty(bardaLona) && bardaLona != "0")
                || !string.IsNullOrEmpty(votanteSeguro)
                || !string.IsNullOrEmpty(status)
                || !string.IsNullOrEmpty(yaVoto))
            {
                string[] wordsToMatch = buscar.Split(new char[] { ' ', '.', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                bool hayPalabras = wordsToMatch.Length != 0; //!string.IsNullOrEmpty(buscar);

                afinidad = string.IsNullOrEmpty(afinidad) ? AfinidadTipo.Ninguno.ToString() : afinidad;
                votanteSeguro = string.IsNullOrEmpty(votanteSeguro) ? BoolTipo.Ninguno.ToString() : votanteSeguro;
                bardaLona = string.IsNullOrEmpty(bardaLona) ? "0" : bardaLona;
                status = string.IsNullOrEmpty(status) ? StatusTipo.Ninguno.ToString() : status;
                verificada = string.IsNullOrEmpty(verificada) ? BoolTipo.Ninguno.ToString() : verificada;
                yaVoto = string.IsNullOrEmpty(yaVoto) ? BoolTipo.Ninguno.ToString() : yaVoto;
                
                personas = personas.Where(p =>
                    (
                        (!hayPalabras)
                        || (from w in wordsToMatch where p.Nombres.Contains(w) select w).ToList().Count > 0
                        || (from w in wordsToMatch where p.ApellidoPaterno.Contains(w) select w).ToList().Count > 0
                        || (from w in wordsToMatch where p.ApellidoMaterno.Contains(w) select w).ToList().Count > 0
                        || (from w in wordsToMatch where p.CorreoElectronico.Contains(w) select w).ToList().Count > 0
                    )
                    //(
                    //    !hayPalabras
                    //    || p.Nombres.Contains(buscar)
                    //    || p.ApellidoPaterno.Contains(buscar)
                    //    || p.ApellidoMaterno.Contains(buscar)
                    //    || p.CorreoElectronico.Contains(buscar)
                    //)
                    && (afinidad == AfinidadTipo.Ninguno.ToString() ? true : p.Afinidad.ToString() == afinidad)
                    && (sectorID == Guid.Empty ? true :
                            sectorTipo == "0" ? (p.SectorBrigadaID == sectorID || p.Seccion.SectorID == sectorID) :
                            sectorTipo == "1" ? p.SectorBrigadaID == sectorID :
                            sectorTipo == "2" ? p.Seccion.SectorID == sectorID :
                            sectorTipo == "3" ? p.Promotor.SectorBrigadaID == sectorID : false)
                    && (seccionID == Guid.Empty ? true : p.SeccionID == seccionID)
                    && (calleID == Guid.Empty ? true : p.UbicacionVive.CalleID == calleID || p.UbicacionVota.CalleID == calleID)
                    && (coloniaID == Guid.Empty ? true : p.UbicacionVive.ColoniaID == coloniaID || p.UbicacionVota.ColoniaID == coloniaID)
                    && (promotorID == Guid.Empty ? true : p.PersonaPromotorID == promotorID)
                    && (bardaLona == "0" ? true : 
                            bardaLona == "1" ? p.TieneBarda == BoolTipo.Si :
                            bardaLona == "2" ? p.TieneLona == BoolTipo.Si :
                            bardaLona == "3" ? (p.TieneBarda == BoolTipo.Si || p.TieneLona == BoolTipo.Si) :
                            bardaLona == "4" ? (p.TieneBarda == BoolTipo.Si && p.TieneLona == BoolTipo.Si) : false)
                    && (votanteSeguro == BoolTipo.Ninguno.ToString() ? true : p.VotanteSeguro.ToString() == votanteSeguro)
                    && (status == StatusTipo.Ninguno.ToString() ? true : p.Status.ToString() == status)
                    && (verificada == BoolTipo.Ninguno.ToString() ? true : p.Verificada.ToString() == verificada)
                    && (yaVoto == BoolTipo.Ninguno.ToString() ? true :
                        p.Voto == null ? yaVoto == BoolTipo.No.ToString() :
                        p.Voto.YaVoto.ToString() == yaVoto.ToString())
                );
            }

            // Si se llama para el reporte estatal, por default ordenarlo por sección
            if (Request.IsAjaxRequest() && llamadaOrigen == "to-estatal") {
                orden = "seccion";
            }

            switch (orden)
            {
                case "nombre_desc":
                    personas = personas.OrderByDescending(p => p.ApellidoPaterno).ThenByDescending(p => p.ApellidoMaterno);
                    break;                
                case "sector":
                    personas = personas
                        .OrderBy(p => p.Seccion == null)
                        .ThenBy(p => p.Seccion.Sector == null)
                        .ThenBy(p => p.Seccion.Sector.Nombre);
                    break;
                case "sector_desc":
                    personas = personas
                        .OrderByDescending(p => p.Seccion == null)
                        .ThenByDescending(p => p.Seccion.Sector == null)
                        .ThenByDescending(p => p.Seccion.Sector.Nombre);
                    break;
                case "seccion":
                    personas = personas
                        .OrderBy(p => p.Seccion == null)
                        .ThenBy(p => p.Seccion.Numero)
                        .ThenBy(p => p.ApellidoPaterno)
                        .ThenBy(p => p.ApellidoMaterno)
                        .ThenBy(p=>p.Nombres);
                    break;
                case "seccion_desc  ":
                    personas = personas
                        .OrderByDescending(p => p.Seccion == null)
                        .ThenByDescending(p => p.Seccion.Numero)
                        .ThenBy(p => p.ApellidoPaterno);
                    break;
                case "brigada":
                    personas = personas
                        .OrderBy(p => p.Sector == null)
                        .ThenBy(p => p.Sector.Nombre)
                        .ThenBy(p => p.ApellidoPaterno);
                    break;
                case "brigada_desc":
                    personas = personas
                        .OrderByDescending(p => p.Sector == null)
                        .ThenByDescending(p => p.Sector.Nombre)
                        .ThenBy(p => p.ApellidoPaterno);
                    break;
                case "colonia":
                    personas = personas
                        .OrderBy(p => p.UbicacionVive == null)
                        .ThenBy(p => p.UbicacionVive.Colonia == null)
                        .ThenBy(p => p.UbicacionVive.Colonia.Nombre);
                    break;
                case "colonia_desc  ":
                    personas = personas
                        .OrderByDescending(p => p.UbicacionVive == null)
                        .ThenByDescending(p => p.UbicacionVive.Colonia == null)
                        .ThenByDescending(p => p.UbicacionVive.Colonia.Nombre);
                    break;
                case "calle":
                    personas = personas
                        .OrderBy(p => p.UbicacionVive == null)
                        .ThenBy(p => p.UbicacionVive.Calle == null)
                        .ThenBy(p => p.UbicacionVive.Calle.Nombre)
                        .ThenBy(p => p.UbicacionVive.NumExterior);
                    //personas = personas.ToList();
                    break;
                case "calle_desc  ":
                    personas = personas
                        .OrderByDescending(p => p.UbicacionVive == null)
                        .ThenByDescending(p => p.UbicacionVive.Calle == null)
                        .ThenByDescending(p => p.UbicacionVive.Calle.Nombre)
                        .ThenByDescending(p => p.UbicacionVive.NumExterior);
                    break;
                default:
                    personas = personas.OrderBy(p => p.ApellidoPaterno).ThenBy(p => p.ApellidoMaterno);
                    break;
            }

            personasList = await personas.ToListAsync();

            // Busqueda especifica en memoria

            if (!string.IsNullOrEmpty(filtroEspecifico))
            {
                PersonasFiltroEspecificoTipo miFiltro = (PersonasFiltroEspecificoTipo)Enum.Parse(typeof(PersonasFiltroEspecificoTipo), filtroEspecifico);

                switch (miFiltro) {
                    case PersonasFiltroEspecificoTipo.SinSeccion:
                        personasList = personasList.Where(p => p.SeccionID == null || p.SeccionID == Guid.Empty).ToList();
                        break;
                    case PersonasFiltroEspecificoTipo.SinCasilla:
                        personasList = personasList.Where(p => p.Voto == null).ToList();
                        break;
                    case PersonasFiltroEspecificoTipo.SinNumeroINE:
                        personasList = personasList.Where(p => p.Voto == null || ( p.Voto != null && p.Voto.NumeroINE == 0)).ToList();
                        break;
                }
            }

            if (string.IsNullOrEmpty(SectorID)) { SectorID = Guid.Empty.ToString(); }

            ViewBag.Afinidad = afinidad;
            ViewBag.BusquedaAvanzada = string.IsNullOrEmpty(busquedaAvanzada) ? "0" : busquedaAvanzada;

            ViewBag.SectorID = await ObtenerListaSectoresAsync(sectorID);
            ViewBag.SeccionID = await ObtenerListaSeccionesAsync(seccionID);
            ViewBag.CalleID = await ObtenerListaCallesAsync(calleID);
            ViewBag.ColoniaID = await ObtenerListaColoniasAsync(coloniaID);
            ViewBag.PromotorID = await ObtenerListaPromotoresAsync(promotorID);

            ViewBag.SectorIDSelected = sectorID;
            ViewBag.SeccionIDSelected = seccionID;
            ViewBag.CalleIDSelected = calleID;
            ViewBag.ColoniaIDSelected = coloniaID;
            ViewBag.PromotorIDSelected = promotorID;

            ViewBag.BardaLona = bardaLona;
            ViewBag.SectorTipo = sectorTipo;
            ViewBag.VotanteSeguro = votanteSeguro;
            ViewBag.Status = status;
            ViewBag.Verificada = verificada;
            ViewBag.YaVoto = yaVoto;
            ViewBag.FiltroEspecifico = filtroEspecifico;

            ViewBag.Count = personasList.Count();

            int numeroPagina = (pagina ?? 1);
            int elementosPagina = 50;

            var personasPagedList = personasList.ToPagedList(numeroPagina, elementosPagina);

            foreach (var p in personasPagedList)
            {
                p.EsResponsableSector = Comun.EsPersonaResponsable(p.PersonaID);
            }

            if (Request.IsAjaxRequest())
            {
                if (llamadaOrigen == "to-estatal")
                {
                    //personasList = personasList
                    //    .OrderBy(p => p.Seccion == null)
                    //    .ThenBy(p => p.Seccion.Numero)                        
                    //    .ThenBy(p => p.NombreCompleto)                        
                    //    .ToList();

                    return PartialView("_listaEstatalToExcel", personasList);
                }
                return PartialView("_listaToExcel", personasList);
            }

            return View(personasPagedList); //View(personasList.ToPagedList(numeroPagina, elementosPagina)); //personas.ToList());
        } // Index

        // GET: Persona/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest())
                {
                    return Content("noid");
                }
                return RedirectToAction("Index");
            }
            Persona persona = await db.Personas
                .Include(p => p.PersonasAfines)
                .Include(p => p.PersonasAfines.Select(pa => pa.Notas))
                .Include(p => p.PersonasAfines.Select(pa => pa.Seccion))
                .Include(p => p.PersonasAfines.Select(pa => pa.Sector))
                .Include(p => p.PersonasAfines.Select(pa => pa.Promotor))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVive))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVive.Calle))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVive.Colonia))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVota))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVota.Calle))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVota.Colonia))
                .Include(p => p.Promotor)
                .Include(p => p.UbicacionVive)
                .Include(p => p.UbicacionVive.Calle)
                .Include(p => p.UbicacionVive.Colonia)
                .Include(p => p.UbicacionVota)
                .Include(p => p.UbicacionVota.Calle)
                .Include(p => p.UbicacionVota.Colonia)
                .Include(p => p.Sector)
                .Include(p => p.Seccion)
                .Include(p => p.Seccion.Sector)
                .Where(p => p.PersonaID == id)
                .FirstOrDefaultAsync();
            if (persona == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("nofound"); }
                return RedirectToAction("Index");
            }
            persona.SoloLectura = true;
            persona.EsResponsableSector = Comun.EsPersonaResponsable(persona.PersonaID);
            if (Request.IsAjaxRequest())
            {
                persona.NmOrigen = "details";
                PersonaEditViewModel personaVM = Comun.ObtenerPersonaEditViewModel(persona, true);
                return PartialView("_details", personaVM);
            }

            return View(persona);
        } // Detail

        // GET: Persona/Create
        [Authorize(Roles = "Admin, Editor, SectorEditor")]
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(ControllerContext.HttpContext.User.Identity.Name);

            Persona persona = new Persona();

            persona.PersonaID = Guid.NewGuid();
            persona.SeccionID = Guid.Empty;
            persona.UbicacionViveID = Guid.Empty;
            persona.VotaEnDomicilio = BoolTipo.Si;
            persona.Afinidad = AfinidadTipo.Ninguno;
            persona.VotanteSeguro = BoolTipo.Si;
            persona.TieneBarda = BoolTipo.Ninguno;
            persona.TieneLona = BoolTipo.Ninguno;

            persona.EstadoCivil = EstadoCivilTipo.Ninguno;
            persona.Whatsapp = BoolTipo.Ninguno;

            persona.FechaAlta = DateTime.Now;
            persona.Verificada = BoolTipo.Ninguno;
            persona.Status = StatusTipo.Ninguno;

            persona.FechaActualizacion = DateTime.Now;
            persona.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;

            db.Personas.Add(persona);

            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = persona.PersonaID });
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }

            // return View();
        } // GET: Create

        // GET: Persona/Edit/5
        [Authorize(Roles = "Admin, Editor, SectorEditor")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            Guid CalleID, ColoniaID, VotaCalleID, VotaColoniaID;

            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
            }

            // Persona persona = await db.Personas.FindAsync(id);
            Persona persona = await db.Personas
                .Include(p => p.PersonasAfines)
                .Include(p => p.PersonasAfines.Select(pa => pa.Notas))
                .Include(p => p.PersonasAfines.Select(pa => pa.Seccion))
                .Include(p => p.PersonasAfines.Select(pa => pa.Sector))
                .Include(p => p.PersonasAfines.Select(pa => pa.Promotor))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVive))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVive.Calle))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVive.Colonia))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVota))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVota.Calle))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVota.Colonia))
                .Include(p => p.Promotor)
                .Include(p => p.UbicacionVive)
                .Include(p => p.UbicacionVive.Calle)
                .Include(p => p.UbicacionVive.Colonia)
                .Include(p => p.UbicacionVota)
                .Include(p => p.UbicacionVota.Calle)
                .Include(p => p.UbicacionVota.Colonia)
                .Include(p => p.Sector)
                .Include(p => p.Seccion)
                .Include(p => p.Seccion.Sector)
                .Include(p => p.Notas)
                .Where(p => p.PersonaID == id)
                .FirstOrDefaultAsync();

            if (persona == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
            }

            if (User.IsInRole("SectorEditor") && persona.Status != StatusTipo.Ninguno && persona.Seccion.SectorID != User.Identity.GetSectorId())
            {
                TempData["MessageBox"] = "No se puede editar una persona que se encuentra en otro Sector asignado al Usuario.";
                return RedirectToAction("Index");
            }

            if (persona.Status == StatusTipo.Baja)
            {
                TempData["MessageBox"] = "No se puede editar una persona dada de baja, primero reactivela desde la ventana de detalles del mismo usuario.";
                return RedirectToAction("Index");
            }

            if (persona.Status == StatusTipo.Eliminado)
            {
                TempData["MessageBox"] = "No se puede editar una persona eliminada, consulte a los administradores.";
                return RedirectToAction("Index");
            }

            ViewBag.PersonaPromotorID = await ObtenerListaPromotoresAsync(persona.PersonaPromotorID ?? Guid.Empty); //new SelectList(db.Personas.Where(p => p.Status != StatusTipo.Ninguno && p.Afinidad == AfinidadTipo.Afiliado).OrderBy(p => p.Nombres), "PersonaID", "Nombres", persona.PersonaPromotorID);
            if (persona.Afinidad == AfinidadTipo.Ninguno && User.IsInRole("SectorEditor"))
            {
                ViewBag.SeccionID = new List<SelectListItem>
                {
                    new SelectListItem { Value = Guid.Empty.ToString(), Text = "(primero seleccione la Afinidad)" }
                };                
            }
            else 
            {
                ViewBag.SeccionID = await ObtenerListaSeccionesAsync(persona.SeccionID ?? Guid.Empty); // new SelectList(db.Secciones.OrderBy(s => s.Numero), "SeccionID", "Numero", persona.SeccionID);
            }
            ViewBag.SectorBrigadaID = await ObtenerListaSectoresAsync(persona.SectorBrigadaID ?? (User.IsInRole("SectorEditor") ? User.Identity.GetSectorId() : Guid.Empty)); //Guid.Empty); //new SelectList(db.Sectores.OrderBy(s => s.Nombre), "SectorID", "Nombre", persona.SectorBrigadaID);

            if (persona.UbicacionVive == null)
            {
                CalleID = Guid.Empty;
                ColoniaID = Guid.Empty;
            }
            else
            {
                CalleID = persona.UbicacionVive.CalleID;
                ColoniaID = persona.UbicacionVive.ColoniaID;
            }

            ViewBag.CalleID = await ObtenerListaCallesAsync(CalleID); //new SelectList(db.Calles.OrderBy(c => c.Nombre), "CalleID", "Nombre", CalleID);
            ViewBag.ColoniaID = await ObtenerListaColoniasAsync(ColoniaID); //new SelectList(db.Colonias.OrderBy(c => c.Nombre), "ColoniaID", "Nombre", ColoniaID);

            if (persona.UbicacionVota == null)
            {
                VotaCalleID = Guid.Empty;
                VotaColoniaID = Guid.Empty;
            }
            else
            {
                VotaCalleID = persona.UbicacionVota.CalleID;
                VotaColoniaID = persona.UbicacionVota.ColoniaID;
            }

            //if (persona.Seccion != null && persona.Seccion.Casillas != null)
            //{
            //    Guid casillaID = Guid.Empty;

            //    if(persona.Voto != null) { casillaID = persona.Voto.CasillaID; }

            //    ViewBag.CasillaVotaID = ObtenerListaCasillas(persona.SeccionID ?? Guid.Empty, casillaID);
            //}

            ViewBag.VotaCalleID = await ObtenerListaCallesAsync(VotaCalleID); //new SelectList(db.Calles.OrderBy(c => c.Nombre), "CalleID", "Nombre", VotaCalleID);
            ViewBag.VotaColoniaID = await ObtenerListaColoniasAsync(VotaColoniaID); // new SelectList(db.Colonias.OrderBy(c => c.Nombre), "ColoniaID", "Nombre", VotaColoniaID);

            PersonaEditViewModel personaVM = Comun.ObtenerPersonaEditViewModel(persona, false);

            return View(personaVM);
        } // GET: Edit

        // POST: Persona/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Editor, SectorEditor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PersonaID,SectorBrigadaID,SeccionID,PersonaPromotorID,CasillaID,Nombres,ApellidoPaterno,ApellidoMaterno,VotaEnDomicilio,Telefono,Celular,CorreoElectronico,Afinidad,VotanteSeguro,TieneBarda,TieneLona,RepresentanteTipo,RepresentanteAsistencia,RepresentanteCapacitacion,FechaAlta,Verificada,FechaNacimiento,Whatsapp,Status,UserNameActualizacion,FechaActualizacion,CalleID,ColoniaID,NumExterior,NumInterior,Descripcion,Latitud,Longitud,VotaCalleID,VotaColoniaID,VotaNumExterior,VotaNumInterior,VotaDescripcion,VotaLatitud,VotaLongitud")] PersonaEditViewModel persona, string notaCheckbox, string submitButton)
        {
            bool esNuevo = persona.Status == StatusTipo.Ninguno;
            Guid CalleID, ColoniaID, VotaCalleID, VotaColoniaID, ubicacionID, votaUbicacionID;

            if (esNuevo)
            {
                persona.Status = StatusTipo.Activo;
            }

            if (persona.PersonaPromotorID == null) { persona.PersonaPromotorID = Guid.Empty; }

            ubicacionID = await BuscarUbicacionAsync(persona.CalleID, persona.ColoniaID, persona.NumExterior, persona.Letra, persona.NumInterior);
            if (persona.VotaEnDomicilio == BoolTipo.No)
            {
                votaUbicacionID = await BuscarUbicacionAsync(persona.VotaCalleID, persona.VotaColoniaID, persona.VotaNumExterior, persona.VotaLetra, persona.VotaNumInterior);
            }
            else { votaUbicacionID = Guid.Empty; }

            if (persona.SeccionID == null || persona.SeccionID == Guid.Empty)
            {
                ModelState.AddModelError("SeccionID", "Faltó especificar la sección donde vota la persona.");
            }

            // Solo por si alguien intenta forzar el cambio de sector de una persona
            if (User.IsInRole("SectorEditor"))
            {
                //TODO: Se debe de validar contra la sección donde vota
                //persona.SectorBrigadaID = User.Identity.GetSectorId();
            }

            // Si hubo algún cambio, agregar la nota

            if (!string.IsNullOrEmpty(notaCheckbox))
            {
                await AgregarNotaAsync(persona.PersonaID, notaCheckbox, User.Identity.Name, PropietarioTipo.Persona);
            }

            if (ModelState.IsValid)
            {
                Persona p = Comun.ObtenerPersona(persona, false);

                p.UbicacionViveID = ubicacionID == Guid.Empty ? AgregarUbicacion(persona, false) : ubicacionID;
                if (p.VotaEnDomicilio == BoolTipo.No)
                {
                    p.UbicacionVotaID = (votaUbicacionID == Guid.Empty) ? AgregarUbicacion(persona, true) : votaUbicacionID;
                }

                p.UserNameActualizacion = User.Identity.Name;
                p.FechaActualizacion = DateTime.Now;

                if (await ExistePersonaAsync(p.PersonaID))
                {
                    db.Entry(p).State = EntityState.Modified;
                }
                else
                {
                    db.Personas.Add(p);
                }

                try
                {
                    await db.SaveChangesAsync();

                    if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                    else { TempData["MessageBox"] = "Cambios guardados con exito."; }

                    if (submitButton == "save-new")
                    {
                        return RedirectToAction("Create");
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                }
            } // IsValid

            // SI NO ES VALIDO RETORNAR A LA VISTA

            ViewBag.PersonaPromotorID = await ObtenerListaPromotoresAsync(persona.PersonaPromotorID ?? Guid.Empty);  // new SelectList(db.Personas.Where(p => p.Status != StatusTipo.Ninguno && p.Afinidad == AfinidadTipo.Afiliado).OrderBy(p => p.Nombres), "PersonaID", "Nombres", persona.PersonaPromotorID);
            //ViewBag.SeccionID = await ObtenerListaSeccionesAsync(persona.SeccionID ?? Guid.Empty); // new SelectList(db.Secciones.OrderBy(s => s.Numero), "SeccionID", "Numero", persona.SeccionID);
            if (persona.Afinidad == AfinidadTipo.Ninguno && User.IsInRole("SectorEditor"))
            {
                ViewBag.SeccionID = new List<SelectListItem>
                {
                    new SelectListItem { Value = Guid.Empty.ToString(), Text = "(primero seleccione la Afinidad)" }
                };
            }
            else
            {
                ViewBag.SeccionID = await ObtenerListaSeccionesAsync(persona.SeccionID ?? Guid.Empty); // new SelectList(db.Secciones.OrderBy(s => s.Numero), "SeccionID", "Numero", persona.SeccionID);
            }
            //ViewBag.SectorBrigadaID = await ObtenerListaSectoresAsync(persona.SectorBrigadaID ?? Guid.Empty); // new SelectList(db.Sectores.OrderBy(s => s.Nombre), "SectorID", "Nombre", persona.SectorBrigadaID);
            ViewBag.SectorBrigadaID = await ObtenerListaSectoresAsync(persona.SectorBrigadaID ?? (User.IsInRole("SectorEditor") ? User.Identity.GetSectorId() : Guid.Empty));

            if (persona.UbicacionVive == null)
            {
                CalleID = Guid.Empty;
                ColoniaID = Guid.Empty;
            }
            else
            {
                CalleID = persona.UbicacionVive.CalleID;
                ColoniaID = persona.UbicacionVive.ColoniaID;
            }

            ViewBag.CalleID = await ObtenerListaCallesAsync(CalleID); // new SelectList(db.Calles.OrderBy(c => c.Nombre), "CalleID", "Nombre", CalleID);
            ViewBag.ColoniaID = await ObtenerListaColoniasAsync(ColoniaID); //new SelectList(db.Colonias.OrderBy(c => c.Nombre), "ColoniaID", "Nombre", ColoniaID);

            if (persona.UbicacionVota == null)
            {
                VotaCalleID = Guid.Empty;
                VotaColoniaID = Guid.Empty;
            }
            else
            {
                VotaCalleID = persona.UbicacionVota.CalleID;
                VotaColoniaID = persona.UbicacionVota.ColoniaID;
            }
            
            //if (persona.SeccionID != Guid.Empty)
            //{
            //    ViewBag.CasillaVotaID = ObtenerListaCasillas(persona.SeccionID ?? Guid.Empty, persona.CasillaVotaID);
            //}

            ViewBag.VotaCalleID = await ObtenerListaCallesAsync(VotaCalleID); //new SelectList(db.Calles.OrderBy(c => c.Nombre), "CalleID", "Nombre", VotaCalleID);
            ViewBag.VotaColoniaID = await ObtenerListaColoniasAsync(VotaColoniaID); //new SelectList(db.Colonias.OrderBy(c => c.Nombre), "ColoniaID", "Nombre", VotaColoniaID);

            PersonaEditViewModel personaVM = Comun.ObtenerPersonaEditViewModel(persona, false);

            return View(personaVM);
        }

        // GET: Persona/Delete/5
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("notid"); }
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Persona persona = await db.Personas.FindAsync(id);

            Persona persona = await db.Personas
                .Include(p => p.PersonasAfines)
                .Include(p => p.PersonasAfines.Select(pa => pa.Notas))
                .Include(p => p.PersonasAfines.Select(pa => pa.Seccion))
                .Include(p => p.PersonasAfines.Select(pa => pa.Sector))
                .Include(p => p.PersonasAfines.Select(pa => pa.Promotor))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVive))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVive.Calle))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVive.Colonia))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVota))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVota.Calle))
                .Include(p => p.PersonasAfines.Select(pa => pa.UbicacionVota.Colonia))
                .Include(p => p.Promotor)
                .Include(p => p.UbicacionVive)
                .Include(p => p.UbicacionVive.Calle)
                .Include(p => p.UbicacionVive.Colonia)
                .Include(p => p.UbicacionVota)
                .Include(p => p.UbicacionVota.Calle)
                .Include(p => p.UbicacionVota.Colonia)
                .Include(p => p.Sector)
                .Include(p => p.Seccion)
                .Where(p => p.PersonaID == id)
                .FirstOrDefaultAsync();
            if (persona == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            persona.SoloLectura = true;
            persona.EsResponsableSector = Comun.EsPersonaResponsable(persona.PersonaID);
            if (Request.IsAjaxRequest())
            {
                persona.NmOrigen = "delete";
                PersonaEditViewModel personaVM = Comun.ObtenerPersonaEditViewModel(persona, true);
                return PartialView("_details", personaVM);
            }
            return View(persona);
        } // Delete

        // POST: Persona/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Persona persona = await db.Personas.FindAsync(id);

            if (!Comun.EsPersonaResponsable(persona.PersonaID))
            {

                if (persona.Status == StatusTipo.Activo)
                {
                    persona.Status = StatusTipo.Baja;
                    persona.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                    persona.FechaActualizacion = DateTime.Now;
                    db.Entry(persona).State = EntityState.Modified;
                    TempData["MessageBox"] = persona.NombreCompleto + " ha sido dado de baja.";
                }
                else if (persona.Status == StatusTipo.Baja)
                {
                    persona.Status = StatusTipo.Eliminado;
                    persona.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                    persona.FechaActualizacion = DateTime.Now;
                    db.Entry(persona).State = EntityState.Modified;
                    TempData["MessageBox"] = persona.NombreCompleto + " ha sido eliminado.";
                }
                else if (persona.Status == StatusTipo.Eliminado)
                {
                    if (persona.PersonasAfines.Count > 0)
                    {
                        TempData["MessageBox"] = persona.NombreCompleto + ", no se puede eliminar definitivamente porque tiene personas asociadas (afines).";
                    }
                    else
                    {
                        TempData["MessageBox"] = persona.NombreCompleto + " ha sido eliminado definitivamente, esta acción no se puede eshacer.";
                        db.Personas.Remove(persona);
                    }
                }

                await db.SaveChangesAsync();
            }
            else {
                TempData["MessageBox"] = persona.NombreCompleto + " es una persona responsable de un sector, no se puede dar de baja.";
            }

            return RedirectToAction("Index");
        } // DeleteConfirmed

        // GET: Persona/Activar/5
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Activar(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Persona persona = db.Personas.Find(id);
            if (persona == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
                //return HttpNotFound();
            }

            persona.Status = StatusTipo.Activo;
            persona.UserNameActualizacion = User.Identity.Name;
            persona.FechaActualizacion = DateTime.Now;
            db.Entry(persona).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "A ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }

            await AgregarNotaAsync(persona.PersonaID, "[REACTIVADA] La persona estaba dada de baja y fué reactivada satisfactoriamente", ControllerContext.HttpContext.User.Identity.Name, PropietarioTipo.Persona);

            TempData["MessageBox"] = persona.NombreCompleto + " a sido reactivado(a) satisfactoriamente.";
            return RedirectToAction("Index");
        } // Activar

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // METODOS AJAX

        public async Task<ActionResult> BuscarSimilares(string nombres, string apellidoPaterno, string apellidoMaterno)
        {
            if (string.IsNullOrEmpty(apellidoPaterno) && string.IsNullOrEmpty(apellidoMaterno))
            {
                return Json(new { message = "nofound" }, JsonRequestBehavior.AllowGet);
            }

            var personas = await (db.Personas
                .Include(p => p.UbicacionVive)
                .Include(p => p.UbicacionVive.Colonia)
                .Include(p => p.UbicacionVive.Calle)
                .Include(p => p.Promotor)
                .Where(p => p.Status != StatusTipo.Ninguno
                && p.Nombres.Contains(nombres)
                && (p.ApellidoPaterno.Contains(apellidoPaterno) || string.IsNullOrEmpty(apellidoPaterno))
                && (p.ApellidoMaterno.Contains(apellidoMaterno) || string.IsNullOrEmpty(apellidoMaterno))))
                .ToListAsync();

            if (personas == null || personas.Count() == 0)
            {
                return Json(new { message = "nofound" }, JsonRequestBehavior.AllowGet);
            }

            return PartialView("_listaPersonasEncontradas", personas);
        } // BuscarSimilares

        public async Task<ActionResult> ObtenerSeccionesVota(string esPorSector)
        {
            Guid sectorID = Guid.Empty;

            if (esPorSector == "1")
            {
                sectorID = User.Identity.GetSectorId();
            }

            var secciones = await (from s in db.Secciones
                                   where s.Status == StatusTipo.Activo
                                    && (esPorSector == "0" ? true : s.SectorID == sectorID)
                                   orderby s.Numero
                                   select new { Id = s.SeccionID, Nombre = s.Numero }).ToListAsync();

            return Json(new
            {
                Secciones = secciones
            }, JsonRequestBehavior.AllowGet);
        } // ObtenerSeccionesVota

        public async Task<ActionResult> AgregarNota(Guid id, string texto, PropietarioTipo tipo)
        {
            Persona persona = db.Personas.Find(id);

            if (persona == null)
            {
                return Json(new { message = "notfound" }, JsonRequestBehavior.AllowGet);
            }

            if (await AgregarNotaAsync(persona.PersonaID, texto, ControllerContext.HttpContext.User.Identity.Name, PropietarioTipo.Persona))
            {
                var listado = await db.Notas.Where(n => n.PropietarioID == id).ToListAsync();
                    //(from n in db.Notas where n.PropietarioID == id select n);
                return PartialView("_listaNotas", listado);
            }
            else
            {
                return Json(new { message = "notadd" }, JsonRequestBehavior.AllowGet);
            }

        } // AgregarNota

        // Para el dashboard

        public async Task<ActionResult> Total1x10()
        {
            int totalMovilizadores = await (from p in db.Personas
                                      where p.Afinidad == AfinidadTipo.Movilizador && p.Status == StatusTipo.Activo
                                      select p).CountAsync();

            int totalAfines = await (from p in db.Personas
                               where p.Afinidad == AfinidadTipo.PorAfiliado && p.Status == StatusTipo.Activo
                               select p).CountAsync();

            return Json(new {
                totalMovilizadores,
                totalAfines
            }, JsonRequestBehavior.AllowGet);
        } // Total1x10

        public async Task<ActionResult> TotalVotantesSeguros()
        {
            int votosSeguros = await (from p in db.Personas
                                where p.VotanteSeguro == BoolTipo.Si && p.Status == StatusTipo.Activo
                                select p).CountAsync();

            int votosMeta = (from s in db.Sectores where s.Status == StatusTipo.Activo select s.VotosMeta).Sum();

            return Json(new
            {
                votosSeguros,
                votosMeta
            }, JsonRequestBehavior.AllowGet);
        } // TotalVotantesSeguros

        public async Task<ActionResult> TotalLonas()
        {
            int total = await (from p in db.Personas
                         where p.TieneLona == BoolTipo.Si && p.Status == StatusTipo.Activo && p.VotanteSeguro == BoolTipo.Si
                         select p).CountAsync();

            return Content(total.ToString());
        } // TotalLonas

        public async Task<ActionResult> TotalBardas()
        {
            int total = await (from p in db.Personas
                         where p.TieneBarda == BoolTipo.Si && p.Status == StatusTipo.Activo && p.VotanteSeguro == BoolTipo.Si
                         select p).CountAsync();

            return Content(total.ToString());
        } // TotalBardas

        public async Task<ActionResult> TotalCasillas()
        {
            int totalCasillas = await (from c in db.Casillas
                         where c.Status == StatusTipo.Activo
                         select c).CountAsync();

            int totalSecciones = await (from s in db.Secciones
                                  where s.Status == StatusTipo.Activo
                                  select s).CountAsync();

            return Json(new
            {
                totalCasillas,
                totalSecciones
            }, JsonRequestBehavior.AllowGet);
        } // TotalCasillas

        //public ActionResult

        // METODOS PRIVADOS

        private async Task<bool> ExistePersonaAsync(Guid id)
        {
            ApplicationDbContext dbLocal = new ApplicationDbContext();
            
            Persona persona = await dbLocal.Personas.FindAsync(id);

            if (persona == null) return false;

            return true;
        } // ExistePersona

        private async Task<bool> ExisteNumeroINEAsync(Guid CasillaID, Guid PersonaID, int NumeroINE)
        {
            bool existeNumeroINE = await db.Votos
                       .Where(v => v.CasillaID == CasillaID
                           && v.NumeroINE == NumeroINE
                           && v.Persona.PersonaID != PersonaID
                           && v.Persona.Status == StatusTipo.Activo)
                       .CountAsync() > 0;

            return existeNumeroINE;
        } // ExisteNumeroINE 

        private async Task<bool> AgregarNotaAsync(Guid id, string texto, string autor, PropietarioTipo tipo)
        {
            ApplicationDbContext dbLocal = new ApplicationDbContext();
            Nota nota = new Nota
            {
                NotaID = Guid.NewGuid(),
                PropietarioID = id,
                Autor = autor,
                Texto = texto,
                PropietarioTipo = PropietarioTipo.Persona,
                FechaPublicacion = DateTime.Now
            };

            dbLocal.Notas.Add(nota);
            try
            {
                await dbLocal.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        } // AgregarNota

        //     Listados

        private async Task<List<SelectListItem>> ObtenerListaPromotoresAsync(Guid selectedID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(promotor)", Value = Guid.Empty.ToString() });

            foreach (var item in await (db.Personas
                .Where(p => (p.Status == StatusTipo.Activo) && (p.Afinidad == AfinidadTipo.Movilizador || p.Afinidad == AfinidadTipo.Simpatizante))
                .OrderBy(p => p.ApellidoPaterno).ThenBy(p => p.ApellidoMaterno)).ToListAsync())
            {
                listado.Add(new SelectListItem {
                    Text = item.NombreCompleto,
                    Value = item.PersonaID.ToString(),
                    Selected = (item.PersonaID == selectedID)
                });
            }

            return listado;
        } // ObtenerListaPromotores

        private async Task<List<SelectListItem>> ObtenerListaSeccionesAsync(Guid selectedID)
        {
            Guid sectorID = Guid.Empty;

            if (User.IsInRole("SectorEditor"))
            {
                sectorID = User.Identity.GetSectorId();
            }

            var listado = new SelectList(await (db.Secciones
                .Where(s => s.Status == StatusTipo.Activo
                    && sectorID == Guid.Empty ? true : s.SectorID == sectorID)
                .OrderBy(s => s.Numero)).ToListAsync(), "SeccionID", "Numero", selectedID).ToList();

            listado.Insert(0, new SelectListItem { Text = "(sección)", Value = Guid.Empty.ToString() });

            return listado;
        } // ObtenerListaSecciones

        private async Task<List<SelectListItem>> ObtenerListaSectoresAsync(Guid selectedID)
        {

            var listado = new SelectList(await (db.Sectores
                .Where(s => s.Status == StatusTipo.Activo)
                .OrderBy(s => s.Nombre)).ToListAsync(), "SectorID", "Nombre", selectedID).ToList();

            listado.Insert(0, new SelectListItem { Text = "(sector)", Value = Guid.Empty.ToString() });

            return listado;
        } // ObtenerListaSectores

        private async Task<List<SelectListItem>> ObtenerListaColoniasAsync(Guid selectedID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(colonia)", Value = Guid.Empty.ToString() });

            var colonias = await db.Colonias
                .Include(c => c.Poblacion)
                .Where(c => c.Status == StatusTipo.Activo)
                .OrderBy(c => c.Nombre).ToListAsync();

            foreach (var item in colonias)
            {
                string secciones = string.Empty;

                if (item.Secciones != null)
                {
                    foreach (var seccion in item.Secciones)
                    {
                        secciones += "[" + seccion.Numero + "]";
                    }
                }

                listado.Add(new SelectListItem
                {
                    Text = item.Nombre + " (" + item.Poblacion.Nombre + ")" + secciones,
                    Value = item.ColoniaID.ToString(),
                    Selected = (item.ColoniaID == selectedID)
                });
            }

            return listado;
        } // ObtenerListaColonias

        private async Task<List<SelectListItem>> ObtenerListaCallesAsync(Guid selectedID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(calle)", Value = Guid.Empty.ToString() });

            foreach (var item in await (db.Calles
                .Where(c => c.Status == StatusTipo.Activo)
                .OrderBy(c => c.Nombre)).ToListAsync())
            {
                listado.Add(new SelectListItem
                {
                    Text = item.Nombre,
                    Value = item.CalleID.ToString(),
                    Selected = (item.CalleID == selectedID)
                });
            }

            return listado;
        } // ObtenerListaCalles

        // Casillas

        //private List<SelectListItem> ObtenerListaCasillas(Guid SeccionID, Guid CasillaID)
        //{
                        
        //    List<SelectListItem> listado = new List<SelectListItem>(); //new SelectList(persona.Seccion.Casillas.OrderBy(c => c.Tipo).ToList(), "CasillaID", "Tipo", casillaID);

        //    foreach (var item in db.Casillas
        //        .Where(c => c.SeccionID == SeccionID && c.Status == StatusTipo.Activo)
        //        .OrderBy(c => c.Tipo))
        //    {
        //        listado.Add(new SelectListItem {
        //            Text = item.Tipo.GetDisplayName(),
        //            Value = item.CasillaID.ToString(),
        //            Selected = (item.CasillaID == CasillaID)
        //        });
        //    }

        //    return listado;
        //} // ObtenerListaCasillas

        //     UBICACIONES

        private async Task<Guid> BuscarUbicacionAsync(Guid CalleID, Guid ColoniaID, int NumExterior, string Letra, string NumInterior)
        {
            Guid resultado = Guid.Empty;

            //Letra = Letra.Trim();
            //NumInterior = NumInterior.Trim();

            //if (CalleID == Guid.Empty && ColoniaID == Guid.Empty 
            //    && NumExterior == 0 
            //    && string.IsNullOrEmpty(Letra) 
            //    && string.IsNullOrEmpty(NumInterior))
            //{
            //    return Guid.Empty;
            //}

            var ubicaciones = await (from u in db.Ubicaciones
                            where u.CalleID == CalleID && u.ColoniaID == ColoniaID
                                && u.NumExterior == NumExterior
                                && string.Compare(u.Letra, Letra, true) == 0
                                && string.Compare(u.NumInterior, NumInterior, true) == 0
                            select u).ToListAsync();

            if (ubicaciones.Count > 0)
            {
                resultado = ubicaciones.FirstOrDefault().UbicacionID;
            }

            return resultado;
        } // BuscarUbicacion

        private Guid AgregarUbicacion(PersonaEditViewModel persona, bool ahiVota)
        {

            //if (persona.CalleID == Guid.Empty && persona.ColoniaID == Guid.Empty
            //    && persona.NumExterior == 0
            //    && string.IsNullOrEmpty(persona.Letra)
            //    && string.IsNullOrEmpty(persona.NumInterior))
            //{
            //    return Guid.Empty;
            //}

            Ubicacion ubicacion = new Ubicacion();

            ubicacion.UbicacionID = Guid.NewGuid();

            ubicacion.CalleID = ahiVota ? persona.VotaCalleID : persona.CalleID;
            ubicacion.ColoniaID = ahiVota ? persona.VotaColoniaID : persona.ColoniaID;
            ubicacion.NumExterior = ahiVota ? persona.VotaNumExterior : persona.NumExterior;
            ubicacion.Letra = ahiVota ? persona.VotaLetra : persona.Letra;
            ubicacion.NumInterior = ahiVota ? persona.VotaNumInterior : persona.NumInterior;
            ubicacion.Descripcion = ahiVota ? persona.VotaDescripcion : persona.Descripcion;
            ubicacion.Latitud = ahiVota ? persona.VotaLatitud : persona.Latitud;
            ubicacion.Longitud = ahiVota ? persona.VotaLongitud : persona.Longitud;

            db.Ubicaciones.Add(ubicacion);

            return ubicacion.UbicacionID;
        } // AgregarUbicacion

        private async Task EliminarRegistrosTemporalesAsync(string userName)
        {
            var personas = await (from p in db.Personas
                           where p.Status == StatusTipo.Ninguno
                            && string.Compare(p.UserNameActualizacion, userName, true) == 0
                           select p).ToListAsync();

            foreach (Persona persona in personas)
            {
                db.Personas.Remove(persona);
            }

            await db.SaveChangesAsync();
        } // EliminarRegistrosTemporales
        
        private ActionResult MigrarPersonas()
        {
            var personasTmp = (from p in db.PersonasTmp select p).ToList();
            int numeroDomicilio = 0;

            foreach (var pTemp in personasTmp)
            {
                Persona persona = new Persona
                {
                    PersonaID = Guid.NewGuid(),
                    Afinidad = AfinidadTipo.Simpatizante,
                    VotanteSeguro = BoolTipo.Si,
                    Status = StatusTipo.Activo
                };

                numeroDomicilio++;

                // Nombre
                string[] nombre = pTemp.Nombre.Split(' ');
                if (nombre.Length == 0)
                {
                    continue;
                }
                if (nombre.Length == 1)
                {
                    persona.Nombres = nombre[0];
                }
                else if (nombre.Length == 2)
                {
                    persona.Nombres = nombre[0];
                    persona.ApellidoPaterno = nombre[1];
                }
                else if (nombre.Length == 3)
                {
                    persona.Nombres = nombre[0];
                    persona.ApellidoPaterno = nombre[1];
                    persona.ApellidoMaterno = nombre[2];
                }
                else if (nombre.Length > 3)
                {
                    string nombres = string.Empty;
                    persona.ApellidoPaterno = nombre[nombre.Length - 2];
                    persona.ApellidoMaterno = nombre[nombre.Length - 1];
                    for(int i = 0; i < nombre.Length - 2; i++)
                    {
                        nombres += " " + nombre[i];
                    }
                    persona.Nombres = nombres.Trim();                    
                }

                // Sexo
                switch (pTemp.Sexo.Trim())
                {
                    case "M":
                        persona.Sexo = SexoTipo.Masculino;
                        break;
                    case "F":
                        persona.Sexo = SexoTipo.Femenino;
                        break;
                    default:
                        persona.Sexo = SexoTipo.Ninguno;
                        break;
                }

                // Ubicacion

                Ubicacion ubicacion = new Ubicacion
                {
                    UbicacionID = Guid.NewGuid(),
                    CalleID = Guid.Empty,
                    ColoniaID = Guid.Empty,
                    NumExterior = numeroDomicilio,
                    Descripcion = pTemp.Direccion
                };

                db.Ubicaciones.Add(ubicacion);

                persona.UbicacionViveID = ubicacion.UbicacionID;

                // Teléfono

                persona.Telefono = pTemp.Telefono;

                // Sector

                int.TryParse(pTemp.Sector, out int sector);

                switch (sector)
                {
                    case 1:
                        persona.SectorBrigadaID = new Guid("818053ea-69f7-435d-b36d-49cce0d7e48e");
                        break;
                    case 2:
                        persona.SectorBrigadaID = new Guid("14872ecc-380d-4e75-b040-7ce95e15b5f6");
                        break;
                    case 3:
                        persona.SectorBrigadaID = new Guid("8a841d8a-e6c0-45e0-90ea-ecd4e233cb0c");
                        break;
                    case 4:
                        persona.SectorBrigadaID = new Guid("986beffd-a439-4c00-bd59-cd183f4d84f8");
                        break;
                    case 5:
                        persona.SectorBrigadaID = new Guid("e1f4a7fa-e55f-4764-842c-ba6a6da0d1f9");
                        break;
                    case 6:
                        persona.SectorBrigadaID = new Guid("7a622988-575b-4c41-9397-f37791b0dc11");
                        break;
                    case 7:
                        persona.SectorBrigadaID = new Guid("e846a22b-9233-469a-956d-a4c90e9c283c");
                        break;
                    case 8:
                        persona.SectorBrigadaID = new Guid("bce38890-103e-468b-83ed-0078d9fd36e6");
                        break;
                    case 9:
                        persona.SectorBrigadaID = new Guid("a09e4181-8206-4d19-9ffe-7ce728ba743d");
                        break;
                    case 10:
                        persona.SectorBrigadaID = new Guid("447b5a15-39c5-4085-85c4-9c8fd9708899");
                        break;
                }

                persona.FechaAlta = DateTime.Now;
                persona.FechaActualizacion = DateTime.Now;
                persona.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;

                db.Personas.Add(persona);
            }

            db.SaveChanges();

            TempData["MessageBox"] = "Cambios realizados con exito.";

            return RedirectToAction("index");
        } // MigrarPersonas

        public ActionResult MigrarPersonasINE() {
            string logExcepciones = string.Empty;
            int totalRegistrosSatisfactorios = 0;
            int totalRegistrosSinCasilla = 0;
            int totalOtrasExcepciones = 0;
            
            foreach (var item in db.PersonasIne.Where(p => p.Registrado == BoolTipo.Ninguno))
            {
                Persona persona = db.Personas.Find(item.PersonaIneID);
                string logError = string.Empty;
                bool esValido = true;

                if (persona != null)
                {
                    var casillaTipo = CasillaTipo.Ninguno;                    

                    switch (item.Casilla.ToUpper())
                    {
                        case "X":
                            persona.Verificada = BoolTipo.No;
                            persona.VotanteSeguro = BoolTipo.No;
                            persona.FechaActualizacion = DateTime.Now;
                            persona.UserNameActualizacion = "primer auditoria (09/06/2018)";
                            db.Entry(persona).State = EntityState.Modified;                            
                            break;
                        case "B":
                            casillaTipo = CasillaTipo.Basica;
                            break;
                        case "C1":
                            casillaTipo = CasillaTipo.Contigua;
                            break;
                        case "C2":
                            casillaTipo = CasillaTipo.ContiguaII;
                            break;
                        case "C3":
                            casillaTipo = CasillaTipo.ContiguaIII;
                            break;
                        case "C4":
                            casillaTipo = CasillaTipo.ContiguaIV;
                            break;
                        case "C5":
                            casillaTipo = CasillaTipo.ContiguaV;
                            break;
                        case "C6":
                            casillaTipo = CasillaTipo.ContiguaVI;
                            break;
                        case "C7":
                            casillaTipo = CasillaTipo.ContiguaVII;
                            break;
                        case "C8":
                            casillaTipo = CasillaTipo.ContiguaVIII;
                            break;
                        case "C9":
                            casillaTipo = CasillaTipo.ContiguaIX;
                            break;
                        case "C10":
                            casillaTipo = CasillaTipo.ContiguaX;
                            break;
                    }

                    if (item.Casilla.ToUpper() != "X")
                    {
                        persona.Nombres = item.Nombres;
                        persona.ApellidoPaterno = item.PrimerApellido;
                        persona.ApellidoMaterno = item.SegundoApellido;

                        // Verifica y asigna la sección si es que la cambiaron o la persona no la tiene
                        if (item.SeccionNumero != null && item.SeccionNumero > 0)
                        {
                            if (persona.Seccion != null)
                            {
                                if (persona.Seccion.Numero != item.SeccionNumero)
                                {
                                    Seccion seccion = db.Secciones.Where(s => s.Numero == item.SeccionNumero).FirstOrDefault();

                                    if (seccion != null)
                                    {
                                        persona.SeccionID = seccion.SeccionID;
                                        persona.Seccion = seccion;
                                    }
                                    else
                                    {
                                        logError = string.Format("No se encontro la sección a asignar la persona {0} {1} {2}, sección: {3}, casilla {4}; [{5}]<br />", item.Nombres, item.PrimerApellido, item.SegundoApellido, item.SeccionNumero, item.Casilla, item.PersonaIneID);
                                        esValido = false;
                                    }
                                }
                            }
                            else
                            {
                                Seccion seccion = db.Secciones.Where(s => s.Numero == item.SeccionNumero).FirstOrDefault();

                                if (seccion != null)
                                {
                                    persona.SeccionID = seccion.SeccionID;
                                    persona.Seccion = seccion;
                                }
                                else
                                {
                                    logError = string.Format("No se encontro la sección a asignar la persona {0} {1} {2}, sección: {3}, casilla {4}; [{5}]<br />", item.Nombres, item.PrimerApellido, item.SegundoApellido, item.SeccionNumero, item.Casilla, item.PersonaIneID);
                                    esValido = false;
                                }
                            }
                        }
                        else
                        {
                            logError = string.Format("No trae número de sección la persona {0} {1} {2}, sección: {3}, casilla {4}; [{5}]<br />", item.Nombres, item.PrimerApellido, item.SegundoApellido, item.SeccionNumero, item.Casilla, item.PersonaIneID);
                            esValido = false;
                        }

                        if (esValido)
                        {
                            Casilla casilla = db.Casillas
                                .Where(c => c.SeccionID == persona.SeccionID && c.Tipo == casillaTipo).FirstOrDefault();

                            if (casilla != null)
                            {
                                // Agregando el votante

                                if (persona.Voto == null)
                                {
                                    Voto voto = new Voto
                                    {
                                        VotoID = Guid.NewGuid(),
                                        CasillaID = casilla.CasillaID,
                                        Persona = persona,
                                        NumeroINE = item.NumeroINE ?? 0,
                                        YaVoto = BoolTipo.Si
                                    };

                                    casilla.Votantes.Add(voto);
                                    persona.Voto = voto;
                                    db.Entry(persona).State = EntityState.Modified;
                                }
                                else
                                {
                                    persona.Voto.NumeroINE = item.NumeroINE ?? 0;
                                    persona.Voto.YaVoto = BoolTipo.Si;
                                    persona.Voto.CasillaID = casilla.CasillaID;
                                    db.Entry(persona.Voto).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                logError = string.Format("No se encontro la casilla a asignar la persona {0} {1} {2}, sección: {3}, casilla {4}; [{5}]<br />", item.Nombres, item.PrimerApellido, item.SegundoApellido, item.SeccionNumero, item.Casilla, item.PersonaIneID);
                                esValido = false;
                            }
                        } // esValido

                    } // if Casilla != X
                    else
                    {
                        logError = string.Format("No se encontro la persona {0} {1} {2}, en la sección: {3} de los libros del INE<br />", item.Nombres, item.PrimerApellido, item.SegundoApellido, item.SeccionNumero, item.Casilla, item.PersonaIneID);
                        esValido = false;
                    }
                }
                else
                {
                    logError = string.Format("No encontro la persona {0} {1} {2}, sección: {3}, casilla {4}; [{5}] en la base de datos<br />", item.Nombres, item.PrimerApellido, item.SegundoApellido, item.SeccionNumero, item.Casilla, item.PersonaIneID);
                    esValido = false;
                }

                if (esValido)
                {
                    totalRegistrosSatisfactorios++;
                }
                else if (item.Casilla.ToUpper() == "X")
                {
                    totalOtrasExcepciones++;
                }
                else
                {
                    totalRegistrosSinCasilla++;
                }

                logExcepciones += logError;

                item.Registrado = esValido ? BoolTipo.Si : BoolTipo.No;
                item.LogError = string.IsNullOrEmpty(logError) ? "Persona registrada satisfatoriamente." : logError;
                db.Entry(item).State = EntityState.Modified;
            } // foreach 

            logExcepciones += "<hr />";
            logExcepciones += "Registros satisfactorios: " + totalRegistrosSatisfactorios + "<br />";
            logExcepciones += "Registros sin casilla: " + totalRegistrosSinCasilla + "<br />";
            logExcepciones += "Registros no registrados: " + totalOtrasExcepciones + "<br />";

            db.SaveChanges();

            return Content(logExcepciones);
        } // MigrarPersonasINE
    }
}
