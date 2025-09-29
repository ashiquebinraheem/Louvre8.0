using Newtonsoft.Json;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Louvre.Shared.Repository
{

    public interface IReflexionRepository
    {
        Task<string> GetToken();
        Task ImportItems();
        Task ImportPOOwners();
        Task PushDeliveryItem(int requestId);
    }


    public class ReflexionRepository: IReflexionRepository
    {
        private readonly IDbContext _dbContext;

        public ReflexionRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public  async Task<string> GetToken()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            var token = new DMSLoginResponse();

            using (var httpClient = new HttpClient(clientHandler))
            {
                httpClient.DefaultRequestHeaders.Add("UserName", "Admin");
                httpClient.DefaultRequestHeaders.Add("Password", "1234");

                using (var response = await httpClient.PostAsync("https://5.195.42.146:8094/ReflexionDeliveryManagement.svc/GET_TOKEN_AUTHENTICATE", null))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    token = JsonConvert.DeserializeObject<DMSLoginResponse>(apiResponse);
                }
            }
            return token.Token;
        }

        public  async Task ImportItems()
        {
            HttpClientHandler clientHandler1 = new HttpClientHandler();
            clientHandler1.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            using (var httpClient = new HttpClient(clientHandler1))
            {
                var token = await GetToken();
                httpClient.DefaultRequestHeaders.Add("Token", token);

                using (var response = await httpClient.PostAsync("https://5.195.42.146:8094/ReflexionDeliveryManagement.svc/Get_Item_Details", null))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var items = JsonConvert.DeserializeObject<DMSItemResultModel>(apiResponse);
                    foreach (var item in items.RMS.ItemsList)
                    {
                        var isExist = await _dbContext.GetAsync<ItemMaster>(item.REFLEXION_ITEM_ID);
                        if (isExist == null)
                        {
                            await _dbContext.ExecuteAsync("Insert into ItemMaster( ItemID, Code, Name, Type, PurchaseUnit, IsExpirable, AddedOn) Values ( @REFLEXION_ITEM_ID, @REFLEXION_ITEM_CODE, @REFLEXION_ITEM_NAME, @ITEM_TYPE, @PURCHASE_UNIT, @EXPIRABLE, @AddedOn)", item);
                        }
                        //else
                        //{
                        //    await _dbContext.ExecuteAsync("Update ItemMaster Set ")
                        //}
                    }
                }
            }
        }

        public async Task ImportPOOwners()
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                using (var httpClient = new HttpClient(clientHandler))
                {
                    var token = await GetToken();
                    httpClient.DefaultRequestHeaders.Add("Token", token);

                    using (var response = await httpClient.PostAsync("https://5.195.42.146:8094/ReflexionDeliveryManagement.svc/Get_POOwner_Details", null))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        var items = JsonConvert.DeserializeObject<DMSPOOwnerResultModel>(apiResponse);
                        foreach (var item in items.RMS.POOwnerList)
                        {
                            var isExist = await _dbContext.GetAsync<POOwner>(item.STAFF_ID);
                            if (isExist == null)
                            {
                                await _dbContext.ExecuteAsync($@"Insert into PoOwner(POOwnerID, StaffName, Designation, AddedOn) Values ( @STAFF_ID,@STAFF_NAME,@DESIGNATION, @AddedOn)", item);
                            }
                            //else
                            //{
                            //    await _dbContext.ExecuteAsync("Update ItemMaster Set ")
                            //}
                        }
                    }
                }
            }catch(Exception ex)
            {

            }
        }

        public async Task PushDeliveryItem(int requestId)
        {
            var headerDetails = await _dbContext.GetAsync<PushDeliveryHeaderModel>($@"Select R.RefID,C.VendorID,CompanyName VendorName,'' Address,ContactPerson,ContactNumber,PONumber,PODate,DeliveryDate,POOwnerID
                From Request R
                JOIN Employee E on E.EmployeeID=R.EmployeeID
                JOIN Company C on C.CompanyID=E.CompanyID
                Where R.RequestID=RequestID", new { RequestID =requestId});


            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            using (var httpClient = new HttpClient(clientHandler))
            {
                var token = await GetToken();
                httpClient.DefaultRequestHeaders.Add("Token", token);
                httpClient.DefaultRequestHeaders.Add("EXTSYS_REF_ID", headerDetails.RefID.ToString());
                httpClient.DefaultRequestHeaders.Add("VENDOR_ID", headerDetails.VendorID.ToString());
                httpClient.DefaultRequestHeaders.Add("VENDOR_NAME", headerDetails.VendorName);
                httpClient.DefaultRequestHeaders.Add("VENDOR_ADDRESS", headerDetails.Address);
                httpClient.DefaultRequestHeaders.Add("VENDOR_CONTACT_PERSON", headerDetails.ContactPerson);
                httpClient.DefaultRequestHeaders.Add("VENDOR_CONTACT_NUM", headerDetails.ContactNumber);
                httpClient.DefaultRequestHeaders.Add("VENDOR_EMAIL", headerDetails.VendorEmail);
                httpClient.DefaultRequestHeaders.Add("PO_NUMBER", headerDetails.PONumber.ToString());
                httpClient.DefaultRequestHeaders.Add("PO_DATE", headerDetails.PODate.ToString("yyyy-MMM-dd"));
                httpClient.DefaultRequestHeaders.Add("DELIVERY_DATE", headerDetails.DeliveryDate.ToString("yyyy-MMM-dd"));
                httpClient.DefaultRequestHeaders.Add("PO_OWNER_ID", headerDetails.POOwnerID.ToString());

               
                var Line_Items = (await _dbContext.GetEnumerableAsync<PushDeliveryItemModel>($@"Select Type as ITEM_TYPE,R.ItemID as REFLEXION_ITEM_ID,Quantity QTY,PRICE,VALUE,OEM,Brand as Make,MODEL,SerialNo SERIAL_NO,
                        Convert(varchar,WarrantyDate,105) WARRANTY_END_DATE,ManufactureYear MANUFACTURING_YEAR,ExpectedLifeYear EXPECTED_LIFE,
                        BatchNo BATCH_NO,Convert(varchar,ExpiryDate,105) EXPIRY_DATE
                        From RequestItem R
                        JOIN ItemMaster M on M.ItemID=R.ItemID
                        Where R.IsDeleted=0 and R.RequestID=@RequestID", new { RequestID =requestId})).ToList();


                var items = new PushDeliveryItemListModel();
                foreach (var item in Line_Items)
                {
                    items.Line_Items.Add(new PushDeliveryListModel()
                    {
                        Line_Items = item
                    });
                }

                var itemjson = JsonConvert.SerializeObject(items.Line_Items);

                httpClient.DefaultRequestHeaders.Add("Line_Items", itemjson);

                using (var response = await httpClient.PostAsync("https://5.195.42.146:8094/ReflexionDeliveryManagement.svc/PUSH_Delivery_Request", null))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<DMSPushResultModel>(apiResponse);
                    if(res.InsertDataItemLines.Status)
                    {
                        await _dbContext.ExecuteAsync($"Update Request Set RefID=@RefID Where RequestID={requestId}", new { RefID=res.InsertDataItemLines.Id});
                    }
                }
            }
        }
    }
}
