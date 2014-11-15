using System;
using System.Collections.Generic;
using System.Text;

namespace DarkMultiPlayerCommon
{
    public interface IMessageFactory
    {
        IMessage Create(ushort opcode);
    }
}
