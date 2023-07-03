using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LayerDAL.Settings;
using LayerDAL.Entitites;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;


namespace LayerDAL.Repository
{
    public class DBRepository : IDBRepository
    {
        private readonly ConnectionSetting _connection;

        public DBRepository(IOptions<ConnectionSetting> connection)
        {
            _connection = connection.Value;
        }

        public async Task<List<User>> GetUsers()
        {

            List<User> ListUsers = new List<User>();


            string SQLQuery = $"SELECT * FROM _User;";
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ListUsers.Add(new User()
                        {
                            IdOfUser = Convert.ToInt32(reader["IdOfUser"]),
                            NickName = reader["NickName"].ToString(),
                            Pass = reader["Pass"].ToString(),
                            email = reader["email"].ToString(),
                            NameOfUser = reader["NameOfUser"].ToString(),
                            Surname = reader["Surname"].ToString(),
                            SecName = reader["SecName"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            RoleOfUser = reader["RoleOfUser"].ToString(),
                            Photo = reader["Photo"].ToString()
                        });

                    }
                }
            }
            return ListUsers;
        }







        public async Task<User> GetUserByNick(string NickName)
        {

            User Userok = new User();


            string SQLQuery = $"SELECT * FROM _User WHERE NickName = '{NickName}';";
            using (var connect = new SqlConnection(_connection.SQLString))
            {
                await connect.OpenAsync();
                SqlCommand cmd = new SqlCommand(SQLQuery, connect);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        User Userok2 = new User()
                        {
                            IdOfUser = Convert.ToInt32(reader["IdOfUser"]),
                            NickName = reader["NickName"].ToString(),
                            Pass = reader["Pass"].ToString(),
                            email = reader["email"].ToString(),
                            NameOfUser = reader["NameOfUser"].ToString(),
                            Surname = reader["Surname"].ToString(),
                            SecName = reader["SecName"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            RoleOfUser = reader["RoleOfUser"].ToString(),
                            Photo = reader["Photo"].ToString()
                        };
                        Userok = Userok2;
                    }
                }
            }
            return Userok;
        }
    
    
    
    
    
    }
}
