using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class VisitRequestDrink : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? VisitRequestDrinkID { get; set; }
        public int? VisitRequestID { get; set; }
        public int? DrinkID { get; set; }
    }
}
