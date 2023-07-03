using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WildDenis.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using LayerDAL.Repository;
using LayerDAL.Entitites;
using LayerDAL.Settings;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Text;

namespace WildDenis.Controllers
{
    public class AccessController : Controller
    {
        private readonly IDBRepository _repository;
        private readonly ConnectionSetting _connection;


        IWebHostEnvironment _appEnvironment;

        public AccessController(IDBRepository repository, IOptions<ConnectionSetting> connection, IWebHostEnvironment appEnvironment)
        {
            _repository = repository;
            _connection = connection.Value;
            _appEnvironment = appEnvironment;
        }

        public string TakeInfoFromUserClaim(ClaimsPrincipal claimUser, string property)
        {
            try
            {
                return claimUser.Claims.First(c => c.Type == property).ToString().Split(' ')[claimUser.Claims.First(c => c.Type == property).ToString().Split(' ').Length - 1];
            }
            catch
            {
                return " "; 
            }
        }


        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated && TakeInfoFromUserClaim(claimUser, ClaimsIdentity.DefaultNameClaimType).ToLower() == "tester")
            {
                return RedirectToAction("TesterPage", "Navigator");
            }
            else if (claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("MainPage", "Navigator");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Authorization modelLogin)
        {
            if (ModelState.IsValid)
            {
                List<User> ListClients = await _repository.GetUsers();

                foreach (var a in ListClients)
                {
                    if (modelLogin.nickName.ToLower() == a.NickName.ToLower() && HashPassword(modelLogin.pass) == a.Pass)
                    {
                        List<Claim> claims = new List<Claim>() {
                            new Claim(ClaimsIdentity.DefaultNameClaimType, modelLogin.nickName)
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        AuthenticationProperties properties = new AuthenticationProperties()
                        {
                            AllowRefresh = true,
                            IsPersistent = modelLogin.keepLoggedIn
                        };
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                        if(modelLogin.nickName.ToLower() == "tester")
                        {
                            return RedirectToAction("TesterPage", "Navigator");
                        }
                        return RedirectToAction("MainPage", "Navigator");
                    }
                }

            }
            ViewData["ValidateMessage"] = "Неправильный логин или пароль";
            return View();
        }

        public IActionResult Registrate()
        {
            return View();
        }

        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Registrate(Registration modelRegistration, IFormFile uploadedFile)
        {
            if (uploadedFile == null)
            {
                ViewData["ValidateMessage"] = "Загрузите фото";
                return View();
            }

            if (ModelState.IsValid)
            {
                string SQLQuery = $"SELECT * FROM _User WHERE NickName = @nick";
                using (var connect = new SqlConnection(_connection.SQLString))
                {
                    await connect.OpenAsync();
                    SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                    cmd.Parameters.AddWithValue("@nick", $"{modelRegistration.nickName.ToLower()}");
                    var reader = await cmd.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        ViewData["ValidateMessage"] = "Данный логин - занят!";
                        return View();
                    }
                }
                Regex rx = new Regex(@"^\+\d{3}\(\d{2}\)\d{3}\-\d{2}\-\d{2}$");
                if (!rx.IsMatch(modelRegistration.phoneNumber))
                {
                    ViewData["ValidateMessage"] = "Неправильный формат номера";
                    return View();
                }
                rx = new Regex(@"^[a-zA-Z0-9]{1}[a-zA-Z0-9-_]{0,30}[a-zA-Z0-9]{1}\@[a-zA-Z]{1,10}\.[a-zA-Z]{1,10}$");
                if (!rx.IsMatch(modelRegistration.email))
                {
                    ViewData["ValidateMessage"] = "Неправильный формат почты";
                    return View();
                }
                rx = new Regex(@"^[A-ZА-Я]{1}[a-zА-я]{0,20}$");
                if (!rx.IsMatch(modelRegistration.nameOfUser))
                {
                    ViewData["ValidateMessage"] = "Неправильный формат имени";
                    return View();
                }
                rx = new Regex(@"^[A-ZА-Я]{1}[a-zА-я]{0,20}$");
                if (!rx.IsMatch(modelRegistration.secName))
                {
                    ViewData["ValidateMessage"] = "Неправильный формат отчества";
                    return View();
                }
                rx = new Regex(@"^[A-ZА-Я]{1}[a-zА-я]{0,20}\-{0,1}[A-ZА-Я]{0,1}[a-zА-я]{0,20}$");
                if (!rx.IsMatch(modelRegistration.surname))
                {
                    ViewData["ValidateMessage"] = "Неправильный формат фамилии";
                    return View();
                }
                rx = new Regex(@"^[A-Za-z0-9]{1,30}$");
                if (!rx.IsMatch(modelRegistration.pass))
                {
                    ViewData["ValidateMessage"] = "В пароле можно использовать только латинские буквы и цифры";
                    return View();
                }
                if (modelRegistration.pass != modelRegistration.passRepeat)
                {
                    ViewData["ValidateMessage"] = "Пароли не совпадают!";
                    return View();
                }

                string file = $"{modelRegistration.nickName}{Path.GetExtension(uploadedFile.FileName)}";
                string path = "/pic/" + file;
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }


                SQLQuery = $"INSERT INTO _User (NickName, Pass, email, NameOfUser, Surname, SecName, Phone, RoleOfUser, Photo) VALUES " +
                    $"(@nick, @pass, @email, @name, @surname," +
                    $"@secname,@phone,@role,@path);";


                using (var connect = new SqlConnection(_connection.SQLString))
                {
                    await connect.OpenAsync();
                    SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                    cmd.Parameters.AddWithValue("@nick", $"{modelRegistration.nickName.ToLower()}");
                    cmd.Parameters.AddWithValue("@pass", $"{HashPassword(modelRegistration.pass)}");
                    cmd.Parameters.AddWithValue("@email", $"{modelRegistration.email.ToLower()}");
                    cmd.Parameters.AddWithValue("@name", $"{modelRegistration.nameOfUser}");
                    cmd.Parameters.AddWithValue("@surname", $"{modelRegistration.surname}");
                    cmd.Parameters.AddWithValue("@secname", $"{modelRegistration.secName}"); ;
                    cmd.Parameters.AddWithValue("@phone", $"{modelRegistration.phoneNumber}");
                    cmd.Parameters.AddWithValue("@role", $"{modelRegistration.roleOfUser}");
                    cmd.Parameters.AddWithValue("@path", $"{file}");
                    await cmd.ExecuteNonQueryAsync();
                }

                return RedirectToAction("Login", "Access");
            }
            ViewData["ValidateMessage"] = "Неправильные поля";
            return View();
        }

    }
}
