using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WildDenis.Models;
using System.ComponentModel.DataAnnotations;
using LayerDAL.Repository;
using LayerDAL.Entitites;
using LayerDAL.Settings;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using NuGet.Protocol.Plugins;

namespace WildDenis.Controllers
{
    public class SearchController : Controller
    {
        private readonly IDBRepository _repository;
        private readonly ConnectionSetting _connection;


        public SearchController(IDBRepository repository, IOptions<ConnectionSetting> connection)
        {
            _repository = repository;
            _connection = connection.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searching)
        {
            string email = User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ')[User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).ToString().Split(' ').Length - 1];
            User Yu = await _repository.GetUserByNick(email);
            if (Yu.RoleOfUser == "Работодатель")
            {
                return RedirectToAction("SearchRes", "Search", new { searching = searching });
            }
            else if (Yu.RoleOfUser == "Соискатель")
            {
                return RedirectToAction("SearchVac", "Search", new { searching = searching });
            }
            return RedirectToAction("MainPage", "Navigator");

        }

        [HttpGet]
        public async Task<IActionResult> SearchRes(string searching)
        {
            string 
            SQLQuery = $@"
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
                                AND (_Resume.AboutMe LIKE '%%' OR _Resume.Skills LIKE '%%')
                                AND _TestResults.Percents >= 90
                                GROUP BY _Skills.NameOfSkill
                            ) AS t
                        ) AS ProovedSkills
                    FROM _Resume
                    INNER JOIN _User ON _Resume.idOfUser = _User.idOfUser
                    WHERE _Resume.AboutMe LIKE '%%' OR _Resume.Skills LIKE '%%'
					ORDER BY LEN(
						(
							SELECT COALESCE(STRING_AGG(NameOfSkill, ', '), '') AS ProovedSkills
							FROM
							(
								SELECT _Skills.NameOfSkill
								FROM _TestResults
								INNER JOIN _Tests ON _TestResults.idOfTest = _Tests.idOfTest
								INNER JOIN _Skills ON _Skills.idOfSkill = _Tests.idOfSkill
								WHERE _TestResults.idOfUser = _User.idOfUser 
								AND (_Resume.AboutMe LIKE '%%' OR _Resume.Skills LIKE '%%')
								AND _TestResults.Percents >= 90
								GROUP BY _Skills.NameOfSkill
							) AS t
						)
					) DESC;
                    ";


            List<ResumeShowing> Resumes = new List<ResumeShowing>();
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                cmd.Parameters.AddWithValue("@searching", $"%{searching}%");

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
                            defaultSkills = reader["Skills"].ToString().Split(", ").ToList(),
                            proovedSkills = reader["ProovedSkills"].ToString().Split(", ").ToList(),
                            email = reader["email"].ToString()
                        });
                    }
                }
            }
            return View(Resumes);
        }

        [HttpGet]
        public async Task<IActionResult> SearchVac(string searching)
        {
            string SQLQuery = $"SELECT _Vacansy.idOfVacansy, _Vacansy.Salary, _User.NameOfUser, _User.Surname, _User.SecName, _User.Phone, _Vacansy.VacansyName, _Vacansy.AboutVacansy, _Vacansy.Skills, _User.email " +
                            $"FROM _Vacansy INNER JOIN _User ON _Vacansy.idOfUser = _User.idOfUser " +
                            $"WHERE _Vacansy.AboutVacansy LIKE @searching OR _Vacansy.Skills LIKE @searching; ";
            List<VacansyShowing> Vacansys = new List<VacansyShowing>();
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                cmd.Parameters.AddWithValue("@searching", $"%{searching}%");
                await connect.OpenAsync();

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

    }
}
