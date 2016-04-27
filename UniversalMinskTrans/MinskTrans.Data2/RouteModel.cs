using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans.Data2
{
    class RouteModel
    {
    }

    struct TimeLine
    {

        public List<Time> Line { get; set; } 
    }

    struct Time
    {
        public byte Hour { get; set; }
        public byte Minutes { get; set; }
    }
}
