using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace SeeingSharpModelViewer
{
    public class IocObjectExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var requType = this.RequestedType;
            if(requType == null) { return null; }

            return SimpleIoc.Default.GetInstance(requType);
        }

        public Type RequestedType
        {
            get;
            set;
        }
    }
}
