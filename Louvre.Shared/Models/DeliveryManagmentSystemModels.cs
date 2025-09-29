using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Models
{
    public class DMSLoginResponse
    {
        public string? Token { get; set; }
    }

    public class DMSItemModel
    {
        public string? ITEM_TYPE { get; set; }
        public string? REFLEXION_ITEM_CODE { get; set; }
        public string? REFLEXION_ITEM_NAME { get; set; }
        public int REFLEXION_ITEM_ID { get; set; }
        public string? PURCHASE_UNIT { get; set; }
        public string? EXPIRABLE { get; set; }
        public DateTime AddedOn { get; set; } = DateTime.UtcNow;
    }

    public  class DMSItemResultModel
    {
        public DMSItemListModel RMS { get; set; }
        
    }

    public class DMSItemListModel
    {
        public List<DMSItemModel> ItemsList { get; set; }
    }

    public class DMSPOOwnerResultModel
    {
        public DMSPOOwnerListModel RMS { get; set; }
    }

    public class DMSPOOwnerListModel
    {
        public List<DMSPOOwnerItemModel> POOwnerList { get; set; }
    }

    public  class DMSPOOwnerItemModel
    {
        public int STAFF_ID { get; set; }
        public string? STAFF_NAME { get; set; }
        public string? DESIGNATION { get; set; }
        public DateTime AddedOn { get; set; } = DateTime.UtcNow;
    }

    public class DMSPushResultModel
    {
        public DMSPushResult1Model InsertDataItemLines { get; set; }
    }

    public class DMSPushResult1Model
    {
        public bool Status { get; set; }
        public int Id { get; set; }
    }

    public class PushDeliveryHeaderModel
    {
        public int RefID { get; set; }
        public int VendorID { get; set; }
        public string? VendorName { get; set; }
        public string? VendorEmail { get; set; } = "";
        public string? Address { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactNumber { get; set; }
        public string? PONumber { get; set; }
        public DateTime PODate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int POOwnerID { get; set; }
    }


    public class PushDeliveryItemListModel
    {
        public List<PushDeliveryListModel> Line_Items { get; set; } = new List<PushDeliveryListModel>();
    }


    public class PushDeliveryListModel
    {
        public PushDeliveryItemModel Line_Items { get; set; }
    }
    public class PushDeliveryItemModel
    {
        public string? ITEM_TYPE { get; set; }
        public int REFLEXION_ITEM_ID { get; set; }
        public int QTY { get; set; } = 0;
        public decimal PRICE { get; set; } = 0;
        public decimal VALUE { get; set; }=0;
        public string? OEM { get; set; } = "";
        public string? MAKE { get; set; } = "";
        public string? MODEL { get; set; } = "";
        public string? SERIAL_NO { get; set; } = "";
        public string? WARRANTY_END_DATE { get; set; } = "";
        public int MANUFACTURING_YEAR { get; set; }
        public int EXPECTED_LIFE { get; set; }
        public string? BATCH_NO { get; set; } = "";
        public string? EXPIRY_DATE { get; set; } = "";
    }
}
