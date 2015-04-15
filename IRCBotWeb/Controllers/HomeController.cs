using IRCBotWeb.Hubs;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace IRCBotWeb.Controllers
{
    public class HomeController : Controller
    {
        private IHubContext _hubContext;
        public HomeController(IConnectionManager connectionManager)
        {
            _hubContext = connectionManager.GetHubContext<IRCHub>();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}