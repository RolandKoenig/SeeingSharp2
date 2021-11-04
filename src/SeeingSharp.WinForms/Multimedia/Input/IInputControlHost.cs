using System.Windows.Forms;

namespace SeeingSharp.Input
{
    public interface IInputControlHost
    {
        Control GetWinFormsInputControl();
    }
}