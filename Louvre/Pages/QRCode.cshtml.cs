using Microsoft.AspNetCore.Mvc.RazorPages;
using Louvre.Shared.Repository;

namespace Louvre.Pages
{
    public class QRCodeModel : PageModel
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;

        public QRCodeModel(IMediaRepository mediaRepository, IEmailSender emailSender)
        {
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
        }

        public string? Image { get; set; }
        public void OnGet()
        {
            Image = _mediaRepository.GetQRImage("1111111");
            var msg = $"<img alt = 'Embedded Image' height='50' width='50' src =\"{Image}\">";
            _emailSender.SendHtmlEmailAsync("abdulrazack83@gmail.com", "QR Code", msg);
        }

        public static string FixBase64ForImage(string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", string.Empty); sbText.Replace(" ", string.Empty);
            return sbText.ToString();
        }
    }
}