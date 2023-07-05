
namespace Lynx.UI.PasteWizard
{
    public interface ITransformViewModelHost
    {
        void Updated(TransformViewModel vm);
        void Delete(TransformViewModel vm);
    }
}
