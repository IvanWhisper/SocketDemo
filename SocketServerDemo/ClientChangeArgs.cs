using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServerDemo
{
    public class ClientChangeArgs
    {
        /// <summary>
        /// 客户端名
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// 改变代号
        /// </summary>
        public int ChangeCode { get; set; } 
    }
}
