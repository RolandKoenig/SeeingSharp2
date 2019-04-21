using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharpModelViewer
{
    public class QueryForViewServiceMessage
    {
        public QueryForViewServiceMessage(Type viewServiceType)
        {
            this.ViewServiceType = viewServiceType;
        }

        public Type ViewServiceType
        {
            get;
        }

        public IViewService ViewService
        {
            get;
            set;
        }
    }
}
