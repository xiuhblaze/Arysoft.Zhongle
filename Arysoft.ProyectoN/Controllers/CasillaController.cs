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
using Microsoft.AspNet.Identity;

namespace Arysoft.ProyectoN.Controllers
{
    [Authorize]
    public class CasillaController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Casilla
        [Authorize(Roles = "Admin, Editor, Consultant")]
        public async Task<ActionResult> Index(string buscar, string filtro, string orden, string SeccionID, string tipo, string status, int? pagina)
        {
            Guid seccionID = (SeccionID != null && SeccionID != Guid.Empty.ToString()) ? new Guid(SeccionID) : Guid.Empty;

            ViewBag.Orden = orden;
            ViewBag.OrdenSeccion = string.IsNullOrEmpty(orden) ? "seccion_desc" : "";
            ViewBag.OrdenTipo = orden == "tipo" ? "tipo_desc" : "tipo";
            ViewBag.OrdenVotantes = orden == "votantes" ? "votantes_desc" : "votantes";
            ViewBag.OrdenVotoSeguro = orden == "votoseguro" ? "votoseguro_desc" : "votoseguro";
            ViewBag.OrdenYaVotaron = orden == "yavotaron" ? "yavotaron_desc" : "yavotaron";
            ViewBag.OrdenVotantesRestantes = orden == "votantesrestantes" ? "votantesrestantes_desc" : "votantesrestantes";

            if (buscar != null)
            {
                pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            var casillas = db.Casillas                
                .Include(c => c.PersonaResponsable)
                .Include(c => c.Seccion)
                .Include(c => c.Notas)
                .Include(c => c.Ubicacion)
                .Include(c => c.Ubicacion.Calle)
                .Include(c => c.Ubicacion.Colonia)
                .Include(c => c.Votantes)
                .Include(c => c.Votantes.Select(v => v.Persona))
                //.Include(c => c.Votantes) // https://stackoverflow.com/questions/10822656/entity-framework-include-multiple-levels-of-properties
                .Include(c => c.Resultados)
                .Where(c => c.Status != StatusTipo.Ninguno);

            if (int.TryParse(buscar, out int seccion)) { buscar = string.Empty; }

            if (string.IsNullOrEmpty(status))
            {
                status = StatusTipo.Activo.ToString();
            }

            //List<Casilla> casillasList = await casillas.ToListAsync();

            if (!string.IsNullOrEmpty(buscar)
                || seccionID != Guid.Empty
                || !string.IsNullOrEmpty(tipo)
                || seccion > 0
                || !string.IsNullOrEmpty(status))
            {
                bool hayPalabras = !string.IsNullOrEmpty(buscar);

                tipo = string.IsNullOrEmpty(tipo) ? CasillaTipo.Ninguno.ToString() : tipo;
                status = string.IsNullOrEmpty(status) ? StatusTipo.Ninguno.ToString() : status;

                casillas = casillas.Where(c => 
                    (
                        !hayPalabras
                        || c.PersonaDuenno.Contains(buscar)
                        || (c.PersonaResponsable == null ? false : c.PersonaResponsable.Nombres.Contains(buscar))
                        || (c.PersonaResponsable == null ? false : c.PersonaResponsable.ApellidoPaterno.Contains(buscar))
                        || (c.PersonaResponsable == null ? false : c.PersonaResponsable.ApellidoMaterno.Contains(buscar))
                        || (
                            c.Representantes.Where(r => r.Nombres.Contains(buscar)).Count() > 0
                            || c.Representantes.Where(r => r.ApellidoPaterno.Contains(buscar)).Count() > 0
                            || c.Representantes.Where(r => r.ApellidoMaterno.Contains(buscar)).Count() > 0
                           )
                        //|| (
                        //       c.Ubicacion == null ? false :
                        //       (
                        //           (c.Ubicacion.Calle == null) ? false :
                        //           c.Ubicacion.Calle.Nombre.Contains(buscar)
                        //       )
                        //   )
                        //|| (c.Ubicacion == null ? false :
                        //    (c.Ubicacion.ColoniaID == Guid.Empty || c.Ubicacion.Colonia == null) ? false :
                        //        c.Ubicacion.Colonia.Nombre.Contains(buscar))
                    )
                    && (seccion == 0 ? true : c.SeccionID == Guid.Empty ? true : c.Seccion.Numero == seccion)
                    && (tipo == CasillaTipo.Ninguno.ToString() ? true : c.Tipo.ToString() == tipo)
                    && (seccionID == Guid.Empty ? true : c.SeccionID == seccionID)
                    && (status == StatusTipo.Ninguno.ToString() ? true: c.Status.ToString() == status)
                );
            }

            //List<Casilla> casillasList = casillas.ToList();
            //casillasList = casillas.ToList();
            db.Database.CommandTimeout = 180;
            List<Casilla> casillasList = await casillas.ToListAsync();

            switch (orden)
            {
                case "seccion_desc":
                    casillasList = casillasList.OrderByDescending(c => c.Seccion.Numero).ThenBy(c => c.Tipo).ToList();
                    break;
                case "tipo":
                    casillasList = casillasList.OrderBy(c => c.Tipo).ToList();
                    break;
                case "tipo_desc":
                    casillasList = casillasList.OrderByDescending(c => c.Tipo).ToList();
                    break;
                case "votantes":
                    casillasList = casillasList.OrderBy(c => c.NumeroVotantes).ToList();
                    break;
                case "votantes_desc":
                    casillasList = casillasList.OrderByDescending(c => c.NumeroVotantes).ToList();
                    break;
                case "votoseguro":
                    casillasList = casillasList.OrderBy(c => c.VotosSeguros).ToList();
                    break;
                case "votoseguro_desc":
                    casillasList = casillasList.OrderByDescending(c => c.VotosSeguros).ToList();
                    break;
                case "yavotaron":
                    casillasList = casillasList.OrderBy(c => c.YaVotaron).ToList();
                    break;
                case "yavotaron_desc":
                    casillasList = casillasList.OrderByDescending(c => c.YaVotaron).ToList();
                    break;
                case "votantesrestantes":
                    casillasList = casillasList.OrderBy(c => c.VotantesRestantes).ToList();
                    break;
                case "votantesrestantes_desc":
                    casillasList = casillasList.OrderByDescending(c => c.VotantesRestantes).ToList();
                    break;
                default:
                    casillasList = casillasList.OrderBy(c => c.Seccion.Numero).ThenBy(c => c.Tipo).ToList();
                    break;
            }

            ViewBag.Count = casillasList.Count;
            ViewBag.Tipo = tipo;
            ViewBag.SeccionID = await ObtenerListaSeccionesAsync(seccionID);
            ViewBag.Status = status;

            ViewBag.SeccionIDSelected = seccionID;

            int numeroPagina = pagina ?? 1;
            int elementosPagina = 50;

            return View(casillasList.ToPagedList(numeroPagina, elementosPagina));
        }

        // GET: Casilla/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("notid"); }
                return RedirectToAction("Index");
            }
            Casilla casilla = await db.Casillas
                .Include(c => c.Seccion)
                .Include(c => c.Ubicacion)
                .Include(c => c.Ubicacion.Calle)
                .Include(c => c.Ubicacion.Colonia)
                .Include(c => c.PersonaResponsable)
                .Include(c => c.Votantes)
                .Include(c => c.Votantes.Select(v => v.Persona))
                .Include(c => c.Votantes.Select(v => v.Persona.Notas))
                .Include(c => c.Representantes)
                .Include(c => c.Notas)
                .FirstOrDefaultAsync(c => c.CasillaID == id);
            if (casilla == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
            }

