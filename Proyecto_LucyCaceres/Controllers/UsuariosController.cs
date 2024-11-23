using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Proyecto_LucyCaceres.Models;

namespace Proyecto_LucyCaceres.Controllers
{
    public class UsuariosController : ApiController
    {
        private TransitoEntities db = new TransitoEntities();

        // GET: api/Usuarios
        public IQueryable<UsuarioVM> GetUsuarios()
        {
            var usuarios = db.Usuarios.Select(usu => new UsuarioVM
            {
                dni = usu.dni,
                nombre = usu.nombre,
                apellido = usu.apellido,
                genero = usu.genero,
                email = usu.email,
                celular = usu.celular,
                contrasena = usu.contrasena,
                rolId = usu.rolId,
                rol = usu.Role.nombre
            });
            return usuarios;
        }

        // GET: api/Usuarios/5
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult GetUsuario(string id)
        {
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var UsuarioVM = new UsuarioVM
            {
                dni = usuario.dni,
                nombre = usuario.nombre,
                apellido = usuario.apellido,
                genero = usuario.genero,
                email = usuario.email,
                celular = usuario.celular,
                contrasena = usuario.contrasena,
                rolId = usuario.rolId,
                rol = usuario.Role.nombre
            };
            return Ok(UsuarioVM);
        }

        // PUT: api/Usuarios/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUsuario(string id, Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != usuario.dni)
            {
                return BadRequest();
            }

            db.Entry(usuario).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Usuarios
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult PostUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Usuarios.Add(usuario);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (UsuarioExists(usuario.dni))
                {
                    return BadRequest("Existe");
                }
                else
                {
                    throw;
                }
            }
            //var uri = Url.Link("Default", new { Controller = "Login", Action = "SinAcceso" });
            return Ok(true);
        }

        // DELETE: api/Usuarios/5
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult DeleteUsuario(string id)
        {
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }

            db.Usuarios.Remove(usuario);
            db.SaveChanges();

            return Ok(usuario);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UsuarioExists(string id)
        {
            return db.Usuarios.Count(e => e.dni == id) > 0;
        }

        [HttpGet]
        [Route("api/Usuarios/codigo/{dni}")]
        [ResponseType(typeof(UsuarioVM))]
        public IHttpActionResult GetUsuarioCodigo(string dni)
        {
            var usuario = db.Usuarios.Where(b => b.dni == dni).Select(usu => new UsuarioVM
            {
                dni = usu.dni,
                nombre = usu.nombre,
                apellido = usu.apellido,
                genero = usu.genero,
                email = usu.email,
                celular = usu.celular,
                contrasena = usu.contrasena,
                rolId = usu.rolId
            }).ToList().FirstOrDefault();

            if (usuario == null)
            {
                return NotFound();
            }
            return Ok(usuario);
        }

        [HttpGet]
        [Route("api/Usuarios/oficial")]
        public IQueryable<UsuarioVM> GetUsuariosOficial()
        {
            var usuarios = db.Usuarios.Where(w=> w.rolId == 2).Select(usu => new UsuarioVM
            {
                dni = usu.dni,
                nombre = usu.nombre,
                apellido = usu.apellido,
                genero = usu.genero,
                email = usu.email,
                celular = usu.celular,
                contrasena = usu.contrasena,
                rol = usu.Role.nombre
            });
            return usuarios;
        }
    }
}