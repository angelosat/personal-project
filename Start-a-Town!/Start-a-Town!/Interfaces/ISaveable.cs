using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public interface ISaveable 
    {
        SaveTag Save(string name = "");
        ISaveable Load(SaveTag tag);
    }
}
