using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient.Model
{
    public class OvoResponse<T>
    {
        public String Status { get; set; }
        public String Code { get; set; }
        public T Data { get; set; }
    }
}
