using BiblioTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace BiblioTrack.Controllers
{
    [Route("api/health")]
    [ApiController]
    public class HealthController : Controller
    {
        private readonly ApiResponse _response;
        public HealthController()
        {
          _response = new ApiResponse();      
        }
        [HttpGet]
        [HttpHead]
        public IActionResult Get() {
           _response.IsSuccess = true;
           _response.StatusCode = System.Net.HttpStatusCode.OK;
           return Ok(_response);
        }
    }
}
