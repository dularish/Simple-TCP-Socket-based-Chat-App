using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketFrm
{
    public enum CommandType { DisplayTextToConsole}
    interface ICommand
    {
        CommandType CommandType { get; }

    }
}
