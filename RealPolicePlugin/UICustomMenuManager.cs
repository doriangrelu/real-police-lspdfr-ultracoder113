using RAGENativeUI;
using RealPolicePlugin.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin
{
    class UICustomMenuManager
    {

        private static MenuPool _MenuPool = null;

        public static MenuPool MenuPool
        {
            get
            {
                if (null == _MenuPool)
                {
                    _MenuPool = new MenuPool();
                }
                return _MenuPool;
            }
        }

    }
}
