namespace Louvre.Shared.Models
{
    public enum UserTypes
    {
        //SuperAdmin = 1,
        //Administrator = 2,
        //FacilityCoordinator=3,
        //LogisticDepartment=4,
        //DisposalCommitte=5,
        //ControlGroup=6,
        //SSLSecurity=7,
        //EntranceSecurity=8,
        Company = 8,
        Individual = 9,
        Employee = 10
    }
    public enum LocationTypes
    {
        DeliveryPoint = 1,
        DropPoint = 2,
        Storage = 3
    }

    public enum AddressType
    {
        Permenent = 1,
        Communication = 2,
        Office = 3
    }
    public enum Genders
    {
        Male = 1,
        Female = 2,
        Transgender = 3
    }
    public enum MaritalStatuses
    {
        Single = 1,
        Married = 2,
        Seperated = 3,
        Widowed = 4
    }
    public enum BloodGroups
    {
        APositive = 1,
        ANegative = 2,
        BPositive = 3,
        BNegative = 4,
        OPositive = 5,
        ONegative = 6,
        ABPositive = 7,
        ABNegative = 8,
    }
}
