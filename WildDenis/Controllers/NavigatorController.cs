using LayerDAL.Entitites;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Security.Claims;
using LayerDAL.Repository;
using LayerDAL.Entitites;
using LayerDAL.Settings;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace WildDenis.Controllers
{
    public class NavigatorController : Controller
    {
        private readonly IDBRepository _repository;
        private readonly ConnectionSetting _connection;

        public NavigatorController(IDBRepository repository, IOptions<ConnectionSetting> connection)
        {
            _repository = repository;
            _connection = connection.Value;
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Access");
        }

        [Authorize]
        public async Task<IActionResult> MainPage()
        {

            return View();
        }
        [Authorize]
        public async Task<IActionResult> TesterPage()
        {

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            string nick = User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ')[User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ').Length - 1];
            User Yu = await _repository.GetUserByNick(nick);
            return View(Yu);
        }



        


    }
}
