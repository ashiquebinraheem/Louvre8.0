using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    [TableName("PersonalInfoAddress")]
    public class PersonalInfoAddress : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? AddressID { get; set; }
        public int? PersonalInfoID { get; set; }
        public int AddressType { get; set; }
        public string? House { get; set; }
        public string? Area { get; set; }
        public string? Post { get; set; }
        public string? Pincode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public int? CountryID { get; set; }
        public string? FullAddress { get; set; }
    }


    [TableName("PersonalInfoAddress")]
    public class PersonalInfoAddress_Client : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? AddressID { get; set; }
        public int? PersonalInfoID { get; set; }
        public string? FullAddress { get; set; }
        public int AddressType { get; set; } = 1;

    }
}
