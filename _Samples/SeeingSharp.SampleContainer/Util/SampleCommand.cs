using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.SampleContainer.Util
{
    public class SampleCommand : DelegateCommand
    {
        public SampleCommand(string commandText, Action execute, Func<bool> canExecute)
            : base(execute, canExecute)
        {
            this.CommandText = commandText;
        }

        public string CommandText
        {
            get;
            private set;
        }
    }
}
