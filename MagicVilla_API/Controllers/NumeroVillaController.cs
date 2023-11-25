using AutoMapper;
using Azure;
using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.Dto;
using MagicVilla_API.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NumeroVillaController : ControllerBase
    {
        private readonly ILogger<VillaController> _logger;
        // private readonly ApplicationDbContext _db; Antes de implementar "Patron de Repositorio" 
        private readonly IVillaRepositorio _villaRepo;
        private readonly INumeroVillaRepositorio _numeroRepo;
        private readonly IMapper _mapper;
        protected APIResponse _response;

        public NumeroVillaController(ILogger<VillaController> logger, INumeroVillaRepositorio numeroRepo,
            IVillaRepositorio villaRepo, IMapper mapper)
        {
            _logger = logger;
            _villaRepo = villaRepo;
            _numeroRepo = numeroRepo;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetNumeroVillas()
        /*Metodo para obtener todas las villas*/
        {
            try
            {
                _logger.LogInformation("Obtener numero villas");
                //IEnumerable<Villa> villaList = await _db.Villas.ToListAsync(); //Consulta las villas en listado
                IEnumerable<NumeroVilla> numerovillaList = await _numeroRepo.ObtenerTodos(); //Obtiene las villas del repositorio
                _response.Resultado = _mapper.Map<IEnumerable<NumeroVillaDto>>(numerovillaList);
                _response.statusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _response.IsExitoso= false;
                _response.ErrorsMessages = new List<string> { ex.ToString() };
            }
            return _response;         
            //return Ok(await _db.Villas.ToListAsync());
        }

        [HttpGet("id:int", Name = "GetNumeroVilla")] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<APIResponse>> GetNumeroVilla(int id)
        {
            try
            {
                //Validacion del Id
                if (id == 0)
                {
                    _logger.LogError("Error al traer numero villa con Id " + id);
                    _response.statusCode=HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    return BadRequest(_response);
                }
                //var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);
                var numeroVilla = await _numeroRepo.Obtener(v => v.VillaNo == id);
                if (numeroVilla == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso = false;
                    return NotFound(_response);
                }
                _response.Resultado = _mapper.Map<VillaDto>(numeroVilla);
                _response.statusCode = HttpStatusCode.OK;
                //return Ok(villa);
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorsMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CrearNumeroVilla([FromBody] NumeroVillaCreateDto CreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (await _numeroRepo.Obtener(v => v.VillaNo == CreateDto.VillaNo) != null)
                {
                    ModelState.AddModelError("NombreExiste", "La villa con ese nombre ya existe!");
                    return BadRequest(ModelState);
                }
                if(await _villaRepo.Obtener(v=>v.Id == CreateDto.VillaId)==null)
                {
                    ModelState.AddModelError("ClaveForanea", "El id de la Villa no existe!");
                    return BadRequest(ModelState);
                }
                if (CreateDto == null)
                {
                    return BadRequest(CreateDto);
                }
                NumeroVilla modelo = _mapper.Map<NumeroVilla>(CreateDto); //Mapea y deposita la informacion de CreateDto a modelo
                //await _db.Villas.AddAsync(modelo);
                //await _db.SaveChangesAsync();
                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;
                await _numeroRepo.Crear(modelo);
                _response.Resultado= modelo;
                _response.statusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetNumeroVilla", new { id = modelo.VillaNo }, _response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorsMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNumeroVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var numeroVilla = await _numeroRepo.Obtener(v => v.VillaNo == id);
                if (numeroVilla == null)
                {
                    _response.IsExitoso= false;
                    _response.statusCode= HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _numeroRepo.Remover(numeroVilla);
                _response.statusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorsMessages = new List<string> { ex.ToString() };
            }
            return BadRequest(_response);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateNumeroVilla(int id, [FromBody] NumeroVillaUpdateDto UpdateDto)
        {
            if (UpdateDto == null || id != UpdateDto.VillaNo)
            {
                _response.IsExitoso = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return BadRequest();
            }
            if (await _villaRepo.Obtener(v => v.Id == UpdateDto.VillaId) == null)
            {
                ModelState.AddModelError("ClaveForanea", "El id de la Villa no existe!");
                return BadRequest(ModelState);
            }

            NumeroVilla modelo = _mapper.Map<NumeroVilla>(UpdateDto);
            await _numeroRepo.Actualizar(modelo);
            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
    }
}
