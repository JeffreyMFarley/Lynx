using Esoteric.UI;
using Lynx.Models;

namespace Lynx.Interfaces
{
    public interface IGenerateSubset : IViewModel, IValidation
    {
        // Properties
        string GeneratorName { get; }
        LinkSet Relationships { get; set; }
        string TableName { get; set; }

        // Members
        LinkSet Generate();
    }
}
