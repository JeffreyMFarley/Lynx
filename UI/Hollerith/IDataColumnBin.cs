using System.Data;

namespace Lynx.UI.Hollerith
{
    public interface IDataColumnBin
    {
        DataColumn Column { get; set; }

        string ValueAsString { get; set; }
    }
}
