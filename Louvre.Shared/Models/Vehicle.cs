using System.Collections.Generic;

namespace Louvre.Shared.Models
{
    public class VehicleSaveResponseViewModel : BaseResponse
    {
        public List<ViVehicle> Vehicles { get; set; }
        public int? NewVehicleID { get; set; }
    }

    public class VehicleListViewModel
    {
        public int? VehicleID { get; set; }
        public string? RegisterNo { get; set; }
        public string? VehicleTypeName { get; set; }
        public string? VehicleSize { get; set; }
        public string? PlateNo { get; set; }
    }
}
