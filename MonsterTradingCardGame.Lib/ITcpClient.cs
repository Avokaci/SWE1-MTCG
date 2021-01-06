using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MTCG.Lib
{
    public interface ITcpClient
    {
        StreamWriter sw();
        StreamReader sr();      
        void End();

    }
}
