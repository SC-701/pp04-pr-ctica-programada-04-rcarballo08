using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web.Pages.Vehiculos
{
    public class EditarModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        public EditarModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }
        [BindProperty]
        public VehiculoResponse vehiculoResponse { get; set; }
        [BindProperty]
        public List <SelectListItem> marcas { get; set; }
        [BindProperty]
        public List<SelectListItem> modelos { get; set; }
        [BindProperty]
        public Guid marcaseleccionada { get; set; }
        [BindProperty]
        public Guid modeloseleccionado { get; set; }

        public async Task<ActionResult> OnGet(Guid? id)
        {
            if (id == Guid.Empty)
                return NotFound();
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints","ObtenerVehiculo");

            Console.WriteLine($"URL completa: {endpoint}");
            System.Diagnostics.Debug.WriteLine($"URL completa: {endpoint}");

            var cliente = new HttpClient();
            var solicitud = new HttpRequestMessage(HttpMethod.Get, string.Format(endpoint, id));
            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();
            if(respuesta.StatusCode == HttpStatusCode.OK)
            {
                await ObtenerMarcas();
                var resultado = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                vehiculoResponse = JsonSerializer.Deserialize<VehiculoResponse>(resultado, opciones);
                if (vehiculoResponse != null)
                {
                    marcaseleccionada = Guid.Parse(marcas.Where(m => m.Text == vehiculoResponse.Marca).FirstOrDefault().Value);
                    modelos = (await ObtenerModelos(marcaseleccionada)).Select(m =>
                    new SelectListItem {
                    Value = m.Id.ToString(),
                    Text = m.Nombre,
                    Selected = m.Nombre == vehiculoResponse.Modelo
                    }
                    ).ToList();
                    modeloseleccionado = Guid.Parse(modelos.Where(m => m.Text == vehiculoResponse.Marca).FirstOrDefault().Value);

                }
            }
            return Page();
        }

        public async Task<ActionResult> OnPost() {
            if (!ModelState.IsValid)
                return Page();
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "EditarVehiculo");
            var cliente = new HttpClient();
            var respuesta = await cliente.PutAsJsonAsync<VehiculoRequest>(string.Format(endpoint, vehiculoResponse.Id), new VehiculoRequest
            {
                Placa= vehiculoResponse.Placa,
                IdModelo = modeloseleccionado,
                Anio = vehiculoResponse.Anio,
                Color = vehiculoResponse.Color,
                Precio = vehiculoResponse.Precio,
                CorreoPropietario = vehiculoResponse.CorreoPropietario,
                TelefonoPropietario = vehiculoResponse.TelefonoPropietario
            });
            respuesta.EnsureSuccessStatusCode();
            return RedirectToPage("./Index");
        }

        private async Task ObtenerMarcas()
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerMarcas");

            Console.WriteLine($"URL completa: {endpoint}");
            System.Diagnostics.Debug.WriteLine($"URL completa: {endpoint}");

            var cliente = new HttpClient();
            var solicitud = new HttpRequestMessage(HttpMethod.Get, endpoint);

            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();
            var resultado = await respuesta.Content.ReadAsStringAsync();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resultadodeserializado = JsonSerializer.Deserialize<List<Marca>>(resultado, opciones);
            marcas = resultadodeserializado.Select(m=>
            new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Nombre,
            }
            ).ToList();
        }
        private async Task<List<Modelo>> ObtenerModelos(Guid marcaId)
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerModelos");

            Console.WriteLine($"URL completa: {endpoint}");
            System.Diagnostics.Debug.WriteLine($"URL completa: {endpoint}");

            var cliente = new HttpClient();
            var solicitud = new HttpRequestMessage(HttpMethod.Get, string.Format(endpoint, marcaId));

            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();
            if (respuesta.StatusCode==HttpStatusCode.OK)
            {
                var resultado = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<Modelo>>(resultado, opciones);
            }
                return new List<Modelo>();
        }
        public async Task <JsonResult> OnGetObtenerModelos(Guid marcaId)
        {
            var modelos = await ObtenerModelos(marcaId);
            return new JsonResult(modelos);
        }
    }
}
