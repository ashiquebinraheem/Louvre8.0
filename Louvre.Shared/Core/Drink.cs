using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class Drink : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? DrinkID { get; set; }
        public string? DrinkName { get; set; }
    }
}
