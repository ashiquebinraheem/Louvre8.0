namespace Louvre.Shared.Models
{
    public class ViPersonalInfo
    {
        public int PersonalInfoID { get; set; }
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? NickName { get; set; }
        public int GenderID { get; set; }
        public string? ProfileImageFileName { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Email1 { get; set; }
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
    }

    public class ViVehicle
    {
        public int VehicleID { get; set; }
        public string? RegisterNo { get; set; }
        public int? VehicleTypeID { get; set; }
        public string? VehicleTypeName { get; set; }
        public string? VehicleSize { get; set; }
        public string? PlateNo { get; set; }
        public int? VehicleMakeID { get; set; }
        public string? VehicleMakeName { get; set; }
        public int? VehiclePlateSourceID { get; set; }
        public string? VehiclePlateSourceName { get; set; }
        public int? VehiclePlateTypeID { get; set; }
        public string? VehiclePlateTypeName { get; set; }
        public string? VehiclePlateCategoryName { get; set; }
        public int? VehiclePlateCategoryID { get; set; }
        public int AddedBy { get; set; }
    }
}

