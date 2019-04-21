using SeeingSharp.Util;

namespace SeeingSharpModelViewer
{
    public interface ICommonDialogsViewService : IViewService
    {
        ResourceLink PickFileByDialog(string fileFilter);
    }
}