using LayerDAL.Repository;
using LayerDAL.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using LayerDAL.Repository;
using LayerDAL.Entitites;
using LayerDAL.Settings;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WildDenis.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace WildDenis.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IDBRepository _repository;
        IWebHostEnvironment _appEnvironment;
        private readonly ConnectionSetting _connection;

        public ProfileController(IDBRepository repository, IOptions<ConnectionSetting> connection, IWebHostEnvironment appEnvironment)
        {
            _repository = repository;
            _connection = connection.Value;
            _appEnvironment = appEnvironment;
        }
        [HttpPost]
        public async Task<IActionResult> SaveProfileChanges(User user, IFormFile uploadedFile, string loginOfUser)
        {
            if( user == null)
            {
                TempData["ValidateMessage"] = "Вы допустили ошибку в заполнении";
                return RedirectToAction("ChangeShow", "Profile");
            }
            Regex rx = new Regex(@"^\+\d{3}\(\d{2}\)\d{3}\-\d{2}\-\d{2}$");
            if (!rx.IsMatch(user.Phone))
            {
                TempData["ValidateMessage"] = "Неправильный формат номера";
                return RedirectToAction("ChangeShow", "Profile");
            }
            rx = new Regex(@"^[a-zA-Z0-9]{1}[a-zA-Z0-9-_]{1,30}[a-zA-Z0-9]{1}\@[a-zA-Z]{1,10}\.[a-zA-Z]{1,10}$");
            if (!rx.IsMatch(user.email))
            {
                TempData["ValidateMessage"] = "Неправильный формат почты";
                return RedirectToAction("ChangeShow", "Profile");
            }
            string file = "";
            string SQLQuery = $"UPDATE _User SET Phone = @phone, email = @email WHERE idOfUser = @id;";


            if (uploadedFile != null)
            {
                file = $"{loginOfUser}{DateTime.Now.Ticks}{Path.GetExtension(uploadedFile.FileName)}";
                string path = "/pic/" + file;
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }

                SQLQuery = $"UPDATE _User SET Phone = @phone, email = @email, Photo = @path WHERE idOfUser = @id;";

            }

            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                cmd.Parameters.AddWithValue("@phone", $"{user.Phone}");
                cmd.Parameters.AddWithValue("@email", $"{user.email.ToLower()}");
                cmd.Parameters.AddWithValue("@id", $"{user.IdOfUser}"); ;
                cmd.Parameters.AddWithValue("@path", $"{file}");
                await cmd.ExecuteNonQueryAsync();
            }

            return RedirectToAction("Profile", "Navigator");

        }

        public async Task<IActionResult> ChangeShow()
        {
            string nick = User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ')[User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ').Length - 1];
            User Yu = await _repository.GetUserByNick(nick);
            return View(Yu);
        }

        public async Task<IActionResult> ResumeAdd()
        {
            string SQLQuery = $"SELECT NameOfSkill FROM _Skills;";
            StringBuilder stringOfSkills = new StringBuilder();
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        stringOfSkills.Append($"{reader["NameOfSkill"].ToString()}, ");
                    }
                }
            }
            if (stringOfSkills.Length >= 2) stringOfSkills.Remove(stringOfSkills.Length - 2, 2);
            TempData["StringOfSkills"] = stringOfSkills;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResumeAdd(ResumeAdding resumeAdding)
        {
            if (ModelState.IsValid)
            {
                string nick = User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ')[User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ').Length - 1];

                string SQLQuery = $"INSERT INTO _Resume (idOfUser, VacansyName, AboutMe, Skills) VALUES " +
                       $"((SELECT idOfUser FROM _User WHERE NickName = @nick),@vacname,@aboutme,@skills);";

                using (var connect = new SqlConnection(_connection.SQLString))
                {
                    await connect.OpenAsync();
                    SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                    cmd.Parameters.AddWithValue("@nick", $"{nick}");
                    cmd.Parameters.AddWithValue("@vacname", $"{resumeAdding.vacansyName}");
                    cmd.Parameters.AddWithValue("@aboutme", $"{resumeAdding.aboutMe}");
                    cmd.Parameters.AddWithValue("@skills", $"{resumeAdding.skills}");

                    await cmd.ExecuteNonQueryAsync();
                }

                return RedirectToAction("Profile", "Navigator");
            }
            ViewData["ValidateMessage"] = "Заполните поля";
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResumeShow(ResumeShowing resumeShowing)
        {
            string nick = User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ')[User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ').Length - 1];


            string SQLQuery = $@"
                    SELECT
                        _Resume.idOfResume,
                        _User.NameOfUser,
                        _User.Surname,
                        _User.SecName,
                        _User.Phone,
                        _User.Photo,
                        _Resume.VacansyName,
                        _Resume.AboutMe,
                        _Resume.Skills,
                        _User.email,
                        (
                            SELECT COALESCE(STRING_AGG(NameOfSkill, ', '), '') AS ProovedSkills
                            FROM
                            (
                                SELECT _Skills.NameOfSkill
                                FROM _TestResults
                                INNER JOIN _Tests ON _TestResults.idOfTest = _Tests.idOfTest
                                INNER JOIN _Skills ON _Skills.idOfSkill = _Tests.idOfSkill
                                WHERE _TestResults.idOfUser = _User.idOfUser
                                AND (_TestResults.idOfUser = (SELECT idOfUser FROM _User WHERE NickName = @nick) AND _TestResults.Percents >= 90)
                                GROUP BY _Skills.NameOfSkill
                            ) AS t
                        ) AS ProovedSkills
                    FROM _Resume
                    INNER JOIN _User ON _Resume.idOfUser = _User.idOfUser
                    WHERE _Resume.idOfUser = (SELECT idOfUser FROM _User WHERE NickName = @nick);
                    ";

            List<ResumeShowing> Resumes = new List<ResumeShowing>();
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                cmd.Parameters.AddWithValue("@nick", $"{nick}");
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Resumes.Add(new ResumeShowing()
                        {
                            idOfResume = Convert.ToInt32(reader["idOfResume"]),
                            NameOfUser = reader["NameOfUser"].ToString(),
                            Surname = reader["Surname"].ToString(),
                            SecName = reader["SecName"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            Photo = reader["Photo"].ToString(),
                            vacansyName = reader["VacansyName"].ToString(),
                            aboutMe = reader["AboutMe"].ToString(),
                            defaultSkills = reader["Skills"].ToString().Split(", ").ToList().Except(reader["ProovedSkills"].ToString().Split(", ").ToList()).ToList(),
                            proovedSkills = reader["ProovedSkills"].ToString().Split(", ").ToList(),
                            email = reader["email"].ToString()
                        });
                    }
                }

            }

            return View(Resumes);
        }

        public async Task<IActionResult> VacansyAdd()
        {
            string SQLQuery = $"SELECT NameOfSkill FROM _Skills;";
            StringBuilder stringOfSkills = new StringBuilder();
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        stringOfSkills.Append($"{reader["NameOfSkill"].ToString()}, ");
                    }
                }
            }
            stringOfSkills.Remove(stringOfSkills.Length - 2, 2);
            TempData["StringOfSkills"] = stringOfSkills;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VacansyAdd(VacansyAdding vacansyAdding)
        {
            if (ModelState.IsValid)
            {
                string nick = User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ')[User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ').Length - 1];

                string SQLQuery = $"INSERT INTO _Vacansy (idOfUser, VacansyName, AboutVacansy, Skills, PublDate, Salary) VALUES " +
                       $"((SELECT idOfUser FROM _User WHERE NickName = @nick),@vacname,@aboutvac,@skills, GETDATE(), @salary);";

                using (var connect = new SqlConnection(_connection.SQLString))
                {
                    await connect.OpenAsync();
                    SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                    cmd.Parameters.AddWithValue("@nick", $"{nick}");
                    cmd.Parameters.AddWithValue("@vacname", $"{vacansyAdding.vacansyName}");
                    cmd.Parameters.AddWithValue("@aboutvac", $"{vacansyAdding.aboutVacansy}");
                    cmd.Parameters.AddWithValue("@skills", $"{vacansyAdding.skills}");
                    cmd.Parameters.AddWithValue("@salary", $"{vacansyAdding.salary}");
                    await cmd.ExecuteNonQueryAsync();
                }

                return RedirectToAction("Profile", "Navigator");
            }
            ViewData["ValidateMessage"] = "Заполните поля";
            return View();
        }


        public IActionResult VacansyShow()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VacansyShow(VacansyShowing vacansyShowing)
        {
            string nick = User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ')[User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ').Length - 1];


            string SQLQuery = $"SELECT _Vacansy.idOfVacansy, _Vacansy.Salary, _User.NameOfUser, _User.Surname, _User.SecName, _User.Phone, _Vacansy.VacansyName, _Vacansy.AboutVacansy, _Vacansy.Skills, _User.email " +
                $"FROM _Vacansy INNER JOIN " +
                $"_User ON _Vacansy.idOfUser = _User.idOfUser " +
                $"WHERE _Vacansy.idOfUser = (SELECT idOfUser FROM _User WHERE NickName = @nick);";

            List<VacansyShowing> Vacansys = new List<VacansyShowing>();
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                cmd.Parameters.AddWithValue("@nick", $"{nick}");
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Vacansys.Add(new VacansyShowing()
                        {
                            idOfVacansy = Convert.ToInt32(reader["idOfVacansy"]),
                            salary = Convert.ToInt32(reader["Salary"]),
                            NameOfUser = reader["NameOfUser"].ToString(),
                            Surname = reader["Surname"].ToString(),
                            SecName = reader["SecName"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            vacansyName = reader["VacansyName"].ToString(),
                            aboutVacansy = reader["AboutVacansy"].ToString(),
                            skills = reader["Skills"].ToString().Split(", ").ToList(),
                            email = reader["email"].ToString()
                        });
                    }
                }

            }

            return View(Vacansys);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVacansy(int? id)
        {
            if (Request.Form["confirmation"] != "true")
            {
                return RedirectToAction("Profile", "Navigator");
            }
            if (id == null)
            {
                return RedirectToAction("Profile", "Navigator");
            }
            string SQLQuery = $"DELETE FROM _Vacansy WHERE idOfVacansy = {id}";
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);

                await cmd.ExecuteNonQueryAsync();
            }
            return RedirectToAction("Profile", "Navigator");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteResume(int? id)
        {
            if (Request.Form["confirmation"] != "true")
            {
                return RedirectToAction("Profile", "Navigator");
            }
            if (id == null)
            {
                return RedirectToAction("Profile", "Navigator");
            }
            string SQLQuery = $"DELETE FROM _Resume WHERE idOfResume = {id}";
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);

                await cmd.ExecuteNonQueryAsync();
            }
            return RedirectToAction("Profile", "Navigator");
        }


    }
}
