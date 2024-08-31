using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GimJem.Core
{
    public interface IController<T>
    {
        void Init(T manager);
    }

}
