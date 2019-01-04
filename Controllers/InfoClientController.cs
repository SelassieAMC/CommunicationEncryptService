using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using servicioCliente.AppUtils;
using servicioCliente.Models;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.Controllers
{
    [Route("Utils/[controller]/{action}")]
    [ApiController]
    public class InfoClientController : ControllerBase
    {
        private readonly IOptions<ParametersModel> parameters;
        public InfoClientController(IOptions<ParametersModel> config){
            parameters = config;
            FileWriter.parameters = config;
        }
        [HttpGet]
        public IActionResult GetIPAddress(){
            string ip = "";
            FileWriter.WriteOnEvents(EventLevel.Info,"Consultando direccion IP cliente.");
            try
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                ip = host.AddressList[1].ToString();
                FileWriter.WriteOnEvents(EventLevel.Info,"Direccion IP obtenida "+ip);
                return Ok(new {result = ip});
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Error intentando optener dirección IP. "+ex.Message);
                ObjectResult obj = new ObjectResult(new {IPAddress = ip});
                obj.StatusCode = 500;
                return obj;
            }
        }
    }
}