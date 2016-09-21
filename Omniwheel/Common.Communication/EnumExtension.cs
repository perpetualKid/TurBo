using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Communication
{
    public static class EnumExtension<T>
    {
        public static T[] All()
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }

}
