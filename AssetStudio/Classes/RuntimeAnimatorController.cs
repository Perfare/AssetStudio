using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class RuntimeAnimatorController : NamedObject
    {
        protected RuntimeAnimatorController(ObjectReader reader) : base(reader)
        {

        }
    }
}
