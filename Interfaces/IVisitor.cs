
namespace Lynx.Interfaces
{
    public interface IVisitor<T>
    {
        bool Accept(T instance);
    }
}
