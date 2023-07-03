using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayerDAL.Entitites
{
    public class Vacansy
    {
        public int idOfVacansy { get; set; }
        public int idOfUser { get; set; }
        public string VacansyName { get; set; }
        public string AboutVacansy { get; set; }
        public string Skills { get; set; }
    }
}
