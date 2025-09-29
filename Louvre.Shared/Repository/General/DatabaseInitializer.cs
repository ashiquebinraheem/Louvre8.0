using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Shared.Repository.General
{
    public interface IDatabaseInitializer
    {
        //Task VersionUpdationAsync(string schema);
        Task InsertDefaultEntries();
    }


    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDbContext _dbContext;
        protected IUserRepository _userRepository;
        private readonly IDbConnection _cn;

        public DatabaseInitializer(IDbContext dbContext, IUserRepository userRepository, IDbConnection cn)
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
            _cn = cn;
        }

        public async Task InsertDefaultEntries()
        {

            #region General Settings
            var dbsettings = await _dbContext.GetAsyncByCondition<GeneralSettings>("SettingsKey=@SettingsKey",new { SettingsKey = "DBVersion" });
            if (dbsettings == null)
            {
                dbsettings = new GeneralSettings()
                {
                    SettingsKey = "DBVersion",
                    SettingsValue = "0"
                };
                dbsettings.SettingsKeyID= await _dbContext.SaveAsync(dbsettings);
            }
            #endregion


            if (dbsettings.SettingsValue == "0")
            {
                await Version1();
                dbsettings.SettingsValue = "1";
            }

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {

                    if (dbsettings.SettingsValue == "11")
                    {
                        await Version12(tran);
                        dbsettings.SettingsValue = "12";
                    }
                    await _dbContext.SaveAsync(dbsettings,tran);
                    tran.Commit();

                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    tran.Rollback();
                }
            }

        }

        public async Task Version1()
        {
            try
            {

                if ((await _dbContext.GetAllAsync<UserType>()).ToList().Count == 0)
                {
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 1, PriorityOrder = 1, UserTypeName = "Super-Admin", DisplayName = "Super Admin", UserNature = 0, ShowInList = false });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 2, PriorityOrder = 2, UserTypeName = "Administrator", DisplayName = "Administrator", UserNature = 0, ShowInList = true });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 3, PriorityOrder = 3, UserTypeName = "Security-Control-Room", DisplayName = "Security Control Room", UserNature = 1, ShowInList = true });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 4, PriorityOrder = 3, UserTypeName = "Logistic-Department", DisplayName = "Logistic Department", UserNature = 1, ShowInList = true });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 5, PriorityOrder = 3, UserTypeName = "Security-Duty-Manager", DisplayName = "Security Duty Manager", UserNature = 1, ShowInList = true });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 6, PriorityOrder = 3, UserTypeName = "Disposal", DisplayName = "Disposal", UserNature = 2, ShowInList = true });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 7, PriorityOrder = 3, UserTypeName = "Registrar", DisplayName = "Registrar", UserNature = 1, ShowInList = true });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 8, PriorityOrder = 5, UserTypeName = "Company", DisplayName = "Company", UserNature = 0, ShowInList = false });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 9, PriorityOrder = 5, UserTypeName = "Individual", DisplayName = "Individual", UserNature = 0, ShowInList = false });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 10, PriorityOrder = 5, UserTypeName = "Employee", DisplayName = "Employee", UserNature = 0, ShowInList = false });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 11, PriorityOrder = 3, UserTypeName = "Security-Managment-Authorization", DisplayName = "Security Managment Authorization", UserNature = 1, ShowInList = true });
                    await _dbContext.NonIdentityInsertAsync(new UserType() { UserTypeID = 12, PriorityOrder = 4, UserTypeName = "Security-Gate", DisplayName = "Security Gate", UserNature = 3, ShowInList = true });
                }

                if ((await _dbContext.GetAllAsync<User>()).ToList().Count == 0)
                {
                    var salt = Guid.NewGuid().ToString("n").Substring(0, 8);
                    var hashPassword = UserRepository.GetHashPassword("admin@2020", salt);
                    var personalInfoId = await _dbContext.SaveAsync<PersonalInfo>(new PersonalInfo() { });

                    await _dbContext.SaveAsync(
                        new User()
                        {
                            UserTypeID = 1,
                            EmailConfirmed = true,
                            LoginStatus = true,
                            Password = hashPassword,
                            Salt = salt,
                            PersonalInfoID = personalInfoId,
                            UserName = "admin"
                        });
                }

                if ((await _dbContext.GetAllAsync<MailSettings>()).ToList().Count == 0)
                {
                    await _dbContext.NonIdentityInsertAsync(new MailSettings()
                    {
                        MailSettingsID = 1,
                        SMTPHost = "plesk3700.is.cc",
                        Port = 587,
                        FromMail = "test@prog-biz.com",
                        FromName = "Louvre",
                        Password = "43fhGr3%",
                        EnableSSL = true
                    });
                }

                if ((await _dbContext.GetAllAsync<Area>()).ToList().Count == 0)
                {
                    var areas = new List<Area>
                    {
                        new Area() { AreaName = "Area 1" },
                        new Area() { AreaName = "Area 2" },
                        new Area() { AreaName = "Area 3" }
                    };
                    await _dbContext.SaveListAsync(areas);
                }

                if ((await _dbContext.GetAllAsync<Branch>()).ToList().Count == 0)
                {
                    var branchId1 = await _dbContext.SaveAsync(new Branch() { BranchName = "Louver Abu Dhbai LLC" });
                    var branchId2 = await _dbContext.SaveAsync(new Branch() { BranchName = "Saadiyat Island" });
                    await _dbContext.SaveAsync(new Branch() { BranchName = "Louver AbuDhabi", ParentBranchID = branchId1 });
                    await _dbContext.SaveAsync(new Branch() { BranchName = "Boutique", ParentBranchID = branchId1 });
                    await _dbContext.SaveAsync(new Branch() { BranchName = "Canteen", ParentBranchID = branchId1 });
                    await _dbContext.SaveAsync(new Branch() { BranchName = "Event", ParentBranchID = branchId1 });
                    await _dbContext.SaveAsync(new Branch() { BranchName = "AFM Exibation ", ParentBranchID = branchId2 });
                }

                if ((await _dbContext.GetAllAsync<Department>()).ToList().Count == 0)
                {
                    var Departments = new List<Department>
                    {
                        new Department() { DepartmentName = "Department 1" },
                        new Department() { DepartmentName = "Department 2" },
                        new Department() { DepartmentName = "Department 3" }
                    };
                    await _dbContext.SaveListAsync(Departments);
                }

                if ((await _dbContext.GetAllAsync<DocumentType>()).ToList().Count == 0)
                {
                    var DocumentTypes = new List<DocumentType>
                    {
                        new DocumentType() { DocumentTypeName = "Emirate ID",DocumentTypeCategoryID=1 },
                        new DocumentType() { DocumentTypeName = "Passport",DocumentTypeCategoryID=1 },
                        new DocumentType() { DocumentTypeName = "Labour Card",DocumentTypeCategoryID=1 },
                        new DocumentType() { DocumentTypeName = "Driving License",DocumentTypeCategoryID=1 },
                        new DocumentType() { DocumentTypeName = "Ejari",DocumentTypeCategoryID=2 },
                        new DocumentType() { DocumentTypeName = "Immegration Card",DocumentTypeCategoryID=2 },
                        new DocumentType() { DocumentTypeName = "Trade License",DocumentTypeCategoryID=2 },
                        new DocumentType() { DocumentTypeName = "Mulkiya",DocumentTypeCategoryID=3 },
                    };
                    await _dbContext.SaveListAsync(DocumentTypes);
                }

                if ((await _dbContext.GetAllAsync<Duration>()).ToList().Count == 0)
                {
                    var Durations = new List<Duration>
                    {
                        new Duration() { DurationName = "30 Mins",Minutes= 30},
                        new Duration() { DurationName = "1 Hour",Minutes= 60},
                        new Duration() { DurationName = "2 Hour",Minutes=120 },
                        new Duration() { DurationName = "15 Mins",Minutes= 15},
                    };
                    await _dbContext.SaveListAsync(Durations);
                }

                if ((await _dbContext.GetAllAsync<EmployeeDesignation>()).ToList().Count == 0)
                {
                    var EmployeeDesignations = new List<EmployeeDesignation>
                    {
                        new EmployeeDesignation() { DesignationName="Marketing Officer" },
                        new EmployeeDesignation() { DesignationName="Manager" },
                        new EmployeeDesignation() { DesignationName="Analyst" },
                        new EmployeeDesignation() { DesignationName="Director" },
                        new EmployeeDesignation() { DesignationName="Executive" },
                    };
                    await _dbContext.SaveListAsync(EmployeeDesignations);
                }



                if ((await _dbContext.GetAllAsync<Location>()).ToList().Count == 0)
                {
                    var Locations = new List<Location>
                    {
                        new Location() { LocationName = "Delivery Point 1",LocationTypeID= 1},
                        new Location() { LocationName = "Delivery Point 2",LocationTypeID= 1},
                        new Location() { LocationName = "Delivery Point 3",LocationTypeID= 1},
                        new Location() { LocationName = "Drop Point 1",LocationTypeID= 2},
                        new Location() { LocationName = "Drop Point 2",LocationTypeID= 2},
                        new Location() { LocationName = "Warehouse 1",LocationTypeID= 3},
                        new Location() { LocationName = "Warehouse 2",LocationTypeID= 3},
                    };
                    await _dbContext.SaveListAsync(Locations);
                }

                if ((await _dbContext.GetAllAsync<Module>()).ToList().Count == 0)
                {
                    await _dbContext.NonIdentityInsertAsync(new Module() { ModuleID = 1, ModuleName = "Material Contractor", PermissionName = "Meterial" });
                    await _dbContext.NonIdentityInsertAsync(new Module() { ModuleID = 2, ModuleName = "Visitor", PermissionName = "Visitor" });
                }

                if ((await _dbContext.GetAllAsync<PackingType>()).ToList().Count == 0)
                {
                    var PackingTypes = new List<PackingType>
                    {
                        new PackingType() { PackingTypeName="PC" },
                        new PackingType() { PackingTypeName="CTN" },
                        new PackingType() { PackingTypeName="Dozen" },
                        new PackingType() { PackingTypeName="Box" },
                    };
                    await _dbContext.SaveListAsync(PackingTypes);
                }

                if ((await _dbContext.GetAllAsync<Purpose>()).ToList().Count == 0)
                {
                    var Purposes = new List<Purpose>
                    {
                        new Purpose() { PurposeName="Sharing Information" },
                        new Purpose() { PurposeName="Making Decisions" },
                        new Purpose() { PurposeName="Creating Solutions" },
                        new Purpose() { PurposeName="Negotiating" },
                    };
                    await _dbContext.SaveListAsync(Purposes);
                }

                if ((await _dbContext.GetAllAsync<RequestMeterialType>()).ToList().Count == 0)
                {
                    await _dbContext.NonIdentityInsertAsync(new RequestMeterialType() { MeterialTypeID = 1, MeterialTypeName = "General" });
                    await _dbContext.NonIdentityInsertAsync(new RequestMeterialType() { MeterialTypeID = 2, MeterialTypeName = "Art" });
                }

                if ((await _dbContext.GetAllAsync<RequestMode>()).ToList().Count == 0)
                {
                    await _dbContext.NonIdentityInsertAsync(new RequestMode() { RequestModeID = 1, ModeName = "Delivery", NeedMeterial = true, IsIn = true, LocationTypeID = 1 });
                    await _dbContext.NonIdentityInsertAsync(new RequestMode() { RequestModeID = 2, ModeName = "Staff Drop off", NeedMeterial = false, IsIn = true, LocationTypeID = 2 });
                    await _dbContext.NonIdentityInsertAsync(new RequestMode() { RequestModeID = 3, ModeName = "Pickup", NeedMeterial = true, IsIn = false, LocationTypeID = 1 });
                }

                if ((await _dbContext.GetAllAsync<RequestType>()).ToList().Count == 0)
                {
                    await _dbContext.NonIdentityInsertAsync(new RequestType() { RequestTypeID = 1, RequestTypeName = "Material In" });
                    await _dbContext.NonIdentityInsertAsync(new RequestType() { RequestTypeID = 2, RequestTypeName = "Art In" });
                    await _dbContext.NonIdentityInsertAsync(new RequestType() { RequestTypeID = 3, RequestTypeName = "Material Out" });
                    await _dbContext.NonIdentityInsertAsync(new RequestType() { RequestTypeID = 4, RequestTypeName = "Art Out" });
                }

                if ((await _dbContext.GetAllAsync<RequestTypeApprovalStage>()).ToList().Count == 0)
                {
                    var RequestTypeApprovalStages = new List<RequestTypeApprovalStage>
                    {
                        new RequestTypeApprovalStage() { RequestTypeID=1,Stage=1,UserTypeID=4 },
                        new RequestTypeApprovalStage() { RequestTypeID=1,Stage=2,UserTypeID= 3},
                        new RequestTypeApprovalStage() { RequestTypeID=2,Stage=1,UserTypeID=7 },
                        new RequestTypeApprovalStage() { RequestTypeID=2,Stage=2,UserTypeID= 3},
                        new RequestTypeApprovalStage() { RequestTypeID=3,Stage=1,UserTypeID= 4},
                        new RequestTypeApprovalStage() { RequestTypeID=3,Stage=2,UserTypeID=6 },
                        new RequestTypeApprovalStage() { RequestTypeID=3,Stage=3,UserTypeID= 3},
                        new RequestTypeApprovalStage() { RequestTypeID=4,Stage=1,UserTypeID= 7},
                        new RequestTypeApprovalStage() { RequestTypeID=4,Stage=2,UserTypeID= 11},
                        new RequestTypeApprovalStage() { RequestTypeID=4,Stage=3,UserTypeID= 3},
                    };
                    await _dbContext.SaveListAsync(RequestTypeApprovalStages);
                }

                if ((await _dbContext.GetAllAsync<VehicleMake>()).ToList().Count == 0)
                {
                    var VehicleMakes = new List<VehicleMake>
                    {
                        new VehicleMake() { VehicleMakeName="Alfa Romeo" },
                        new VehicleMake() { VehicleMakeName="Abarth" },
                        new VehicleMake() { VehicleMakeName="Acura" },
                        new VehicleMake() { VehicleMakeName="Arrinera" },
                        new VehicleMake() { VehicleMakeName="Aixam" },
                        new VehicleMake() { VehicleMakeName="Ariel" },
                        new VehicleMake() { VehicleMakeName="Audi" },
                        new VehicleMake() { VehicleMakeName="Aston Martin" },
                        new VehicleMake() { VehicleMakeName="Bugatti" },
                        new VehicleMake() { VehicleMakeName="Bentley" },
                        new VehicleMake() { VehicleMakeName="BMW" },
                        new VehicleMake() { VehicleMakeName="Buick" },
                        new VehicleMake() { VehicleMakeName="Cadillac" },
                        new VehicleMake() { VehicleMakeName="Chevrolet" },
                        new VehicleMake() { VehicleMakeName="Citroen" },
                        new VehicleMake() { VehicleMakeName="Caterham" },
                        new VehicleMake() { VehicleMakeName="Chrysler" },
                        new VehicleMake() { VehicleMakeName="Corvette" },
                        new VehicleMake() { VehicleMakeName="Dacia" },
                        new VehicleMake() { VehicleMakeName="Dodge" },
                        new VehicleMake() { VehicleMakeName="Daewoo" },
                        new VehicleMake() { VehicleMakeName="Daihatsu" },
                        new VehicleMake() { VehicleMakeName="Elfin" },
                        new VehicleMake() { VehicleMakeName="Fiat" },
                        new VehicleMake() { VehicleMakeName="Ferrari" },
                        new VehicleMake() { VehicleMakeName="Fisker" },
                        new VehicleMake() { VehicleMakeName="Ford" },
                        new VehicleMake() { VehicleMakeName="Gaz" },
                        new VehicleMake() { VehicleMakeName="Geely" },
                        new VehicleMake() { VehicleMakeName="Gillet" },
                        new VehicleMake() { VehicleMakeName="GMC" },
                        new VehicleMake() { VehicleMakeName="Ginetta" },
                        new VehicleMake() { VehicleMakeName="Gumpert" },
                        new VehicleMake() { VehicleMakeName="Great Wall" },
                        new VehicleMake() { VehicleMakeName="Honda" },
                        new VehicleMake() { VehicleMakeName="Hennessey" },
                        new VehicleMake() { VehicleMakeName="Holden" },
                        new VehicleMake() { VehicleMakeName="Hyundai" },
                        new VehicleMake() { VehicleMakeName="Hummer" },
                        new VehicleMake() { VehicleMakeName="Infiniti" },
                        new VehicleMake() { VehicleMakeName="Isuzu" },
                        new VehicleMake() { VehicleMakeName="Jeep" },
                        new VehicleMake() { VehicleMakeName="Jaguar" },
                        new VehicleMake() { VehicleMakeName="Joss" },
                        new VehicleMake() { VehicleMakeName="Koenigsegg" },
                        new VehicleMake() { VehicleMakeName="Kia" },
                        new VehicleMake() { VehicleMakeName="Lada" },
                        new VehicleMake() { VehicleMakeName="Lexus" },
                        new VehicleMake() { VehicleMakeName="Lamborghini" },
                        new VehicleMake() { VehicleMakeName="Land Rover" },
                        new VehicleMake() { VehicleMakeName="Lincoln" },
                        new VehicleMake() { VehicleMakeName="Lotus" },
                        new VehicleMake() { VehicleMakeName="Luxgen Mahindra" },
                        new VehicleMake() { VehicleMakeName="Lancia" },
                        new VehicleMake() { VehicleMakeName="Maruti Suzuki" },
                        new VehicleMake() { VehicleMakeName="Maserati" },
                        new VehicleMake() { VehicleMakeName="Maybach" },
                        new VehicleMake() { VehicleMakeName="Mazda" },
                        new VehicleMake() { VehicleMakeName="Mclaren" },
                        new VehicleMake() { VehicleMakeName="Mercedes Benz" },
                        new VehicleMake() { VehicleMakeName="Mitsubishi" },
                        new VehicleMake() { VehicleMakeName="Morgan Motor" },
                        new VehicleMake() { VehicleMakeName="Mini" },
                        new VehicleMake() { VehicleMakeName="Mosler" },
                        new VehicleMake() { VehicleMakeName="Mustang" },
                        new VehicleMake() { VehicleMakeName="Nissan Motors" },
                        new VehicleMake() { VehicleMakeName="Noble Automotive" },
                        new VehicleMake() { VehicleMakeName="Opel" },
                        new VehicleMake() { VehicleMakeName="Pagani" },
                        new VehicleMake() { VehicleMakeName="Panoz" },
                        new VehicleMake() { VehicleMakeName="Perodua" },
                        new VehicleMake() { VehicleMakeName="Peugeot" },
                        new VehicleMake() { VehicleMakeName="Piaggio" },
                        new VehicleMake() { VehicleMakeName="Pininfarina" },
                        new VehicleMake() { VehicleMakeName="Porsche" },
                        new VehicleMake() { VehicleMakeName="Proton" },
                        new VehicleMake() { VehicleMakeName="Renault" },
                        new VehicleMake() { VehicleMakeName="Reva" },
                        new VehicleMake() { VehicleMakeName="Rimac Automobili" },
                        new VehicleMake() { VehicleMakeName="Rolls Royce" },
                        new VehicleMake() { VehicleMakeName="Ruf Automobile" },
                        new VehicleMake() { VehicleMakeName="Saab" },
                        new VehicleMake() { VehicleMakeName="Scania" },
                        new VehicleMake() { VehicleMakeName="Scion" },
                        new VehicleMake() { VehicleMakeName="Seat" },
                        new VehicleMake() { VehicleMakeName="Shelby" },
                        new VehicleMake() { VehicleMakeName="Skoda" },
                        new VehicleMake() { VehicleMakeName="Smart" },
                        new VehicleMake() { VehicleMakeName="Spyker Cars" },
                        new VehicleMake() { VehicleMakeName="Ssangyong" },
                        new VehicleMake() { VehicleMakeName="SSC" },
                        new VehicleMake() { VehicleMakeName="Suzuki" },
                        new VehicleMake() { VehicleMakeName="Subaru" },
                        new VehicleMake() { VehicleMakeName="Tata" },
                        new VehicleMake() { VehicleMakeName="Tatra" },
                        new VehicleMake() { VehicleMakeName="Tesla" },
                        new VehicleMake() { VehicleMakeName="Think" },
                        new VehicleMake() { VehicleMakeName="Toyota" },
                        new VehicleMake() { VehicleMakeName="Tramontana" },
                        new VehicleMake() { VehicleMakeName="Troller" },
                        new VehicleMake() { VehicleMakeName="TVR" },
                        new VehicleMake() { VehicleMakeName="UAZ" },
                        new VehicleMake() { VehicleMakeName="Vandenbrink Design" },
                        new VehicleMake() { VehicleMakeName="Vauxhall" },
                        new VehicleMake() { VehicleMakeName="Vector Motors" },
                        new VehicleMake() { VehicleMakeName="Venturi" },
                        new VehicleMake() { VehicleMakeName="Vauxhall" },
                        new VehicleMake() { VehicleMakeName="Volkswagen" },
                        new VehicleMake() { VehicleMakeName="Volvo" },
                        new VehicleMake() { VehicleMakeName="Wiesmann" },
                        new VehicleMake() { VehicleMakeName="Zagato" },
                        new VehicleMake() { VehicleMakeName="Zaz" },
                        new VehicleMake() { VehicleMakeName="Zil" },
                        new VehicleMake() { VehicleMakeName="LAND CRUISER" },
                        new VehicleMake() { VehicleMakeName="Mercury" },
                        new VehicleMake() { VehicleMakeName="Rang Rover" },
                        new VehicleMake() { VehicleMakeName="PANAMER" }
                    };
                    await _dbContext.SaveListAsync(VehicleMakes);
                }

                if ((await _dbContext.GetAllAsync<VehiclePlateCategory>()).ToList().Count == 0)
                {
                    var VehiclePlateCategories = new List<VehiclePlateCategory>
                    {
                        new VehiclePlateCategory() { VehiclePlateCategoryName="1",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="2",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="3",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="4",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="5",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="6",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="7",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="8",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="9",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="10",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="11",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="12",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="13",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="14",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="15",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="16",Type=1 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="17",Type=1 },

                        new VehiclePlateCategory() { VehiclePlateCategoryName="A-0",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="B-1",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="C-2",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="D-3",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="E-4",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="F-5",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="G-6",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="H-7",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="I-8",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="J-9",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="K-K",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="L-L",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="M-M",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="N-N",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="O-O",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="P-P",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="Q-Q",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="R-R",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="S-S",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="T-T",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="U-U",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="V-V",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="W-W",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="X-X",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="Y-Y",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="Z-Z",Type=2 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="WHITE",Type=2 },

                        new VehiclePlateCategory() { VehiclePlateCategoryName="WHITE",Type=3 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="1",Type=3 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="2",Type=3 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="3",Type=3 },

                        new VehiclePlateCategory() { VehiclePlateCategoryName="A",Type=4 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="B",Type=4 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="C",Type=4 },

                        new VehiclePlateCategory() { VehiclePlateCategoryName="A",Type=5 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="B",Type=5 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="C",Type=5 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="D",Type=5 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="G",Type=5 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="H",Type=5 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="X",Type=5 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="White",Type=5 },

                        new VehiclePlateCategory() { VehiclePlateCategoryName="White",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="Qata",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="A",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="C",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="D",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="I",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="K",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="M",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="N",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="S",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="V",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="Y",Type=6 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="B",Type=6 },

                        new VehiclePlateCategory() { VehiclePlateCategoryName="A",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="B",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="C",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="D",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="E",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="F",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="G",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="K",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="M",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="P",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="R",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="S",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="T",Type=7 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="White",Type=7 },

                        new VehiclePlateCategory() { VehiclePlateCategoryName="Private",Type=8 },

                        new VehiclePlateCategory() { VehiclePlateCategoryName="D",Type=9 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="DD",Type=9 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="M",Type=9 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="B",Type=9 },
                        new VehiclePlateCategory() { VehiclePlateCategoryName="S",Type=9 },

                    };
                    await _dbContext.SaveListAsync(VehiclePlateCategories);
                }

                if ((await _dbContext.GetAllAsync<VehiclePlateSource>()).ToList().Count == 0)
                {
                    var VehiclePlateSources = new List<VehiclePlateSource>
                    {
                        new VehiclePlateSource() { VehiclePlateSourceName="ABUDHABI" },
                        new VehiclePlateSource() { VehiclePlateSourceName="DUBAI" },
                        new VehiclePlateSource() { VehiclePlateSourceName="SHARJAH" },
                        new VehiclePlateSource() { VehiclePlateSourceName="AJMAN" },
                        new VehiclePlateSource() { VehiclePlateSourceName="UM AL QUEWAIN" },
                        new VehiclePlateSource() { VehiclePlateSourceName="RAS AL KHAYMAH" },
                        new VehiclePlateSource() { VehiclePlateSourceName="FUJAIRAH" },
                        new VehiclePlateSource() { VehiclePlateSourceName="BAHRAIN" },
                        new VehiclePlateSource() { VehiclePlateSourceName="OMAN" },
                    };
                    await _dbContext.SaveListAsync(VehiclePlateSources);
                }

                if ((await _dbContext.GetAllAsync<VehiclePlateType>()).ToList().Count == 0)
                {
                    var VehiclePlateTypes = new List<VehiclePlateType>
                    {
                        new VehiclePlateType() { VehiclePlateTypeName="Private" },
                        new VehiclePlateType() { VehiclePlateTypeName="Diplomatic" },
                        new VehiclePlateType() { VehiclePlateTypeName="Delegate" },
                        new VehiclePlateType() { VehiclePlateTypeName="Government" },
                        new VehiclePlateType() { VehiclePlateTypeName="Customs" },
                        new VehiclePlateType() { VehiclePlateTypeName="Police" },
                        new VehiclePlateType() { VehiclePlateTypeName="Company" },
                        new VehiclePlateType() { VehiclePlateTypeName="Rent a car" },
                    };
                    await _dbContext.SaveListAsync(VehiclePlateTypes);
                }

                if ((await _dbContext.GetAllAsync<VehicleType>()).ToList().Count == 0)
                {
                    var VehicleTypes = new List<VehicleType>
                    {
                        new VehicleType() { VehicleTypeName="Car" },
                        new VehicleType() { VehicleTypeName="Truck" },
                    };
                    await _dbContext.SaveListAsync(VehicleTypes);
                }

                if ((await _dbContext.GetAllAsync<Country>()).ToList().Count == 0)
                {
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 1, CountryName = "Afghanistan", Capital = "Kabul", Code2 = "AF", Code3 = "AFG", ISDCode = "93", TimeZone = "UTC+04:30", TimeZoneMinutes = 270, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 2, CountryName = "Åland Islands", Capital = "Mariehamn", Code2 = "AX", Code3 = "ALA", ISDCode = "358", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 3, CountryName = "Albania", Capital = "Tirana", Code2 = "AL", Code3 = "ALB", ISDCode = "355", TimeZone = "UTC+01:00", TimeZoneMinutes = 60, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 4, CountryName = "Algeria", Capital = "Algiers", Code2 = "DZ", Code3 = "DZA", ISDCode = "213", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 5, CountryName = "American Samoa", Capital = "Pago Pago", Code2 = "AS", Code3 = "ASM", ISDCode = "", TimeZone = "UTC-11:00", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 6, CountryName = "Andorra", Capital = "Andorra la Vella", Code2 = "AD", Code3 = "AND", ISDCode = "376", TimeZone = "UTC+01:00", TimeZoneMinutes = 60, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 7, CountryName = "Angola", Capital = "Luanda", Code2 = "AO", Code3 = "AGO", ISDCode = "244", TimeZone = "UTC+01:00", TimeZoneMinutes = 60, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 8, CountryName = "Anguilla", Capital = "The Valley", Code2 = "AI", Code3 = "AIA", ISDCode = "", TimeZone = "UTC-04:00", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 9, CountryName = "Antarctica", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 10, CountryName = "Antigua and Barbuda", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 11, CountryName = "Argentina", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 12, CountryName = "Armenia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 13, CountryName = "Aruba", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 14, CountryName = "Australia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 15, CountryName = "Austria", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 16, CountryName = "Azerbaijan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 17, CountryName = "Bahamas", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 18, CountryName = "Bahrain", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 19, CountryName = "Bangladesh", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 20, CountryName = "Barbados", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 31, CountryName = "Belarus", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 32, CountryName = "Belgium", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 33, CountryName = "Belize", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 34, CountryName = "Benin", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 35, CountryName = "Bermuda", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 36, CountryName = "Bhutan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 37, CountryName = "Bolivia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 38, CountryName = "Bonaire, Sint Eustatius and Saba", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 39, CountryName = "Bosnia and Herzegovina", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 40, CountryName = "Botswana", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 41, CountryName = "Bouvet Island", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 42, CountryName = "Brazil", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 43, CountryName = "British Indian Ocean Territory", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 44, CountryName = "Brunei", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 45, CountryName = "Bulgaria", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 46, CountryName = "Burkina Faso", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 47, CountryName = "Burundi", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 48, CountryName = "Cambodia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 49, CountryName = "Cameroon", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 50, CountryName = "Canada", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 51, CountryName = "Cape Verde", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 52, CountryName = "Cayman Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 53, CountryName = "Central African Republic", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 54, CountryName = "Chad", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 55, CountryName = "Chile", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 56, CountryName = "China", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 57, CountryName = "Christmas Island", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 58, CountryName = "Cocos (Keeling) Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 59, CountryName = "Colombia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 60, CountryName = "Comoros", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 61, CountryName = "Congo, Republic of the", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 62, CountryName = "Congo, the Democratic Republic of the", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 63, CountryName = "Cook Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 64, CountryName = "Costa Rica", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 65, CountryName = "Côte d'Ivoire", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 66, CountryName = "Croatia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 67, CountryName = "Cuba", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 68, CountryName = "Curaçao", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 69, CountryName = "Cyprus", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 70, CountryName = "Czech Republic", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 71, CountryName = "Denmark", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 72, CountryName = "Djibouti", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 73, CountryName = "Dominica", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 74, CountryName = "Dominican Republic", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 75, CountryName = "East Timor", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 76, CountryName = "Ecuador", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 77, CountryName = "Egypt", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 78, CountryName = "El Salvador", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 79, CountryName = "Equatorial Guinea", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 80, CountryName = "Eritrea", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 81, CountryName = "Estonia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 82, CountryName = "Ethiopia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 83, CountryName = "Falkland Islands (Malvinas)", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 84, CountryName = "Faroe Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 85, CountryName = "Fiji", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 86, CountryName = "Finland", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 87, CountryName = "France", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 88, CountryName = "French Guiana", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 89, CountryName = "French Polynesia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 90, CountryName = "French Southern Territories", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 91, CountryName = "Gabon", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 92, CountryName = "Gambia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 93, CountryName = "Georgia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 94, CountryName = "Germany", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 95, CountryName = "Ghana", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 96, CountryName = "Gibraltar", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 97, CountryName = "Greece", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 98, CountryName = "Greenland", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 99, CountryName = "Grenada", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 100, CountryName = "Guadeloupe", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 101, CountryName = "Guam", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 102, CountryName = "Guatemala", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 103, CountryName = "Guernsey", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 104, CountryName = "Guinea", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 105, CountryName = "Guinea-Bissau", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 106, CountryName = "Guyana", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 107, CountryName = "Haiti", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 108, CountryName = "Heard Island and McDonald Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 109, CountryName = "Honduras", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 110, CountryName = "Hong Kong", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 111, CountryName = "Hungary", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 112, CountryName = "Iceland", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 113, CountryName = "India", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 114, CountryName = "Indonesia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 115, CountryName = "Iran", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 116, CountryName = "Iraq", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 117, CountryName = "Ireland", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 118, CountryName = "Isle of Man", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 119, CountryName = "Israel", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 120, CountryName = "Italy", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 121, CountryName = "Jamaica", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 122, CountryName = "Japan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 123, CountryName = "Jersey", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 124, CountryName = "Jordan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 125, CountryName = "Kazakhstan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 126, CountryName = "Kenya", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 127, CountryName = "Kiribati", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 128, CountryName = "Kuwait", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 129, CountryName = "Kyrgyzstan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 130, CountryName = "Laos", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 131, CountryName = "Latvia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 132, CountryName = "Lebanon", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 133, CountryName = "Lesotho", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 134, CountryName = "Liberia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 135, CountryName = "Libya", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 136, CountryName = "Liechtenstein", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 137, CountryName = "Lithuania", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 138, CountryName = "Luxembourg", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 139, CountryName = "Macau", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 140, CountryName = "Macedonia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 141, CountryName = "Madagascar", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 142, CountryName = "Malawi", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 143, CountryName = "Malaysia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 144, CountryName = "Maldives", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 145, CountryName = "Mali", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 146, CountryName = "Malta", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 147, CountryName = "Marshall Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 148, CountryName = "Martinique", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 149, CountryName = "Mauritania", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 150, CountryName = "Mauritius", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 151, CountryName = "Mayotte", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 152, CountryName = "Mexico", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 153, CountryName = "Micronesia, Federated States of", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 154, CountryName = "Moldova, Republic of", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 155, CountryName = "Monaco", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 156, CountryName = "Mongolia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 157, CountryName = "Montenegro", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 158, CountryName = "Montserrat", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 159, CountryName = "Morocco", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 160, CountryName = "Mozambique", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 161, CountryName = "Myanmar (Burma)", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 162, CountryName = "Namibia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 163, CountryName = "Nauru", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 164, CountryName = "Nepal", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 165, CountryName = "Netherlands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 166, CountryName = "New Caledonia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 167, CountryName = "New Zealand", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 168, CountryName = "Nicaragua", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 169, CountryName = "Niger", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 170, CountryName = "Nigeria", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 171, CountryName = "Niue", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 172, CountryName = "Norfolk Island", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 173, CountryName = "North Korea", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 174, CountryName = "Northern Mariana Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 175, CountryName = "Norway", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 176, CountryName = "Oman", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 177, CountryName = "Pakistan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 178, CountryName = "Palau", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 179, CountryName = "Palestinian Territory, Occupied", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 180, CountryName = "Panama", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 181, CountryName = "Papua New Guinea", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 182, CountryName = "Paraguay", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 183, CountryName = "Peru", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 184, CountryName = "Philippines", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 185, CountryName = "Pitcairn", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 186, CountryName = "Poland", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 187, CountryName = "Portugal", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 188, CountryName = "Puerto Rico", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 189, CountryName = "Qatar", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 190, CountryName = "Réunion", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 191, CountryName = "Romania", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 192, CountryName = "Russia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 193, CountryName = "Rwanda", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 194, CountryName = "Saint Barthélemy", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 195, CountryName = "Saint Helena", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 196, CountryName = "Saint Kitts and Nevis", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 197, CountryName = "Saint Lucia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 198, CountryName = "Saint Martin (French part)", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 199, CountryName = "Saint Pierre and Miquelon", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 200, CountryName = "Saint Vincent and the Grenadines", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 201, CountryName = "Samoa", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 202, CountryName = "San Marino", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 203, CountryName = "São Tomé and Príncipe", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 204, CountryName = "Saudi Arabia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 205, CountryName = "Senegal", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 206, CountryName = "Serbia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 207, CountryName = "Seychelles", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 208, CountryName = "Sierra Leone", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 209, CountryName = "Singapore", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 210, CountryName = "Sint Maarten", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 211, CountryName = "Slovakia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 212, CountryName = "Slovenia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 213, CountryName = "Solomon Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 214, CountryName = "Somalia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 215, CountryName = "South Africa", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 216, CountryName = "South Georgia and the South Sandwich Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 217, CountryName = "South Korea", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 218, CountryName = "Spain", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 219, CountryName = "Sri Lanka", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 220, CountryName = "Sudan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 221, CountryName = "Suriname", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 222, CountryName = "Svalbard and Jan Mayen", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 223, CountryName = "Swaziland", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 224, CountryName = "Sweden", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 225, CountryName = "Switzerland", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 226, CountryName = "Syrian Arab Republic", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 227, CountryName = "Taiwan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 228, CountryName = "Tajikistan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 229, CountryName = "Tanzania", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 230, CountryName = "Thailand", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 231, CountryName = "Togo", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 232, CountryName = "Tokelau", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 233, CountryName = "Tonga", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 234, CountryName = "Trinidad and Tobago", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 235, CountryName = "Tunisia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 236, CountryName = "Turkey", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 237, CountryName = "Turkmenistan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 238, CountryName = "Turks and Caicos Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 239, CountryName = "Tuvalu", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 240, CountryName = "Uganda", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 241, CountryName = "Ukraine", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 242, CountryName = "United Arab Emirates", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 243, CountryName = "United Kingdom", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 244, CountryName = "United States", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 245, CountryName = "United States Minor Outlying Islands", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 246, CountryName = "Uruguay", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 247, CountryName = "Uzbekistan", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 248, CountryName = "Vanuatu", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 249, CountryName = "Vatican City", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 250, CountryName = "Venezuela", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 251, CountryName = "Vietnam", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 252, CountryName = "Virgin Islands, British", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 253, CountryName = "Virgin Islands, U.S.", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 254, CountryName = "Wallis and Futuna", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 255, CountryName = "Western Sahara", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 256, CountryName = "Yemen", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 257, CountryName = "Zambia", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });
                    await _dbContext.NonIdentityInsertAsync(new Country() { CountryID = 258, CountryName = "Zimbabwe", Capital = "", Code2 = "", Code3 = "", ISDCode = "", TimeZone = "", TimeZoneMinutes = 0, Show = false });

                    await _dbContext.ExecuteAsync("Update Country Set CountryID=CountryID-10 where CountryID>20",null);
                }

                #region General Settings
                var settings = await _dbContext.GetAsyncByCondition<GeneralSettings>("SettingsKey=@SettingsKey", new { SettingsKey = "SlotSelectionBefore" });
                if (settings == null)
                {
                    GeneralSettings generalSettings = new GeneralSettings()
                    {
                        SettingsKey = "SlotSelectionBefore",
                        SettingsValue = "1"
                    };
                    await _dbContext.SaveAsync(generalSettings);
                }
                #endregion

                #region General Settings
                var settings2 = await _dbContext.GetAsyncByCondition<GeneralSettings>("SettingsKey=@SettingsKey", new { SettingsKey = "VisitSelectionBefore" });
                if (settings2 == null)
                {
                    GeneralSettings generalSettings = new GeneralSettings()
                    {
                        SettingsKey = "VisitSelectionBefore",
                        SettingsValue = "2"
                    };
                    await _dbContext.SaveAsync(generalSettings);
                }

                var c = (await _dbContext.GetAllAsync<LocationType>()).ToList().Count;
                if (c == 0)
                {
                    await _dbContext.NonIdentityInsertAsync(new LocationType() { LocationTypeID = 1, LocationTypeName = "Delivery Point" } );
                    await _dbContext.NonIdentityInsertAsync(new LocationType() { LocationTypeID = 2, LocationTypeName = "Drop Point" } );
                    await _dbContext.NonIdentityInsertAsync(new LocationType() { LocationTypeID = 3, LocationTypeName = "Storage" });
                }
                #endregion
            }
            catch (Exception err)
            {
            }
        }

        public async Task Version12(IDbTransaction tran)
        {
            await _dbContext.ExecuteAsync("Update Document Set IsDeleted=1 Where MediaID is null and MediaID2 is null", null, tran);
        }
    }
}
