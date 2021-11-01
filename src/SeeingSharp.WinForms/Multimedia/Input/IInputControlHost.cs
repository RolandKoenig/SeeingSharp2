using System.Windows.Forms;

namespace SeeingSharp.Multimedia.Input
{
    public interface IInputControlHost
    {
        Control GetWinFormsInputControl();
    }
}