            casilla.SoloLectura = true;

            if (Request.IsAjaxRequest())
            {
                casilla.NmOrigen = "details";
                CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, true);
                return PartialView("_details", casillaEM);
            }

            return View(casilla);
        } // GET: Details

        // GET: Casilla/Create
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(ControllerContext.HttpContext.User.Identity.Name);

            Casilla casilla = new Casilla
            {
                CasillaID = Guid.NewGuid(),
                SeccionID = Guid.Empty,
                UbicacionID = Guid.Empty,
                //PersonaResponsableID = Guid.Empty,
                Tipo = CasillaTipo.Ninguno,
                PersonaDuenno = string.Empty,
                NumeroVotantes = 0,
                Status = StatusTipo.Ninguno,
                FechaActualizacion = DateTime.Now,
                UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name
            };

            db.Casillas.Add(casilla);

            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = casilla.CasillaID });
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }

        } // GET: Create

        // GET: Casilla/Edit/5
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            Guid CalleID = Guid.Empty;
            Guid ColoniaID = Guid.Empty;

            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
            }
            Casilla casilla = await db.Casillas
                .Include(c => c.Seccion)
                .Include(c => c.Ubicacion)
                .Include(c => c.PersonaResponsable)
                .Include(c => c.Votantes)
                .Include(c => c.Votantes.Select(v => v.Persona))
                .Include(c => c.Votantes.Select(v => v.Persona.Notas))
                .Include(c => c.Representantes)
                .Include(c => c.Resultados)
                .Include(c => c.Notas)
                .FirstOrDefaultAsync(c => c.CasillaID == id);
            if (casilla == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
            }

            if (casilla.Ubicacion != null)
            {
                CalleID = casilla.Ubicacion.CalleID;
                ColoniaID = casilla.Ubicacion.ColoniaID;
            }
                        
            ViewBag.PersonaResponsableID = await ObtenerListaPromotoresAsync(casilla.PersonaResponsableID ?? Guid.Empty); 
            ViewBag.SeccionID = await ObtenerListaSeccionesAsync(casilla.SeccionID ?? Guid.Empty); 
            ViewBag.CalleID = await ObtenerListaCallesAsync(CalleID); 
            ViewBag.ColoniaID = await ObtenerListaColoniasAsync(ColoniaID);

            CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            return View(casillaEM);
        } // GET: Edit

        // POST: Casilla/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Edit([Bind(Include = "CasillaID,SeccionID,PersonaResponsableID,Tipo,PersonaDuenno,NumeroVotantes,Status,UserNameActualizacion,FechaActualizacion,CalleID,NumExterior,Letra,NumInterior,ColoniaID,Descripcion")] CasillaEditModel casilla)
        {
            bool esNuevo = casilla.Status == StatusTipo.Ninguno;
            
            if (esNuevo)
            {
                casilla.Status = StatusTipo.Activo;
            }

            Guid UbicacionID = await BuscarUbicacionAsync(casilla.CalleID, casilla.ColoniaID, casilla.NumExterior, casilla.Letra, casilla.NumInterior);

            if (ModelState.IsValid)
            {
                Casilla c = Comun.ObtenerCasilla(casilla, false);

                c.UbicacionID = UbicacionID == Guid.Empty ? AgregarUbicacion(casilla) : UbicacionID;
                c.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                c.FechaActualizacion = DateTime.Now;

                db.Entry(c).State = EntityState.Modified;
                await db.SaveChangesAsync();

                if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                else { TempData["MessageBox"] = "Cambios guardados con exito."; }

                return RedirectToAction("Index");
            }

            // Volver a la vista
            
            ViewBag.PersonaResponsableID = await ObtenerListaPromotoresAsync(casilla.PersonaResponsableID ?? Guid.Empty);
            ViewBag.SeccionID = await ObtenerListaSeccionesAsync(casilla.SeccionID ?? Guid.Empty);
            ViewBag.CalleID = await ObtenerListaCallesAsync(casilla.CalleID);
            ViewBag.ColoniaID = await ObtenerListaColoniasAsync(casilla.ColoniaID);

            return View(casilla);
        } // POST: Edit

        // GET: Casilla/Delete/5
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
            Casilla casilla = await db.Casillas
                .Include(c => c.Seccion)
                .Include(c => c.Ubicacion)
                .Include(c => c.Ubicacion.Calle)
                .Include(c => c.Ubicacion.Colonia)
                .Include(c => c.PersonaResponsable)
                .Include(c => c.Votantes)
                .Include(c => c.Votantes.Select(v => v.Persona))
                .Include(c => c.Votantes.Select(v => v.Persona.Notas))
                .Include(c => c.Representantes)
                .Include(c => c.Notas)
                .FirstOrDefaultAsync(c => c.CasillaID == id);
            if (casilla == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            casilla.SoloLectura = true;
            if (Request.IsAjaxRequest())
            {
                casilla.NmOrigen = "delete";
                CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, true);
                return PartialView("_details", casillaEM);
            }
            return View(casilla);
        } // Delete

        // POST: Casilla/Delete/5
        [Authorize(Roles = "Admin, Editor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Casilla casilla = await db.Casillas
                .Include(c => c.Seccion)
                .FirstOrDefaultAsync(c => c.CasillaID == id);

            if (casilla.Status == StatusTipo.Activo)
            {
                casilla.Status = StatusTipo.Baja;
                casilla.UserNameActualizacion = User.Identity.Name;
                casilla.FechaActualizacion = DateTime.Now;
                db.Entry(casilla).State = EntityState.Modified;
                //TempData["MessageBox"] = "Casilla " + casilla.Seccion.Numero + " " + casilla.Tipo.GetDisplayName() + " ha sido dado de baja.";
            }
            else if (casilla.Status == StatusTipo.Baja)
            {
                casilla.Status = StatusTipo.Eliminado;
                casilla.UserNameActualizacion = User.Identity.Name;
                casilla.FechaActualizacion = DateTime.Now;
                db.Entry(casilla).State = EntityState.Modified;
                //TempData["MessageBox"] = "Casilla " + casilla.Seccion.Numero + " " + casilla.Tipo.GetDisplayName() + " ha sido eliminada.";
            }
            else if (casilla.Status == StatusTipo.Eliminado)
            {
                //HACK: Validar que no tenga asociaciones
                //TempData["MessageBox"] = "Casilla " + casilla.Seccion.Numero + " " + casilla.Tipo.GetDisplayName() + " ha sido eliminada definitivamente, esta acción no se puede deshacer.";
                db.Casillas.Remove(casilla);
            }
            
            db.SaveChanges();
            return RedirectToAction("Index");
        } // DeleteConfirmed

        // GET: Casilla/Activar/5
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Activar(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Casilla casilla = await db.Casillas.FindAsync(id);
            Casilla casilla = await db.Casillas
                .Include(c => c.Seccion)
                .FirstOrDefaultAsync(c => c.CasillaID == id);
            if (casilla == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
                //return HttpNotFound();
            }

            casilla.Status = StatusTipo.Activo;
            casilla.UserNameActualizacion = User.Identity.Name;
            casilla.FechaActualizacion = DateTime.Now;
            db.Entry(casilla).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "A ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }

            await AgregarNotaAsync(casilla.CasillaID, "[REACTIVADA] La casilla estaba dada de baja y fué reactivada satisfactoriamente", ControllerContext.HttpContext.User.Identity.Name, PropietarioTipo.Casilla);

            TempData["MessageBox"] = casilla.Seccion.Numero + " " + casilla.Tipo.GetDisplayName() + " a sido reactivada satisfactoriamente.";
            return RedirectToAction("Index");

        } // GET: Activar

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // METODOS AJAX
        ////////////////////////////////////////////////////////////////////////////////

        // Dashboard

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

        // Votantes

        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> AgregarVotantes(Guid id)
        {
            Casilla casilla = db.Casillas.Find(id);

            ViewBag.CasillaID = id;
            ViewBag.SectorID = await ObtenerListaSectoresAsync(Guid.Empty);
            // ViewBag.SeccionID = ObtenerListaSecciones(Guid.Empty);
            ViewBag.CalleID = await ObtenerListaCallesAsync(Guid.Empty);
            ViewBag.ColoniaID = await ObtenerListaColoniasAsync(Guid.Empty);
            
            ViewBag.Casilla = casilla;

            return PartialView("_votantesAgregar");
        } // GET: AgregarVotantes

        [Authorize(Roles = "Admin, Editor")]
        [HttpPost]
        public async Task<ActionResult> AgregarVotantes(Guid id, string votantes)
        {
            Casilla casilla = await db.Casillas
                .Include(c => c.Votantes)
                .FirstOrDefaultAsync(c => c.CasillaID == id);

            char[] separador = new char[] { ',' };
            string[] identificadores = votantes.Split(separador, StringSplitOptions.RemoveEmptyEntries);

            if (identificadores.Length > 0)
            {
                foreach (string personaId in identificadores)
                {
                    Guid PersonaID = new Guid(personaId);
                    Persona persona = await db.Personas
                        .Include(p => p.Voto)
                        .FirstOrDefaultAsync(p => p.PersonaID == PersonaID);                    

                    if (persona.Voto == null)
                    {
                        Voto voto = new Voto
                        {
                            VotoID = Guid.NewGuid(),
                            CasillaID = casilla.CasillaID,
                            Persona = persona,
                            NumeroINE = 0,
                            YaVoto = BoolTipo.No
                        };

                        casilla.Votantes.Add(voto);

                        persona.Voto = voto;
                        db.Entry(persona).State = EntityState.Modified;
                    }
                    else { // Si existe, actualizar la casilla

                        if (persona.Voto.CasillaID != casilla.CasillaID) // Si cambió de casilla
                        {
                            persona.Voto.NumeroINE = 0; // Borrar número por si ya esta siendo utilizado en la otra casilla
                            persona.Voto.YaVoto = BoolTipo.No;
                        }
                        persona.Voto.CasillaID = casilla.CasillaID;                        
                        db.Entry(persona.Voto).State = EntityState.Modified;
                    }
                    
                } // foreach

                await db.SaveChangesAsync();
            }

            CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            return PartialView("_listaVotantes", casillaEM);
        } // POST: AgregarVotantes

        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> BuscarVotantes(string buscar, string orden, string afinidad, string SectorID, string SeccionID,
            string CalleID, string ColoniaID, string bardaLona, string sectorTipo, string votanteSeguro)
        {
            Guid sectorID = (SectorID != null && SectorID != Guid.Empty.ToString()) ? new Guid(SectorID) : Guid.Empty;
            Guid seccionID = (SeccionID != null && SeccionID != Guid.Empty.ToString()) ? new Guid(SeccionID) : Guid.Empty;
            Guid calleID = (CalleID != null && CalleID != Guid.Empty.ToString()) ? new Guid(CalleID) : Guid.Empty;
            Guid coloniaID = (ColoniaID != null && ColoniaID != Guid.Empty.ToString()) ? new Guid(ColoniaID) : Guid.Empty;

            ViewBag.Orden = orden;
            ViewBag.OrdenNombre = string.IsNullOrEmpty(orden) ? "nombre_desc" : "";
            ViewBag.OrdenBrigada = orden == "brigada" ? "brigada_desc" : "brigada";
            ViewBag.OrdenSector = orden == "sector" ? "sector_desc" : "sector";
            ViewBag.OrdenSeccion = orden == "seccion" ? "seccion_desc" : "seccion";

            var personas = db.Personas
                .Include(s => s.Seccion)
                .Include(s => s.Sector)
                .Where(p => p.Status == StatusTipo.Activo);
            //var personasList = new List<Persona>();

            if (!string.IsNullOrEmpty(buscar)
                || sectorID != Guid.Empty //!string.IsNullOrEmpty(SectorID)
                || seccionID != Guid.Empty //!string.IsNullOrEmpty(SeccionID)
                || calleID != Guid.Empty
                || coloniaID != Guid.Empty
                || !string.IsNullOrEmpty(afinidad)
                || (!string.IsNullOrEmpty(bardaLona) && bardaLona != "0")
                || !string.IsNullOrEmpty(votanteSeguro))
            {
                bool hayPalabras = !string.IsNullOrEmpty(buscar);

                afinidad = string.IsNullOrEmpty(afinidad) ? AfinidadTipo.Ninguno.ToString() : afinidad;
                votanteSeguro = string.IsNullOrEmpty(votanteSeguro) ? BoolTipo.Ninguno.ToString() : votanteSeguro;
                bardaLona = string.IsNullOrEmpty(bardaLona) ? "0" : bardaLona;

                personas = personas.Where(p =>
                    (
                        !hayPalabras
                        || p.Nombres.Contains(buscar)
                        || p.ApellidoPaterno.Contains(buscar)
                        || p.ApellidoMaterno.Contains(buscar)
                        || p.CorreoElectronico.Contains(buscar)
                    )
                    && (afinidad == AfinidadTipo.Ninguno.ToString() ? true : p.Afinidad.ToString() == afinidad)
                    && (sectorID == Guid.Empty ? true :
                            sectorTipo == "0" ? (p.SectorBrigadaID == sectorID || p.Seccion.SectorID == sectorID) :
                            sectorTipo == "1" ? p.SectorBrigadaID == sectorID :
                            sectorTipo == "2" ? p.Seccion.SectorID == sectorID : false)
                    && (seccionID == Guid.Empty ? true : p.SeccionID == seccionID)
                    && (calleID == Guid.Empty ? true : p.UbicacionVive.CalleID == calleID || p.UbicacionVota.CalleID == calleID)
                    && (coloniaID == Guid.Empty ? true : p.UbicacionVive.ColoniaID == coloniaID || p.UbicacionVota.ColoniaID == coloniaID)
                    && (bardaLona == "0" ? true :
                            bardaLona == "1" ? p.TieneBarda == BoolTipo.Si :
                            bardaLona == "2" ? p.TieneLona == BoolTipo.Si :
                            bardaLona == "3" ? (p.TieneBarda == BoolTipo.Si || p.TieneLona == BoolTipo.Si) :
                            bardaLona == "4" ? (p.TieneBarda == BoolTipo.Si && p.TieneLona == BoolTipo.Si) : false)
                    && (votanteSeguro == BoolTipo.Ninguno.ToString() ? true : p.VotanteSeguro.ToString() == votanteSeguro)
                );
            }

            switch (orden)
            {
                case "nombre_desc":
                    personas = personas.OrderByDescending(p => p.ApellidoPaterno).ThenByDescending(p => p.ApellidoMaterno);
                    break;
                case "sector":
                    personas = personas.OrderBy(p => p.Seccion == null).ThenBy(p => p.Seccion.Sector == null).ThenBy(p => p.Seccion.Sector.Nombre);
                    break;
                case "sector_desc":
                    personas = personas.OrderByDescending(p => p.Seccion == null).ThenByDescending(p => p.Seccion.Sector.Nombre);
                    break;
                case "seccion":
                    personas = personas.OrderBy(p => p.Seccion == null).ThenBy(p => p.Seccion.Numero).ThenBy(p => p.ApellidoPaterno);
                    break;
                case "seccion_desc  ":
                    personas = personas.OrderByDescending(p => p.Seccion == null).ThenByDescending(p => p.Seccion.Numero).ThenBy(p => p.ApellidoPaterno);
                    break;
                case "brigada":
                    personas = personas.OrderBy(p => p.Sector == null).ThenBy(p => p.Sector.Nombre).ThenBy(p => p.ApellidoPaterno);
                    break;
                case "brigada_desc":
                    personas = personas.OrderByDescending(p => p.Sector == null).ThenByDescending(p => p.Sector.Nombre).ThenBy(p => p.ApellidoPaterno);
                    break;
                default:
                    personas = personas.OrderBy(p => p.ApellidoPaterno).ThenBy(p => p .ApellidoMaterno);
                    break;
            }

            return PartialView("_listaVotantesAgregarCasilla", await personas.ToListAsync());
        } // BuscarVotantes

        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> RemoveVotante(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return Content("notid");
            }

            Persona persona = await db.Personas
                .Include(p => p.Voto)
                .FirstOrDefaultAsync(p => p.PersonaID == id);

            if (persona == null)
            {
                return Content("notfound");
            }

            Guid casillaID = Guid.Empty;

            if (persona.Voto != null)
            {
                casillaID = persona.Voto.CasillaID;
                db.Votos.Remove(persona.Voto);
                persona.Voto = null;
                db.Entry(persona).State = EntityState.Modified;

                await db.SaveChangesAsync();
            }

            if (casillaID == Guid.Empty)
            {
                return Content("nocasilla");
            }

            Casilla casilla = await db.Casillas
                .Include(c => c.Seccion)
                .Include(c => c.Ubicacion)
                .Include(c => c.PersonaResponsable)
                .Include(c => c.Votantes)
                .Include(c => c.Representantes)
                .Include(c => c.Notas)
                .FirstOrDefaultAsync(c => c.CasillaID == casillaID);
            CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            return PartialView("_listaVotantes", casillaEM);
        } // RemoveVotante

        [HttpPost]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> EditINE(Guid PersonaID, int NumeroINE) {

            if(PersonaID == null || PersonaID == Guid.Empty)
            {
                return Content("notid");
            }

            Persona persona = await db.Personas
                .Include(p => p.Voto)
                .FirstOrDefaultAsync(p => p.PersonaID == PersonaID);

            if (persona == null)
            {
                return Content("notfound");
            }

            if (persona.Voto != null)
            {
                bool existeNumeroINE = await (from v in db.Votos
                                        where v.CasillaID == persona.Voto.CasillaID
                                         && v.NumeroINE == NumeroINE
                                         && v.Persona.PersonaID != persona.PersonaID
                                        select v).CountAsync() > 0;

                if (existeNumeroINE) { return Content("numINEexist"); }

                persona.Voto.NumeroINE = NumeroINE;
                db.Entry(persona.Voto).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            return Content("success");
        } // EditINE

        [HttpPost]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> EditVoto(Guid PersonaID, BoolTipo YaVoto, VotoHoraTipo Hora)
        {
            if (PersonaID == null || PersonaID == Guid.Empty)
            {
                return Content("notid");
            }

            Persona persona = await db.Personas
                .Include(p => p.Voto)
                .FirstOrDefaultAsync(p => p.PersonaID == PersonaID);

            if (persona == null)
            {
                return Content("notfound");
            }

            if (persona.Voto != null)
            {
                persona.Voto.YaVoto = YaVoto;
                persona.Voto.VotoHora = YaVoto == BoolTipo.Si ? Hora : VotoHoraTipo.Ninguno;
                db.Entry(persona.Voto).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            return Content("success");
        } // EditVoto
        
        // NOTAS

        public async Task<ActionResult> AgregarNota(Guid id, string texto, PropietarioTipo tipo)
        {
            Casilla casilla = await db.Casillas.FindAsync(id);

            if (casilla == null)
            {
                return Json(new { message = "notfound" }, JsonRequestBehavior.AllowGet);
            }

            if (await AgregarNotaAsync(casilla.CasillaID, texto, ControllerContext.HttpContext.User.Identity.Name, PropietarioTipo.Persona))
            {
                var listado = await (from n in db.Notas where n.PropietarioID == id select n).ToListAsync();
                return PartialView("_listaNotas", listado);
            }
            else
            {
                return Json(new { message = "notadd" }, JsonRequestBehavior.AllowGet);
            }

        } // AgregarNota

        // REPRESENTANTES DE CASILLA

        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> ComprobarRepresentante(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return Content("notid");
            }

            Persona persona = await db.Personas
                .Include(p => p.Casilla)
                .Include(p => p.Casilla.Seccion)
                .FirstOrDefaultAsync(p => p.PersonaID == id); // HACK: A ver si funciona esto :/

            if (persona == null)
            {
                return Content("notfound");
            }

            if (persona.CasillaID == null)
            {
                return Content("available");
            }

            return Json(new {
                Seccion = persona.Casilla.Seccion.Numero,
                CasillaTipo = persona.Casilla.Tipo.GetDisplayName(),
                RepresentanteTipo = persona.RepresentanteTipo.GetDisplayName()
            }, JsonRequestBehavior.AllowGet);
        } // ComprobarRepresentante

        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> EditRepresentante(Guid id, Guid? CasillaID)
        {
            Persona persona = null;
            Casilla casilla = null;

            if (id == Guid.Empty)
            {
                persona = new Persona();

                if (CasillaID == null)
                {
                    return Content("notcasilla");
                }

                casilla = await db.Casillas.FindAsync(CasillaID ?? Guid.Empty);

                if (casilla == null)
                {
                    return Content("casillanotfound");
                }

                persona.CasillaID = casilla.CasillaID;
                persona.Casilla = casilla;
            }
            else {
                persona = await db.Personas
                    .Include(p => p.Casilla)
                    .Include(p => p.Casilla.Seccion)
                    .FirstOrDefaultAsync(p => p.PersonaID == id);

                if (persona == null)
                {
                    return Content("notfound");
                }
            }

            ViewBag.RepresentanteID = await ObtenerListaPersonasAsync();
            
            return PartialView("_editRepresentante", persona);
        } // EditRepresentante

        [Authorize(Roles = "Admin, Editor")]
        [HttpPost]
        public async Task<ActionResult> EditRepresentante(Guid id, Guid CasillaID, RepresentanteTipo tipo, BoolTipo Capacitacion, BoolTipo Asistencia)
        {
            if (id == Guid.Empty)
            {
                return Content("notid");
            }

            Persona persona = await db.Personas.FindAsync(id);
            
            if (persona == null)
            {
                return Content("personanotfound");
            }

            persona.CasillaID = CasillaID;
            persona.RepresentanteTipo = tipo;
            persona.RepresentanteCapacitacion = Capacitacion;
            persona.RepresentanteAsistencia = Asistencia;

            db.Entry(persona).State = EntityState.Modified;
            await db.SaveChangesAsync();

            //Casilla casilla = await db.Casillas.FindAsync(CasillaID);

            //CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            Casilla casilla = await db.Casillas                
                .Include(c => c.Ubicacion)
                .Include(c => c.Representantes)
                .Include(c => c.Representantes.Select(r => r.Promotor))
                .Include(c => c.Representantes.Select(r => r.PersonasAfines))
                .Include(c => c.Representantes.Select(r => r.UbicacionVive))
                .Include(c => c.Representantes.Select(r => r.Notas))
                .Include(c => c.Notas)
                .FirstOrDefaultAsync(c => c.CasillaID == CasillaID);
            CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            return PartialView("_listaRepresentantes", casillaEM);

        } // EditRepresentante

        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> RemoveRepresentante(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return Content("notid");
            }
            
            Persona persona = await db.Personas.FindAsync(id);
            if (persona == null)
            {
                return Content("notfound");
            }

            Guid CasillaID = persona.CasillaID ?? Guid.Empty;

            persona.CasillaID = null;
            persona.RepresentanteTipo = RepresentanteTipo.Ninguno;
            db.Entry(persona).State = EntityState.Modified;
            await db.SaveChangesAsync();

            //Casilla casilla = db.Casillas.Find(CasillaID);
            //CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            Casilla casilla = await db.Casillas
                .Include(c => c.Seccion)
                .Include(c => c.Ubicacion)
                .Include(c => c.PersonaResponsable)
                .Include(c => c.Votantes)
                .Include(c => c.Representantes)
                .Include(c => c.Notas)
                .FirstOrDefaultAsync(c => c.CasillaID == CasillaID);
            CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            return PartialView("_listaRepresentantes", casillaEM);
        } // RemoveRepresentante

        // BINGO

        public async Task<ActionResult> ObtenerBingo(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return Content("notid");
            }

            Casilla casilla = await db.Casillas
                .Include(c => c.Seccion)
                .Include(c => c.Ubicacion)
                .Include(c => c.PersonaResponsable)
                .Include(c => c.Votantes)
                .Include(c => c.Representantes)
                .Include(c => c.Notas)
                .FirstOrDefaultAsync(c => c.CasillaID == id);

            if (casilla == null)
            {
                return Content("notfound");
            }

            CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            return PartialView("_listaBingo", casillaEM);
        } // ObtenerBingo
        
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> RegistrarVotos(Guid id, string votos, VotoHoraTipo hora)
        {
            if (id == null || id == Guid.Empty)
            {
                return Content("notid");
            }

            Casilla casilla = await db.Casillas.FindAsync(id);

            if (casilla == null)
            {
                return Content("notfound");
            }

            if (string.IsNullOrEmpty(votos))
            {
                return Content("novotes");
            }

            char[] delimitadores = new char[] { ',', ' ', '.' };
            string[] votosList = votos.Split(delimitadores, StringSplitOptions.RemoveEmptyEntries);
            string numerosNoRegistrados = string.Empty;

            if (votosList.Length > 0)
            {
                foreach (string voto in votosList)
                {
                    int.TryParse(voto, out int numeroINE);

                    if (numeroINE >= 1 && numeroINE <= casilla.NumeroVotantes)
                    {
                        // Agregar voto
                        // Verificar si existe voto en casilla
                        // Marcar el voto
                        // Si no existe, crear el objeto y agregarlo a la casilla
                        
                        Voto miVoto = await db.Votos
                            .Where(v => v.NumeroINE == numeroINE && v.CasillaID == casilla.CasillaID)
                            .FirstOrDefaultAsync();

                        if (miVoto == null)
                        {
                            miVoto = new Voto
                            {
                                VotoID = Guid.NewGuid(),
                                CasillaID = casilla.CasillaID,
                                NumeroINE = numeroINE,
                                YaVoto = BoolTipo.Si,
                                VotoHora = hora
                            };
                            db.Votos.Add(miVoto);
                        }
                        else
                        {
                            if (miVoto.YaVoto == BoolTipo.No)
                            {
                                miVoto.YaVoto = BoolTipo.Si;
                                miVoto.VotoHora = hora;
                                db.Entry(miVoto).State = EntityState.Modified;
                            }
                        }
                    }
                    else {
                        numerosNoRegistrados += ", " + voto;
                    }
                }

                await db.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(numerosNoRegistrados))
            {
                ViewBag.NumerosNoRegistrados = numerosNoRegistrados;
            }

            casilla = await db.Casillas
                .Include(c => c.Seccion)
                .Include(c => c.Ubicacion)
                .Include(c => c.PersonaResponsable)
                .Include(c => c.Votantes)
                .Include(c => c.Representantes)
                .Include(c => c.Notas)
                .FirstOrDefaultAsync(c => c.CasillaID == id);
            CasillaEditModel casillaEM = Comun.ObtenerCasillaEditModel(casilla, false);

            return PartialView("_listaBingo", casillaEM);
        }  // RegistrarVotos

        // RESULTADOS

        public async Task<ActionResult> ObtenerResultados(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return Content("notid");
            }

            Casilla casilla = await db.Casillas
                .Include(c => c.Resultados).FirstOrDefaultAsync(c => c.CasillaID == id);

            if (casilla == null)
            {
                return Content("notfound");
            }

            var Partidos = await db.Partidos.Where(p => p.Status == StatusTipo.Activo).ToListAsync();

            CasillaResultadosViewModel Resultados = new CasillaResultadosViewModel
            {
                Casilla = casilla,
                Partidos = Partidos
            };

            return PartialView("_listaPartidos", Resultados);
        } // ObtenerResultados

        [HttpPost]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> EditResultado(Guid PartidoID, Guid CasillaID, int Total)
        {
            if (PartidoID == null || PartidoID == Guid.Empty)
            {
                return Content("notpartidoid");
            }

            if (CasillaID == null || CasillaID == Guid.Empty)
            {
                return Content("notcasillaid");
            }

            Partido partido = await db.Partidos.FindAsync(PartidoID);

            if (partido == null) {
                return Content("partidonotfound");
            }

            Casilla casilla = await db.Casillas.FindAsync(CasillaID);

            if (casilla == null)
            {
                return Content("casillanotfound");
            }

            ResultadoCasilla resultado = await db.ResultadoCasillas
                .Where(r => r.CasillaID == CasillaID && r.PartidoID == PartidoID)
                .FirstOrDefaultAsync();

            if (resultado == null)
            {
                resultado = new ResultadoCasilla
                {
                    ResultadoCasillaID = Guid.NewGuid(),
                    PartidoID = PartidoID,
                    CasillaID = CasillaID,
                    Total = Total
                };

                db.ResultadoCasillas.Add(resultado);
            }
            else
            {
                resultado.Total = Total;
                db.Entry(resultado).State = EntityState.Modified;
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            return Content("success");
        } // EditResultado

        // BUSCAR

        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> BuscarCasillasXSeccion(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return Json(new { message = "notid" });
            }

            var casillas = await (from c in db.Casillas
                           where c.SeccionID == id && c.Status == StatusTipo.Activo
                           orderby c.Tipo
                           select new { Tipo = c.Tipo, ID = c.CasillaID }).ToListAsync();

            return Json(new {
                Casillas = casillas
            }, JsonRequestBehavior.AllowGet);
        } // BuscarCasillasXSeccion

        // METODOS PRIVADOS
        ////////////////////////////////////////////////////////////////////////////////

        // Ubicaciones

        private async Task<Guid> BuscarUbicacionAsync(Guid CalleID, Guid ColoniaID, int NumExterior, string Letra, string NumInterior)
        {
            Guid resultado = Guid.Empty;

            //Letra = Letra.Trim();
            //NumInterior = NumInterior.Trim();

            if (CalleID == Guid.Empty && ColoniaID == Guid.Empty
                && NumExterior == 0
                && string.IsNullOrEmpty(Letra)
                && string.IsNullOrEmpty(NumInterior))
            {
                return Guid.Empty;
            }

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

        private Guid AgregarUbicacion(CasillaEditModel casilla)
        {

            if (casilla.CalleID == Guid.Empty && casilla.ColoniaID == Guid.Empty
                && casilla.NumExterior == 0
                && string.IsNullOrEmpty(casilla.Letra)
                && string.IsNullOrEmpty(casilla.NumInterior))
            {
                return Guid.Empty;
            }

            Ubicacion u = new Ubicacion
            {
                UbicacionID = Guid.NewGuid(),
                CalleID = casilla.CalleID,
                ColoniaID = casilla.ColoniaID,
                NumExterior = casilla.NumExterior,
                Letra = casilla.Letra,
                NumInterior = casilla.NumInterior,
                Descripcion = casilla.Descripcion,
                Latitud = casilla.Latitud,
                Longitud = casilla.Longitud
            };

            db.Ubicaciones.Add(u);

            return u.UbicacionID;
        } // AgregarUbicacion

        // Notas

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

        // LISTADOS

        private List<SelectListItem> ObtenerListaPersonas()
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(seleccionar persona)", Value = Guid.Empty.ToString() });

            foreach (var item in db.Personas
                .Where(p => p.Status == StatusTipo.Activo)
                .OrderBy(p => p.Nombres).ThenBy(p => p.ApellidoPaterno))
            {
                listado.Add(new SelectListItem
                {
                    Text = item.NombreCompleto,
                    Value = item.PersonaID.ToString()
                });
            }

            return listado;
        } // ObtenerListaPromotores

        private List<SelectListItem> ObtenerListaPromotores(Guid selectedID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(promotor)", Value = Guid.Empty.ToString() });

            foreach (var item in db.Personas
                .Where(p => (p.Status == StatusTipo.Activo) && (p.Afinidad == AfinidadTipo.Movilizador || p.Afinidad == AfinidadTipo.Simpatizante))
                .OrderBy(p => p.Nombres).ThenBy(p => p.ApellidoPaterno))
            {
                listado.Add(new SelectListItem
                {
                    Text = item.NombreCompleto,
                    Value = item.PersonaID.ToString(),
                    Selected = (item.PersonaID == selectedID)
                });
            }

            return listado;
        } // ObtenerListaPromotores

        private List<SelectListItem> ObtenerListaSectores(Guid selectedID)
        {
            var listado = new SelectList(db.Sectores.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Nombre), "SectorID", "Nombre", selectedID).ToList();

            listado.Insert(0, new SelectListItem { Text = "(sector)", Value = Guid.Empty.ToString() });

            return listado;
        } // ObtenerListaSectores

        private List<SelectListItem> ObtenerListaSecciones(Guid selectedID)
        {
            var listado = new SelectList(db.Secciones.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Numero), "SeccionID", "Numero", selectedID).ToList();

            listado.Insert(0, new SelectListItem { Text = "(sección)", Value = Guid.Empty.ToString() });

            return listado;
        } // ObtenerListaSecciones

        private List<SelectListItem> ObtenerListaColonias(Guid selectedID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(colonia)", Value = Guid.Empty.ToString() });

            foreach (var item in db.Colonias
                .Where(p => p.Status == StatusTipo.Activo)
                .OrderBy(p => p.Nombre))
            {
                //string seccion = item.Seccion != null ? " [" + item.Seccion.Numero + "]" : "";
                string secciones = string.Empty;  //item.Seccion != null ? " [" + item.Seccion.Numero + "]" : "";

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

        private List<SelectListItem> ObtenerListaCalles(Guid selectedID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(calle)", Value = Guid.Empty.ToString() });

            foreach (var item in db.Calles
                .Where(c => c.Status == StatusTipo.Activo)
                .OrderBy(c => c.Nombre))
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

        // METODOS ASYNC

        private async Task<List<SelectListItem>> ObtenerListaPersonasAsync()
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(seleccionar persona)", Value = Guid.Empty.ToString() });

            foreach (var item in await (db.Personas
                .Where(p => p.Status == StatusTipo.Activo)
                .OrderBy(p => p.Nombres).ThenBy(p => p.ApellidoPaterno)).ToListAsync())
            {
                listado.Add(new SelectListItem
                {
                    Text = item.NombreCompleto,
                    Value = item.PersonaID.ToString()
                });
            }

            return listado;
        } // ObtenerListaPromotores

        private async Task<List<SelectListItem>> ObtenerListaPromotoresAsync(Guid selectedID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(promotor)", Value = Guid.Empty.ToString() });

            foreach (var item in await (db.Personas
                .Where(p => (p.Status == StatusTipo.Activo) && (p.Afinidad == AfinidadTipo.Movilizador || p.Afinidad == AfinidadTipo.Simpatizante))
                .OrderBy(p => p.Nombres)
                .ThenBy(p => p.ApellidoPaterno))
            .ToListAsync())
            {
                listado.Add(new SelectListItem
                {
                    Text = item.NombreCompleto,
                    Value = item.PersonaID.ToString(),
                    Selected = (item.PersonaID == selectedID)
                });
            }

            return listado;
        } // ObtenerListaPromotores

        private async Task<List<SelectListItem>> ObtenerListaSectoresAsync(Guid selectedID)
        {
            var sectores = await (db.Sectores
                .Where(s => s.Status == StatusTipo.Activo)
                .OrderBy(s => s.Nombre))
            .ToListAsync();
            var listado = new SelectList(sectores, "SectorID", "Nombre", selectedID).ToList();

            listado.Insert(0, new SelectListItem { Text = "(sector)", Value = Guid.Empty.ToString() });

            return listado;
        } // ObtenerListaSectores

        private async Task<List<SelectListItem>> ObtenerListaSeccionesAsync(Guid selectedID)
        {
            var secciones = await (db.Secciones
                .Where(s => s.Status == StatusTipo.Activo)
                .OrderBy(s => s.Numero))
            .ToListAsync();
            var listado = new SelectList(secciones, "SeccionID", "Numero", selectedID).ToList();

            listado.Insert(0, new SelectListItem { Text = "(sección)", Value = Guid.Empty.ToString() });

            return listado;
        } // ObtenerListaSecciones

        private async Task<List<SelectListItem>> ObtenerListaColoniasAsync(Guid selectedID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(colonia)", Value = Guid.Empty.ToString() });
            var colonias = await (db.Colonias.Include(c => c.Secciones).Include(c => c.Poblacion)
                .Where(p => p.Status == StatusTipo.Activo)
                .OrderBy(p => p.Nombre)).ToListAsync();

            foreach (var item in colonias)
            {
                //string seccion = item.Seccion != null ? " [" + item.Seccion.Numero + "]" : "";

                string secciones = string.Empty;  //item.Seccion != null ? " [" + item.Seccion.Numero + "]" : "";

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
            var calles = await (db.Calles
                .Where(c => c.Status == StatusTipo.Activo)
                .OrderBy(c => c.Nombre)).ToListAsync();

            foreach (var item in calles)
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

        private async Task EliminarRegistrosTemporalesAsync(string userName)
        {
            var casillas = await (from c in db.Casillas
                            where c.Status == StatusTipo.Ninguno && string.Compare(c.UserNameActualizacion, userName, true) == 0
                            select c).ToListAsync();

            foreach (Casilla casilla in casillas)
            {
                db.Casillas.Remove(casilla);
            }

            await db.SaveChangesAsync();
        } // EliminarRegistrosTemporales
    }
}
