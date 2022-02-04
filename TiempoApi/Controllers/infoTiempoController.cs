using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TiempoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class infoTiempoController : ControllerBase
    {
        private readonly DatosContext _context;

        public infoTiempoController(DatosContext context)
        {
            _context = context;
        }

        
        // GET: api/infoTiempo
        [Autohorrize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<infoTiempo>>> GetInfoTiempo()
        {
            return await _context.TiempoInfo.ToListAsync();
        }

        // GET: api/infoTiempo/{localidad}
        [Autohorrize]
        [HttpGet("{localidad}")]
        public async Task<ActionResult<infoTiempo>> GetInfoTiempo(string localidad)
        {
            var tiempoItem =  await  _context.TiempoInfo.Where(o => o.Localidad == localidad).FirstAsync();

            if (tiempoItem == null)
            {
                return NotFound();
            }

            return tiempoItem;
        }

        private bool TiempoItemExists(string id)
        {
            return _context.TiempoInfo.Any(e => e.Localidad == id);
        }
    }
}
