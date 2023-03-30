using Microsoft.AspNetCore.Mvc;
using ServiceReference;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Index()
        {
            WebServiceSoap ws = new WebServiceSoapClient(WebServiceSoapClient.EndpointConfiguration.WebServiceSoap);

            var request = new HelloRequest(new HelloRequestBody() { name = "Fatih" });
            var response = ws.HelloAsync(request);

            return response.Result.Body.HelloResult;
        }
    }
}