namespace Louvre.Shared.Models
{
    public class BaseResponse
    {
        public int Code;

        public int ResponseCode { get; set; }
        public string? ResponseTitle { get; set; }
        public string? ResponseMessage { get; set; }
        public string? ResponseErrorDescription { get; set; }
        public string? Message { get; set; }

        public BaseResponse()
        {

        }

        public BaseResponse(int code)
        {
            if (code > -1)
                CreatSuccessResponse(code);
            else
                CreatErrorResponse(code);
        }

        public virtual void CreatSuccessResponse(int code = 0)
        {
            ResponseCode = code;
            ResponseTitle = "";
            switch (code)
            {
                case 0:
                    ResponseMessage = "You've done!!";
                    break;
                case 1:
                    ResponseMessage = "You've done!!";
                    break;
                case 2:
                    ResponseMessage = "Your Email Address verified Successfully";
                    break;
                case 3:
                    ResponseMessage = "Your Password changed successfully";
                    break;
                case 4:
                    ResponseMessage = "Your profile details successfully updated";
                    break;
                case 5:
                    ResponseTitle = "You're done!!";
                    ResponseMessage = $@"your account has been registered succesfully!! Go to your email inbox and confirm your email.</b>
						Click <a href='/login'>here</a> to login";
                    break;
                case 101:
                    ResponseTitle = "You're done!!";
                    ResponseMessage = "Your request has been sent. We will update you shortly.";
                    break;
                case 102:
                    ResponseMessage = "Request approved and gate pass send to the requester";
                    break;
                case 103:
                    ResponseMessage = "Request is accepted and passed to the higher authority for approval";
                    break;
                case 104:
                    ResponseMessage = "Request rejected succesfully";
                    break;
                case 105:
                    ResponseMessage = "Application accepted";
                    break;
                case 106:
                    ResponseMessage = "Application rejected";
                    break;
                case 107:
                    ResponseMessage = "Visit Request Accepted Succesfully";
                    break;
            }
        }

        public virtual void CreatErrorResponse(int code, string errorDescription = "")
        {
            ResponseCode = code;
            switch (code)
            {
                case -2:
                    ResponseTitle = "Invalid Credential!!";
                    ResponseMessage = "Please check your user name!!";
                    break;
                case -3:
                    ResponseTitle = "Invalid Credential!!";
                    ResponseMessage = "Please check your password";
                    break;
                case -4:
                    ResponseTitle = "Invalid Credential!!";
                    ResponseMessage = $"Please check your current password";
                    break;
                case -5:
                    ResponseTitle = "Duplication!!";
                    ResponseMessage = $"A user is already exist with the email address!!";
                    break;
                case -6:
                    ResponseMessage = "Mail not send";
                    break;
                case -7:
                    ResponseTitle = "Duplication!!";
                    ResponseMessage = $"A branch is already exist with the same name!!";
                    break;
                case -8:
                    ResponseTitle = "Duplication!!";
                    ResponseMessage = $"A user is already exist with the same name!!";
                    break;

                //Application wise response will start from 100
                case -100:
                    ResponseTitle = "Duplication!!";
                    ResponseMessage = $"Already applied screen pattern on these days {errorDescription}!!";
                    break;
                case -101:
                    ResponseTitle = "Duplication!!";
                    ResponseMessage = $"already have a slot on this day";
                    break;
                case -102:
                    ResponseTitle = "Duplication!!";
                    ResponseMessage = $"a Slot group already exist with the same name";
                    break;
                case -103:
                    ResponseTitle = "Duplication!!";
                    ResponseMessage = $"a Slot pattern already exist with the same name";
                    break;
                case -104:
                    ResponseTitle = "Duplication!!";
                    ResponseMessage = $"a person already exist with the same name";
                    break;
                case -105:
                    ResponseTitle = "Duplication!!";
                    ResponseMessage = $"a vehicle already exist with the same register number";
                    break;
                case -106:
                    ResponseTitle = "Duplication!!";
                    ResponseMessage = $"a company already exist with the same name";
                    break;
                case -107:
                    ResponseTitle = "Host email not found!!";
                    ResponseMessage = $"cannot find employee with this email address";
                    break;
                case -108:
                    ResponseTitle = "Cant delete!!";
                    ResponseMessage = $"Slot already used";
                    break;
                case -109:
                    ResponseTitle = "Your email not confirmed";
                    ResponseMessage = $"Goto your inbox and confirm your email";
                    break;
                case -110:
                    ResponseTitle = "Your login is inactive";
                    break;
                case -111:
                    ResponseTitle = "QR code not found";
                    break;
                case -112:
                    ResponseTitle = "Invalid User Type";
                    break;
                case -113:
                    ResponseTitle = "Invalid File Uploaded";
                    break;
            }
        }

        public virtual void CreatErrorResponse(string responseMessage, string responseTitle)
        {
            ResponseCode = -1;
            ResponseTitle = responseTitle;
            ResponseMessage = responseMessage;
        }


        public virtual void CreatThrowResponse(string description)
        {
            ResponseCode = -1;
            ResponseTitle = "Oops...";
            ResponseMessage = "Something went wrong!";
            ResponseErrorDescription = description;
        }
    }

    public class APIBaseResponse
    {
        public bool Status { get; set; } = true;
        public string? Message { get; set; } = "";

        public void CreateFailureResponse(string message)
        {
            Status = false;
            Message = message;
        }
    }
}
