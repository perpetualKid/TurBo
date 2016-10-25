using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Base.Categories
{
    public abstract class StorageControllable: Controllable
    {
        public StorageControllable(string componentName): base(componentName)
        {
        }

        protected abstract Task ConnectStorage(MessageContainer data);

        protected abstract Task DisconnectStorage(MessageContainer data);

        protected abstract Task ListContent(MessageContainer data);

    }
}
