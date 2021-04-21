using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Arysoft.ProyectoN.Models;

namespace Arysoft.ProyectoN.Controllers
{
    [Authorize]
    public class AuditoriaController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Auditoria
        [Authorize(Roles = "Admin, Editor, Auditor, Consultant")]
        public ActionResult Index(string buscar, string filtro, string status, string orden)
        {
            ViewBag.Orden = orden;
            ViewBag.OrdenFolio = string.IsNullOrEmpty(orden) ? "folio" : "";
            ViewBag.OrdenResponsable = orden == "responsable" ? "responsable_desc" : "responsable";
            ViewBag.OrdenRealizacion = orden == "realizacion" ? "realizacion_desc" : "realizacion";
            ViewBag.OrdenNoPersonas = orden == "no_personas" ? "no_personas_desc" : "no_personas";

            if (buscar != null)
            {
                //pagina = 1
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            var auditorias = db.Auditorias
                .Include(a => a.Responsable)
                .Where(a => a.Status != AuditoriaStatusTipo.Ninguno);

            if (!string.IsNullOrEmpty(buscar) || !string.IsNullOrEmpty(status))
            {
                bool hayPalabras = !string.IsNullOrEmpty(buscar);
                status = string.IsNullOrEmpty(status) ? AuditoriaStatusTipo.Ninguno.ToString() : status;

                int.TryParse(buscar, out int folio);

                auditorias = auditorias.Where(a =>
                    (
                        !hayPalabras
                        || a.Nombre.Contains(buscar)
                        || (a.Responsable == null ? false : a.Responsable.Nombres.Contains(buscar))
                        || (a.Responsable == null ? false : a.Responsable.ApellidoPaterno.Contains(buscar))
                        || (a.Responsable == null ? false : a.Responsable.ApellidoMaterno.Contains(buscar))
                        || a.Descripcion.Contains(buscar)
                        || (folio == 0 ? false : a.Folio == folio)
                    )
                    && (status == AuditoriaStatusTipo.Ninguno.ToString() ? true : a.Status.ToString() == status)
                );
            }

            switch (orden) {
                case "folio":
                    auditorias = auditorias.OrderBy(a => a.Folio);
                    break;
                case "responsable":
                    auditorias = auditorias
                        .OrderBy(a => a.Responsable == null)
                        .ThenBy(a => a.Responsable.Nombres);
                    break;
                case "responsable_desc":
                    auditorias = auditorias
                        .OrderByDescending(a => a.Responsable == null)
                        .ThenByDescending(a => a.Responsable.Nombres);
                    break;
                case "realizacion":
                    auditorias = auditorias.OrderBy(a => a.FechaRealizacion);
                    break;
                case "realizacion_desc":
                    auditorias = auditorias.OrderByDescending(a => a.FechaRealizacion);
                    break;
                case "no_personas":
                    auditorias = auditorias
                        .OrderBy(a => a.PersonasAuditadas == null)
                        .ThenBy(a => a.PersonasAuditadas.Count());
                    break;
                case "no_personas_desc":
                    auditorias = auditorias
                        .OrderByDescending(a => a.PersonasAuditadas == null)
                        .ThenByDescending(a => a.PersonasAuditadas.Count());
                    break;
                default:
                    auditorias = auditorias.OrderByDescending(a => a.Folio);
                    break;
            }

            ViewBag.Status = status;

            return View(auditorias.ToList());
        }

        // GET: Auditoria/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("noid"); }
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auditoria auditoria = db.Auditorias
                .Include(a => a.PersonasAuditadas)
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Seccion))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Seccion.Sector))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Promotor))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive.Calle))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive.Colonia))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota.Calle))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota.Colonia))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Notas))
                .Where(a => a.AuditoriaID == id)
                .FirstOrDefault();
            if (auditoria == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("nofound"); }
                return HttpNotFound();
            }

            auditoria.SoloLectura = true;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_details", auditoria);
            }

            return View(auditoria);
        }

        // GET: Auditoria/Create
        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult Create()
        {
            var user = ControllerContext.HttpContext.User.Identity;

            EliminarRegistrosTemporales(user.Name);

            Auditoria auditoria = new Auditoria
            {
                AuditoriaID = Guid.NewGuid(),
                Folio = 0,
                Nombre = string.Empty,
                Descripcion = string.Empty,
                Status = AuditoriaStatusTipo.Ninguno,
                FechaActualizacion = DateTime.Now,
                UserNameActualizacion = user.Name
            };

            db.Auditorias.Add(auditoria);

            try
            {
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = auditoria.AuditoriaID });
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }            
        } // GET: Create

        // GET: Auditoria/Edit/5
        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auditoria auditoria = db.Auditorias
                .Include(a => a.PersonasAuditadas)
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Seccion))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Seccion.Sector))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Promotor))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive.Calle))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive.Colonia))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota.Calle))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota.Colonia))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Notas))
                .Where(a => a.AuditoriaID == id)
                .FirstOrDefault();
            if (auditoria == null)
            {
                return HttpNotFound();
            }

            ViewBag.ResponsableID = ObtenerListaAfiliados(auditoria.ResponsableID ?? Guid.Empty); //new SelectList(db.Personas.Where(p => p.Status == StatusTipo.Activo && p.Afinidad == AfinidadTipo.Afiliado), "PersonaID", "Nombres", auditoria.ResponsableID);

            auditoria.SoloLectura = false;

            return View(auditoria);
        } // GET: Edit

        // POST: Auditoria/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult Edit([Bind(Include = "AuditoriaID,ResponsableID,Folio,Nombre,Descripcion,FechaRealizacion,Status,UserNameActualizacion,FechaActualizacion")] Auditoria auditoria, string submitButton)
        {
            var user = ControllerContext.HttpContext.User.Identity;
            bool esNuevo = auditoria.Status == AuditoriaStatusTipo.Ninguno;
            bool cambioStatusRealizada = false;

            if (esNuevo)
            {

                if (auditoria.FechaRealizacion > DateTime.Today)
                {
                    auditoria.Status = AuditoriaStatusTipo.Espera;
                }
                else
                {
                    auditoria.Status = AuditoriaStatusTipo.Realizada;
                }
            }
            else
            {
                
            }

            if (ModelState.IsValid)
            {
                if (esNuevo)
                {
                    auditoria.Folio = ObtenerFolio();
                }
                else
                {
                    switch (submitButton) {
                        case "realizada":
                            auditoria.Status = AuditoriaStatusTipo.Realizada;
                            cambioStatusRealizada = true;
                            AgregarNota(auditoria.AuditoriaID, "Auditoria Realizada", user.Name, PropietarioTipo.Auditoria);
                            break;
                        case "cancelar":
                            auditoria.Status = AuditoriaStatusTipo.Cancelada;
                            AgregarNota(auditoria.AuditoriaID, "Auditoria Cancelada", user.Name, PropietarioTipo.Auditoria);
                            break;
                        case "cerrar":
                            auditoria.Status = AuditoriaStatusTipo.Cerrada;
                            AgregarNota(auditoria.AuditoriaID, "Auditoria Cerrada", user.Name, PropietarioTipo.Auditoria);
                            break;
                    }
                }

                auditoria.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                auditoria.FechaActualizacion = DateTime.Now;

                db.Entry(auditoria).State = EntityState.Modified;
                db.SaveChanges();

                // Si no cambia, se guardan los cambios y se regresa a index, si cambia
                // se queda en la pantalla con los cambios realizados.
                if (!cambioStatusRealizada)
                {
                    return RedirectToAction("Index");
                }
                else {
                    auditoria.Notas = db.Notas.Where(n => n.PropietarioID == auditoria.AuditoriaID).ToList();
                    auditoria.PersonasAuditadas = db.AuditoriaPersonas.Where(ap => ap.AuditoriaID == auditoria.AuditoriaID).ToList();
                }
            }

            ViewBag.ResponsableID = ObtenerListaAfiliados(auditoria.ResponsableID ?? Guid.Empty);

            auditoria.SoloLectura = false;

            return View(auditoria);
        }

        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult Activar(Guid? id)
        {
            string mensaje = string.Empty;

            if (id == null)
            {
                mensaje = "No se recibió un identificador";   
            }

            Auditoria auditoria = db.Auditorias.Find(id);
            if (auditoria == null)
            {
                if (mensaje.Length > 0) mensaje += "<br />";
                mensaje += "No se recibió un identificador";
            }
            
            if (string.IsNullOrEmpty(mensaje))
            {
                AgregarNota(auditoria.AuditoriaID, "Auditoria Reactivada", ControllerContext.HttpContext.User.Identity.Name, PropietarioTipo.Auditoria);

                auditoria.Status = AuditoriaStatusTipo.Espera;
                auditoria.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                auditoria.FechaActualizacion = DateTime.Now;

                db.Entry(auditoria).State = EntityState.Modified;
                db.SaveChanges();

                TempData["MessageBox"] = "Reactivación de auditoria realizada con éxito.";
            }
            else
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + mensaje;
            }

            return RedirectToAction("Index");
        } // GET: Activar

        // GET: Auditoria/Delete/5
        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auditoria auditoria = db.Auditorias.Find(id);
            if (auditoria == null)
            {
                return HttpNotFound();
            }
            return View(auditoria);
        }

        // POST: Auditoria/Delete/5
        [Authorize(Roles = "Admin, Editor, Auditor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Auditoria auditoria = db.Auditorias.Find(id);
            db.Auditorias.Remove(auditoria);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

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

        // DASHBOARD
        
        public ActionResult TotalAuditorias()
        {
            int totalAuditorias = (from a in db.Auditorias
                                   where a.Status != AuditoriaStatusTipo.Ninguno
                                   select a).Count();

            int totalAuditoriasEspera = (from a in db.Auditorias
                                         where a.Status == AuditoriaStatusTipo.Espera
                                         select a).Count();

            int totalAuditoriasRealizada = (from a in db.Auditorias
                                         where a.Status == AuditoriaStatusTipo.Realizada
                                         select a).Count();

            int totalAuditoriasCerrada = (from a in db.Auditorias
                                         where a.Status == AuditoriaStatusTipo.Cerrada
                                         select a).Count();

            int totalAuditoriasCancelada = (from a in db.Auditorias
                                         where a.Status == AuditoriaStatusTipo.Cancelada
                                         select a).Count();

            return Json(new {
                totalAuditorias,
                totalAuditoriasEspera,
                totalAuditoriasRealizada,
                totalAuditoriasCerrada,
                totalAuditoriasCancelada
            }, JsonRequestBehavior.AllowGet);
        } // TotalAuditorias

        // PERSONAS

        public ActionResult DetallesPersona(Guid id, string url)
        {

            if (id == null)
            {
                if(Request.IsAjaxRequest()) { return Content("notid"); }
            }

            AuditoriaPersona ap = db.AuditoriaPersonas
                .Include(a => a.Persona)
                .Include(a => a.Persona.Seccion)
                .Include(a => a.Persona.Seccion.Sector)
                .Include(a => a.Persona.Promotor)
                .Include(a => a.Persona.UbicacionVive)
                .Include(a => a.Persona.UbicacionVive.Calle)
                .Include(a => a.Persona.UbicacionVive.Colonia)
                .Include(a => a.Persona.UbicacionVota)
                .Include(a => a.Persona.UbicacionVota.Calle)
                .Include(a => a.Persona.UbicacionVota.Colonia)
                .Include(a => a.Persona.Notas)
                .Where(a => a.AuditoriaPersonaID == id)
                .FirstOrDefault();

            if (ap == null)
            {
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
            }

            ap.SoloLectura = true;
            ViewBag.Url = url;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_detailsPersona", ap);
            }

            return View(ap);
        } // DetallesPersona

        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult AgregarPersonas(Guid id)
        {
            ViewBag.AuditoriaID = id;
            ViewBag.SectorID = ObtenerListaSectores(Guid.Empty);
            ViewBag.SeccionID = ObtenerListaSecciones(Guid.Empty);
            ViewBag.CalleID = ObtenerListaCalles(Guid.Empty);
            ViewBag.ColoniaID = ObtenerListaColonias(Guid.Empty);

            return PartialView("_personasAgregar");
        } // AgregarPersonas

        [HttpPost]
        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult AgregarPersonas(Guid id, string personas)
        {
            Auditoria auditoria = db.Auditorias.Find(id);

            char[] separador = new char[] { ',' };
            string[] identificadores = personas.Split(separador, StringSplitOptions.RemoveEmptyEntries);

            if (identificadores.Length > 0)
            {
                foreach (string personaId in identificadores)
                {
                    Guid PersonaID = new Guid(personaId);
                    if (auditoria.PersonasAuditadas.Where(pa => pa.PersonaID == PersonaID).Count() == 0)
                    {
                        AuditoriaPersona ap = new AuditoriaPersona();
                        Persona p = db.Personas.Find(PersonaID);

                        ap.AuditoriaPersonaID = Guid.NewGuid();
                        ap.AuditoriaID = auditoria.AuditoriaID;
                        ap.PersonaID = PersonaID;
                        ap.Observaciones = string.Empty;
                        ap.VotanteSeguro = p.VotanteSeguro;
                        ap.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                        ap.FechaActualizacion = DateTime.Now;

                        auditoria.PersonasAuditadas.Add(ap);
                    }
                } // foreach

                db.SaveChanges();
            }

            Auditoria auditoriaResult = db.Auditorias
                .Include(a => a.PersonasAuditadas)
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Seccion))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Seccion.Sector))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Promotor))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive.Calle))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive.Colonia))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota.Calle))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota.Colonia))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Notas))
                .Where(a => a.AuditoriaID == id)
                .FirstOrDefault();

            return PartialView("_listaPersonas", auditoriaResult);
        } // AgregarPersonas

        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult EditarPersona(Guid id)
        {
            if (id == null)
            {
                if (Request.IsAjaxRequest()) { return Content("notid"); }
            }

            // AuditoriaPersona ap = db.AuditoriaPersonas.Find(id);

            AuditoriaPersona ap = db.AuditoriaPersonas
                .Include(a => a.Persona)
                .Include(a => a.Persona.Seccion)
                .Include(a => a.Persona.Seccion.Sector)
                .Include(a => a.Persona.Promotor)
                .Include(a => a.Persona.UbicacionVive)
                .Include(a => a.Persona.UbicacionVive.Calle)
                .Include(a => a.Persona.UbicacionVive.Colonia)
                .Include(a => a.Persona.UbicacionVota)
                .Include(a => a.Persona.UbicacionVota.Calle)
                .Include(a => a.Persona.UbicacionVota.Colonia)
                .Include(a => a.Persona.Notas)
                .Where(a => a.AuditoriaPersonaID == id)
                .FirstOrDefault();

            if (ap == null)
            {
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
            }

            ap.SoloLectura = false; //!(ap.Auditoria.Status == AuditoriaStatusTipo.Espera || ap.Auditoria.Status == AuditoriaStatusTipo.Realizada);
            
            if (Request.IsAjaxRequest())
            {
                return PartialView("_detailsPersona", ap);
            }

            return View(ap);
        } // EditarPersona

        [HttpPost]
        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult EditarPersona(Guid id, string observaciones, string votanteSeguro, DateTime fecha, AuditoriaResultadoTipo resultado)
        {
            if (id == null)
            {
                if (Request.IsAjaxRequest()) { return Content("notid"); }
            }

            // AuditoriaPersona ap = db.AuditoriaPersonas.Find(id);

            AuditoriaPersona ap = db.AuditoriaPersonas
                .Include(a => a.Persona)
                .Include(a => a.Persona.Seccion)
                .Include(a => a.Persona.Seccion.Sector)
                .Include(a => a.Persona.Promotor)
                .Include(a => a.Persona.UbicacionVive)
                .Include(a => a.Persona.UbicacionVive.Calle)
                .Include(a => a.Persona.UbicacionVive.Colonia)
                .Include(a => a.Persona.UbicacionVota)
                .Include(a => a.Persona.UbicacionVota.Calle)
                .Include(a => a.Persona.UbicacionVota.Colonia)
                .Include(a => a.Persona.Notas)
                .Where(a => a.AuditoriaPersonaID == id)
                .FirstOrDefault();

            if (ap == null)
            {
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
            }

            ap.FechaAuditoria = fecha;
            ap.Observaciones = observaciones;
            ap.ResultadoTipo = resultado;
            ap.VotanteSeguro = votanteSeguro == BoolTipo.Si.ToString() ? BoolTipo.Si : BoolTipo.No;

            if (ap.VotanteSeguro != ap.Persona.VotanteSeguro)
            {
                string texto = "CAMBIO INTENCION DEL VOTO A " 
                    + (ap.VotanteSeguro == BoolTipo.Si ? "VOTO SEGURO" : "NO SEGURO") 
                    + " (ver auditoria " + ap.Auditoria.Folio.ToString().PadLeft(3, '0') + "): " 
                    + observaciones;
                AgregarNota(ap.PersonaID, texto, ControllerContext.HttpContext.User.Identity.Name, PropietarioTipo.Persona);

                ap.Persona.VotanteSeguro = ap.VotanteSeguro;
                ap.Persona.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                ap.Persona.FechaActualizacion = DateTime.Now;
                db.Entry(ap.Persona).State = EntityState.Modified;
            }

            ap.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
            ap.FechaActualizacion = DateTime.Now;
            db.Entry(ap).State = EntityState.Modified;
            db.SaveChanges();

            Auditoria auditoria = db.Auditorias
                .Include(a => a.PersonasAuditadas)
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Seccion))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Seccion.Sector))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Promotor))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive.Calle))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVive.Colonia))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota.Calle))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.UbicacionVota.Colonia))
                .Include(a => a.PersonasAuditadas.Select(p => p.Persona.Notas))
                .Where(a => a.AuditoriaID == ap.AuditoriaID)
                .FirstOrDefault();

            return PartialView("_listaPersonas", auditoria);

        } // EditarPersona

        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult EditarPersonas()
        {

            //if (string.IsNullOrEmpty(ids))
            //{
            //    if (Request.IsAjaxRequest()) { return Content("notids"); }
            //}

            //char[] separador = new char[] { ',' };
            //string[] identificadores = ids.Split(separador, StringSplitOptions.RemoveEmptyEntries);

            //var listaPersonas = db.AuditoriaPersonas.Where(ap => identificadores.Contains(ap.AuditoriaPersonaID.ToString()));

            return PartialView("_editPersonas"); //, listaPersonas);
        } // EditarPersonas

        [HttpPost]
        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult EditarPersonas(string ids, string observaciones, string votanteSeguro, DateTime fecha, AuditoriaResultadoTipo resultado)
        {
            Guid auditoriaID = Guid.Empty;

            //if (string.IsNullOrEmpty(ids))
            //{
            //    if (Request.IsAjaxRequest()) { return Content("notids"); }
            //}

            char[] separador = new char[] { ',' };
            string[] identificadores = ids.Split(separador, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in db.AuditoriaPersonas
                .Include(a => a.Auditoria)
                .Include(a => a.Persona)
                .Where(ap => identificadores.Contains(ap.AuditoriaPersonaID.ToString())))
            {
                item.FechaAuditoria = fecha;
                item.Observaciones = observaciones;
                item.ResultadoTipo = resultado;
                item.VotanteSeguro = votanteSeguro == BoolTipo.Si.ToString() ? BoolTipo.Si : BoolTipo.No;

                if (item.VotanteSeguro != item.Persona.VotanteSeguro)
                {
                    string texto = "CAMBIO INTENCION DEL VOTO A "
                    + (item.VotanteSeguro == BoolTipo.Si ? "VOTO SEGURO" : "NO SEGURO")
                    + " (ver auditoria " + item.Auditoria.Folio.ToString().PadLeft(3, '0') + "): "
                    + observaciones;
                    AgregarNota(item.PersonaID, texto, User.Identity.Name, PropietarioTipo.Persona);

                    item.Persona.VotanteSeguro = item.VotanteSeguro;
                    item.Persona.UserNameActualizacion = User.Identity.Name;
                    item.Persona.FechaActualizacion = DateTime.Now;
                    db.Entry(item.Persona).State = EntityState.Modified;
                }

                item.UserNameActualizacion = User.Identity.Name;
                item.FechaActualizacion = DateTime.Now;
                db.Entry(item).State = EntityState.Modified;

                auditoriaID = item.AuditoriaID;
            }

            db.SaveChanges();

            Auditoria auditoria = db.Auditorias.Find(auditoriaID);

            return PartialView("_listaPersonas", auditoria);
        } // EditarPersonas

        [HttpPost]
        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult EliminarPersonas(Guid id, string personas)
        {
            Auditoria auditoria = db.Auditorias.Find(id);

            char[] separador = new char[] { ',' };
            string[] identificadores = personas.Split(separador, StringSplitOptions.RemoveEmptyEntries);

            if ((auditoria.Status == AuditoriaStatusTipo.Espera 
                    || auditoria.Status == AuditoriaStatusTipo.Realizada
                    || auditoria.Status == AuditoriaStatusTipo.Ninguno)
                && identificadores.Length > 0)
            {
                foreach (string personaId in identificadores)
                {
                    Guid PersonaID = new Guid(personaId);
                    AuditoriaPersona ap = db.AuditoriaPersonas.Find(PersonaID);

                    if (ap != null)
                    {
                        db.AuditoriaPersonas.Remove(ap);
                    }                    
                }

                db.SaveChanges();
                auditoria = db.Auditorias.Find(id);
            }

            return PartialView("_listaPersonas", auditoria);
        } // EliminarPersonas

        [Authorize(Roles = "Admin, Editor, Auditor")]
        public ActionResult BuscarPersonas(string buscar, string orden, string afinidad, string SectorID, string SeccionID,
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
                .Include(p => p.Seccion)
                .Include(p => p.Seccion.Sector)
                .Include(p => p.UbicacionVive)
                .Include(p => p.UbicacionVive.Calle)
                .Include(p => p.UbicacionVive.Colonia)
                .Include(p => p.UbicacionVota)
                .Include(p => p.UbicacionVota.Calle)
                .Include(p => p.UbicacionVota.Colonia)
                .Include(p => p.PersonasAfines)
                .Include(p => p.Notas)
                .Where(p => p.Status == StatusTipo.Activo || p.Status == StatusTipo.Baja);
            var personasList = new List<Persona>();

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
                    personas = personas.OrderByDescending(p => p.Nombres);
                    break;
                case "sector":
                    personasList = personasList.OrderBy(p => p.Seccion == null).ThenBy(p => p.Seccion.Sector == null).ThenBy(p => p.Seccion.Sector.Nombre).ToList();
                    break;
                case "sector_desc":
                    personasList = personasList.OrderByDescending(p => p.Seccion == null).ThenByDescending(p => p.Seccion.Sector.Nombre).ToList();
                    break;
                case "seccion":
                    personas = personas.OrderBy(p => p.Seccion == null).ThenBy(p => p.Seccion.Numero).ThenBy(p => p.Nombres);
                    break;
                case "seccion_desc  ":
                    personas = personas.OrderByDescending(p => p.Seccion == null).ThenByDescending(p => p.Seccion.Numero).ThenBy(p => p.Nombres);
                    break;
                case "brigada":
                    personas = personas.OrderBy(p => p.Sector == null).ThenBy(p => p.Sector.Nombre).ThenBy(p => p.Nombres);
                    break;
                case "brigada_desc":
                    personas = personas.OrderByDescending(p => p.Sector == null).ThenByDescending(p => p.Sector.Nombre).ThenBy(p => p.Nombres);
                    break;
                default:
                    personas = personas.OrderBy(p => p.Nombres);
                    break;
            }

            return PartialView("_listaPersonasAgregarAuditoria", personas);
        } // BuscarPersonas

        //     NOTAS

        public ActionResult AgregarNota(Guid id, string texto, PropietarioTipo tipo)
        {

            AgregarNota(id, texto, ControllerContext.HttpContext.User.Identity.Name, tipo);

            var listaNotas = from n in db.Notas where n.PropietarioID == id select n;

            return PartialView("_listaNotas", listaNotas);
        } // AgregarNota

        // METODOS PRIVADOS

        private int ObtenerFolio() {
            int folio = 0;
            int encontroFolio = 0;

            folio = (from a in db.Auditorias where a.Status != AuditoriaStatusTipo.Ninguno select a).Count();
            
            do {
                folio++;
                encontroFolio = (from a in db.Auditorias
                                 where a.Status != AuditoriaStatusTipo.Ninguno && a.Folio == folio
                                 select a).Count();                
            } while (encontroFolio != 0);

            return folio;
        } // ObtenerFolio

        private bool AgregarNota(Guid id, string texto, string autor, PropietarioTipo tipo)
        {
            ApplicationDbContext dbLocal = new ApplicationDbContext();
            Nota nota = new Nota
            {
                NotaID = Guid.NewGuid(),
                PropietarioID = id,
                Autor = autor,
                Texto = texto,
                PropietarioTipo = tipo,
                FechaPublicacion = DateTime.Now
            };

            dbLocal.Notas.Add(nota);
            try
            {
                dbLocal.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        } // AgregarNota

        // LISTADOS

        private List<SelectListItem> ObtenerListaAfiliados(Guid selectedID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(selecciona persona)", Value = Guid.Empty.ToString() });

            foreach (var item in db.Personas
                .Where(p => p.Status == StatusTipo.Activo && (p.Afinidad == AfinidadTipo.Movilizador || p.Afinidad == AfinidadTipo.Simpatizante))
                .OrderBy(p => p.Nombres).ThenBy(p => p.ApellidoPaterno))
            {
                listado.Add(new SelectListItem {
                    Text = item.NombreCompleto,
                    Value = item.PersonaID.ToString(),
                    Selected = (item.PersonaID == selectedID)
                });
            }

            return listado;
        } // ObtenerListaAfiliados

        private List<SelectListItem> ObtenerListaSecciones(Guid selectedID)
        {
            var listado = new SelectList(db.Secciones.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Numero), "SeccionID", "Numero", selectedID).ToList();

            listado.Insert(0, new SelectListItem { Text = "(sección)", Value = Guid.Empty.ToString() });

            return listado;
        } // ObtenerListaSecciones

        private List<SelectListItem> ObtenerListaSectores(Guid selectedID)
        {
            var listado = new SelectList(db.Sectores.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Nombre), "SectorID", "Nombre", selectedID).ToList();

            listado.Insert(0, new SelectListItem { Text = "(sector)", Value = Guid.Empty.ToString() });

            return listado;
        } // ObtenerListaSectores

        private List<SelectListItem> ObtenerListaColonias(Guid selectedID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(colonia)", Value = Guid.Empty.ToString() });

            foreach (var item in db.Colonias
                .Include(c => c.Poblacion)
                .Where(p => p.Status == StatusTipo.Activo)
                .OrderBy(p => p.Nombre))
            {
                // string seccion = item.Seccion != null ? " [" + item.Seccion.Numero + "]" : "";
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


        private void EliminarRegistrosTemporales(string userName)
        {
            var auditorias = (from a in db.Auditorias
                              where a.Status == AuditoriaStatusTipo.Ninguno
                                && string.Compare(a.UserNameActualizacion, userName, true) == 0
                              select a).ToList();

            foreach (var item in auditorias)
            {
                if (item.PersonasAuditadas.Count > 0)
                {
                    //foreach (var element in item.PersonasAuditadas)
                    //{
                    //    db.AuditoriaPersonas.Remove(element);
                    //}
                }
                db.Auditorias.Remove(item);
            }

            db.SaveChanges();
        } // EliminarRegistrosTemporales
    }
}
