using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LayerDAL.Entitites
{
    public class Resume
    {
        public int IdOfResume { get; set; }
        public int IdOfUser { get; set; }
        string VacansyName { get; set; }
        string AboutMe { get; set; }
        string Skills { get; set; }

    }
}
