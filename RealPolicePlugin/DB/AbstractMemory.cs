using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.DB
{
    abstract class AbstractMemory<E> : IMemoryStorage<E>
    {

        protected List<E> _Elements = new List<E>();


        public void DeleteElement(E element)
        {
            if (this.Exists(element))
            {
                this._Elements.Remove(element); 
            }
        }

        public bool Exists(E element)
        {
            return this._Elements.Contains(element);
        }

        public void SetElement(E element)
        {
            if(this.Exists(element) == false)
            {
                this._Elements.Add(element); 
            }
        }
    }
}
