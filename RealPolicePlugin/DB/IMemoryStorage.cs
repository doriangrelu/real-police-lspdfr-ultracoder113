using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.DB
{
    interface IMemoryStorage<E>
    {

        bool Exists(E element);

        void SetElement(E element);

        void DeleteElement(E element); 

    }
}
