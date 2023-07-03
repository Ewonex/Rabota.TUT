using LayerDAL.Entitites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayerDAL.Repository
{
    public interface IDBRepository
    {
        Task<List<User>> GetUsers();

        Task<User> GetUserByNick(string NickName);
    }
}
