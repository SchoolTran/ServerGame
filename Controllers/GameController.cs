using Microsoft.AspNetCore.Mvc;
using ServerGame.Data;
using ServerGame.Models;

namespace ServerGame.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _context;
        public GameController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            IEnumerable<GameLevel> gameLevels = _context.GameLevels.ToList();
            return View(gameLevels);
        }
    }
}
