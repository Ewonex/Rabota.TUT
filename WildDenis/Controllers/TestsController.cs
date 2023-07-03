using LayerDAL.Entitites;
using LayerDAL.Repository;
using LayerDAL.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WildDenis.Models;

namespace WildDenis.Controllers
{
    public class TestsController : Controller
    {
        private readonly IDBRepository _repository;
        private readonly ConnectionSetting _connection;

        public TestsController(IDBRepository repository, IOptions<ConnectionSetting> connection)
        {
            _repository = repository;
            _connection = connection.Value;
        }

        public async Task<IActionResult> CreateTestShow()
        {
            string SQLQuery = @"SELECT NameOfSkill
                                    FROM _Skills
                                    WHERE idOfSkill NOT IN (SELECT idOfSkill FROM _Tests);";

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
        public async Task<IActionResult> CreateTest(string pickedSkill, string testName, string testDescription)
        {
            string SQLQuery = $"INSERT INTO _Tests(idOfSkill, DescriptionOfTest, NameOfTest) " +
                                    $"VALUES((SELECT idOfSkill FROM _Skills WHERE NameOfSkill = @skill), @descript, @tname); ";



            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                cmd.Parameters.AddWithValue("@skill", $"{pickedSkill}");
                cmd.Parameters.AddWithValue("@descript", $"{testDescription}");
                cmd.Parameters.AddWithValue("@tname", $"{testName}");
                await cmd.ExecuteNonQueryAsync();
            }

            TempData["PickedSkill"] = pickedSkill;
            TempData["TestName"] = testName;


            return RedirectToAction("OpenListOfTests", "Tests");
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(int id, string nameOfTest)
        {
            TempData["TestName"] = nameOfTest;
            TempData["idOfTest"] = id;

            return View();
        }

        public async Task<IActionResult> OpenListOfTests()
        {
            string SQLQuery = $"SELECT s1.idOfTest, NameOfSkill, DescriptionOfTest, NameOfTest, COUNT(_Questions.idOfTest) AS questionCount " +
                $"FROM (SELECT _Tests.idOfTest, _Skills.NameOfSkill, _Tests.DescriptionOfTest, _Tests.NameOfTest " +
                $"FROM _Tests INNER JOIN _Skills ON _Tests.idOfSkill = _Skills.idOfSkill) s1 " +
                $"LEFT JOIN _Questions ON s1.idOfTest = _Questions.idOfTest " +
                $"GROUP BY s1.idOfTest, NameOfSkill, DescriptionOfTest, NameOfTest;";


            List<TestShowing> Tests = new List<TestShowing>();
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Tests.Add(new TestShowing()
                        {
                            idOfTest = Convert.ToInt32(reader["idOfTest"]),
                            nameOfSkill = reader["NameOfSkill"].ToString(),
                            description = reader["DescriptionOfTest"].ToString(),
                            nameOfTest = reader["NameOfTest"].ToString(),
                            countOfQuestions = Convert.ToInt32(reader["questionCount"])
                        });
                    }
                }
            }
            return View(Tests);
        }

        public async Task<IActionResult> OpenTestChanging(int id)
        {
            PickedTest test = await FindTestById(id);
            return View(test);
        }

        public async Task<IActionResult> DeleteQuestion(int id)
        {
            string SQLQuery = $"DELETE FROM _Questions WHERE idOfQuestion = {id};";
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);

                await cmd.ExecuteNonQueryAsync();
            }
            return RedirectToAction("OpenListOfTests", "Tests");
        }

        public async Task<IActionResult> AddQuestionConfirming(string hiddenField, int correctAnswer, int idOfTest)
        {
            List<string> question = hiddenField.Split("~").ToList();
            question.RemoveAt(0);
            StringBuilder questionStringSB = new StringBuilder();
            for (int i = 1; i < question.Count; i++)
            {
                questionStringSB.Append($"{i}. {question[i]}~");
            }

            string SQLQuery = $"INSERT INTO _Questions(idOfTest, TextOfQuestion, Answers, CorrectAnswer) " +
                $"VALUES (@id,@question0, @questionstring, @correctAnsw);";

            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                cmd.Parameters.AddWithValue("@id", $"{idOfTest}");
                cmd.Parameters.AddWithValue("@question0", $"{question[0]}");
                cmd.Parameters.AddWithValue("@questionstring", $"{questionStringSB}");
                cmd.Parameters.AddWithValue("@correctAnsw", $"{correctAnswer}");
                await cmd.ExecuteNonQueryAsync();
            }


            return RedirectToAction("OpenListOfTests", "Tests");
        }

        public async Task<IActionResult> TestPassingShow()
        {
            string SQLQuery = $"SELECT s1.idOfTest, NameOfSkill, DescriptionOfTest, NameOfTest, COUNT(_Questions.idOfTest) AS questionCount " +
                $"FROM (SELECT _Tests.idOfTest, _Skills.NameOfSkill, _Tests.DescriptionOfTest, _Tests.NameOfTest " +
                $"FROM _Tests INNER JOIN _Skills ON _Tests.idOfSkill = _Skills.idOfSkill) s1 " +
                $"LEFT JOIN _Questions ON s1.idOfTest = _Questions.idOfTest " +
                $"GROUP BY s1.idOfTest, NameOfSkill, DescriptionOfTest, NameOfTest;";


            List<TestShowing> Tests = new List<TestShowing>();
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Tests.Add(new TestShowing()
                        {
                            idOfTest = Convert.ToInt32(reader["idOfTest"]),
                            nameOfSkill = reader["NameOfSkill"].ToString(),
                            description = reader["DescriptionOfTest"].ToString(),
                            nameOfTest = reader["NameOfTest"].ToString(),
                            countOfQuestions = Convert.ToInt32(reader["questionCount"])
                        });
                    }
                }
            }
            return View(Tests);
        }

        public async Task<IActionResult> DeleteTest(int id)
        {

            if (Request.Form["confirmation"] != "true")
            {
                return RedirectToAction("Profile", "Navigator");
            }
            string SQLQuery = $"DELETE FROM _Tests WHERE idOfTest = {id};";
            Console.WriteLine(SQLQuery);
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);

                await cmd.ExecuteNonQueryAsync();
            }
            return RedirectToAction("OpenListOfTests", "Tests");
        }

        public async Task<PickedTest> FindTestById(int id)
        {
            PickedTest test = new PickedTest();
            string SQLQuery = $"SELECT _Tests.idOfTest,_Skills.NameOfSkill,_Tests.DescriptionOfTest, _Tests.NameOfTest FROM _Tests INNER JOIN _Skills ON _Tests.idOfSkill=_Skills.idOfSkill WHERE idOfTest = {id};";
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        test.idOfTest = Convert.ToInt32(reader["idOfTest"]);
                        test.NameOfSkill = reader["NameOfSkill"].ToString();
                        test.description = reader["DescriptionOfTest"].ToString();
                        test.nameOfTest = reader["NameOfTest"].ToString();
                    }
                }
            }

            SQLQuery = $"SELECT * FROM _Questions WHERE idOfTest = {id};";
            List<Question> questions = new List<Question>();
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        List<string> answ = reader["Answers"].ToString().Split("~").ToList();
                        answ.RemoveAt(answ.Count()-1);
                        questions.Add(new Question()
                        {
                            idOfQuestion = Convert.ToInt32(reader["idOfQuestion"]),
                            idOfTest = Convert.ToInt32(reader["idOfTest"]),
                            textOfQuestion = reader["TextOfQuestion"].ToString(),
                            answers = answ,
                            correctAnswer = Convert.ToInt32(reader["CorrectAnswer"])
                        }) ;
                    }
                }
            }
            test.questions = questions;
            return test;
        }

        public async Task<IActionResult> OpenTestInfo(int id, int countOfQuestions)
        {
            string SQLQuery = $"SELECT DATEDIFF(DAY, " +
            $"(SELECT TOP 1 DateOfPass FROM _TestResults WHERE idOfTest = {id} AND idOfUser = (SELECT idOfUser FROM _User WHERE NickName = '{TempData["login"]}') " +
            $"ORDER BY DateOfPass DESC), GETDATE()) AS Difference;";

            int dif = 1;
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (reader["Difference"] != DBNull.Value)
                        {
                            dif = Convert.ToInt32(reader["Difference"]);
                        }
                        
                    }
                }
            }
            if (dif == 0)
            {
                TempData["TestInfo"] = "Сегодня вы уже проходили этот тест.";
            }
            if(countOfQuestions == 0)
            {
                TempData["TestInfo"] = "В тесте нет вопросов.";
            }

            PickedTest test = await FindTestById(id);
            return View(test);
        }

        public async Task<IActionResult> StartTestPassing(int idOfTest)
        {
            PickedTest test = await FindTestById(idOfTest);
            return View(test);
        }

        public async Task<IActionResult> ShowResults (int idOfTest, string hiddenField)
        {
            PickedTest test = await FindTestById(idOfTest);
            List<string> answers = hiddenField.Split('~').ToList();
            int correctAnswers = 0;
            for(int i = 0; i < answers.Count(); i++)
            {
                if (answers[i] == test.questions[i].correctAnswer.ToString())
                {
                    correctAnswers++;
                }
            }
            int percents = correctAnswers * 100 / answers.Count();
            TempData["ResultPercents"] = percents;
            TempData["NameOfTest"] = test.nameOfTest;
            TempData["Skill"] = test.NameOfSkill;

            string SQLQuery = $"INSERT INTO _TestResults(idOfTest, idOfUser, DateOfPass, Percents) " +
                $"VALUES ({idOfTest},(SELECT idOfUser FROM _User WHERE NickName = '{TempData["login"]}'),GETDATE(),{percents});";

            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                await cmd.ExecuteNonQueryAsync();
            }

            return View();
        }

        public IActionResult CreateSkill()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateSkill(string pickedSkill)
        {
            string SQLQuery = $"SELECT * FROM _Skills WHERE NameOfSkill = @skill;";

            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                cmd.Parameters.AddWithValue("@skill", $"{pickedSkill}");
                var reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    TempData["ValidateMessage"] = "Данный навык уже существует";
                    return RedirectToAction("CreateSkill", "Tests");
                }
            }

            SQLQuery = $"INSERT INTO _Skills(NameOfSkill) " +
                                    $"VALUES(@skill); ";

            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                cmd.Parameters.AddWithValue("@skill", $"{pickedSkill}");

                await cmd.ExecuteNonQueryAsync();
            }

            return RedirectToAction("TesterPage", "Navigator");
        }

        public async Task<IActionResult> SaveChanges(string nameOfTest, string description, string id)
        {
            string SQLQuery = $"UPDATE _Tests SET DescriptionOfTest = @description, NameOfTest = @tname WHERE idOfTest = @id;";

            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                cmd.Parameters.AddWithValue("@description", $"{description}");
                cmd.Parameters.AddWithValue("@tname", $"{nameOfTest}");
                cmd.Parameters.AddWithValue("@id", $"{id}"); ;
                await cmd.ExecuteNonQueryAsync();
            }

            return RedirectToAction("OpenListOfTests", "Tests");
        }

    }
}
