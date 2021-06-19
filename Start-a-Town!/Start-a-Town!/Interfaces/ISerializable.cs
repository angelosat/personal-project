using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Start_a_Town_
{
    public interface ISerializable
    {
        void Write(BinaryWriter w);
        ISerializable Read(BinaryReader r);
    }
}
