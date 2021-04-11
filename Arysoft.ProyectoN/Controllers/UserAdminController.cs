using Arysoft.ProyectoN.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Arysoft.ProyectoN.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersAdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public UsersAdminController()
        {
        }

        public UsersAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        //
        // GET: /Users/
        public async Task<ActionResult> Index(string buscar, string filtro, string orden)
        {

            ViewBag.Orden = orden;
            ViewBag.OrdenUserName = string.IsNullOrEmpty(orden) ? "user_desc" : "";
            ViewBag.OrdenNombre = orden == "nombre" ? "nombre_desc" : "nombre";
            //ViewBag.OrdenCURP = orden == "curp" ? "curp_desc" : "curp";

            if (buscar == null) { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            var users = await UserManager.Users
                .Include(u => u.Sector)
                .ToListAsync();

            //HttpContext.GetOwinContext().get

            users = users.Where(u => u.NombreCompleto.Contains(buscar) 
                || u.UserName.Contains(buscar)).ToList();

            switch (orden) {
                case "user_desc":
                    users = users.OrderByDescending(u => u.UserName).ToList();
                    break;
                case "nombre":
                    users = users.OrderBy(u => u.NombreCompleto).ToList();
                    break;
                case "nombre_desc":
                    users = users.OrderByDescending(u => u.NombreCompleto).ToList();
                    break;
                //case "curp":
                //    users = users.OrderBy(u => u.CURP).ToList();
                //    break;
                //case "curp_desc":
                //    users = users.OrderByDescending(u => u.CURP).ToList();
                //    break;
                default:
                    users = users.OrderBy(u => u.UserName).ToList();
                    break;
            }

            return View(users);//await UserManager.Users.ToListAsync()); // Esta consutla se hace más arriba.
        } // Index

        //
        // GET: /Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);


            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }

        //
        // GET: /Users/Create
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            var roles = await RoleManager.Roles.ToListAsync();

            roles = roles.OrderBy(r => r.Name).ToList();
            ViewBag.RoleId = new SelectList(roles, "Name", "Name"); // new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            ViewBag.SectorID = await ObtenerListaSectoresAsync(Guid.Empty);
            return View();
        } // Create

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, params string[] SelectedRole)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser {
                    SectorID = userViewModel.SectorID,
                    UserName = userViewModel.Email,
                    Email = userViewModel.Email,
                    Nombres = userViewModel.Nombres,
                    ApellidoPaterno = userViewModel.ApellidoPaterno,
                    ApellidoMaterno = userViewModel.ApellidoMaterno
                };
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    if (SelectedRole != null)
                    {
                        var result = await UserManager.AddToRolesAsync(user.Id, SelectedRole);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                            ViewBag.SectorID = await ObtenerListaSectoresAsync(Guid.Empty);
                            return View();
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                    ViewBag.SectorID = await ObtenerListaSectoresAsync(Guid.Empty);
                    return View();

                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            ViewBag.SectorID = await ObtenerListaSectoresAsync(Guid.Empty);
            return View();
        } // Create:POST

        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);
            ViewBag.SectorID = await ObtenerListaSectoresAsync(user.SectorID ?? Guid.Empty);

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                SectorID = user.SectorID,
                Email = user.Email,
                Nombres = user.Nombres,
                ApellidoPaterno = user.ApellidoPaterno,
                ApellidoMaterno = user.ApellidoMaterno,                
                RolesList = RoleManager.Roles.ToList().OrderBy(x => x.Name).Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        } // Edit

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id,SectorID,Nombres,ApellidoPaterno,ApellidoMaterno")] EditUserViewModel editUser, params string[] selectedRole)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.SectorID = editUser.SectorID;
                user.UserName = editUser.Email;
                user.Email = editUser.Email;
                user.Nombres = editUser.Nombres;
                user.ApellidoPaterno = editUser.ApellidoPaterno;
                user.ApellidoMaterno = editUser.ApellidoMaterno;
                //user.CURP = editUser.CURP;

                var userRoles = await UserManager.GetRolesAsync(user.Id);

                selectedRole = selectedRole ?? new string[] { };

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Ah ocurrido una excepción.");
            return View();
        }

        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        private async Task<List<SelectListItem>> ObtenerListaSectoresAsync(Guid selectedID)
        {
            var listado = new SelectList(await (db.Sectores
                .Where(c => c.Status == StatusTipo.Activo)
                .OrderBy(c => c.Nombre))
                .ToListAsync(), "SectorID", "Nombre", selectedID).ToList();
            listado.Insert(0, (new SelectListItem { Text = "(seleccionar sector)", Value = Guid.Empty.ToString() }));
            return listado;
        } // ObtenerListaSectoresAsync
    }
}
