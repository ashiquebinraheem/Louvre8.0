using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using System;

public class ErrorModel : PageModel
{
    private readonly IHostEnvironment _env;

    public ErrorModel(IHostEnvironment env)
    {
        _env = env;
    }

    public int? StatusCode { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
    public bool ShowDetails => _env.IsDevelopment();

    public void OnGet(int? code = null)
    {
        StatusCode = code;

        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (feature != null)
        {
            Exception = feature.Error;
            Message = Exception.Message;
        }
        else if (code.HasValue)
        {
            Message = code switch
            {
                404 => "Page not found.",
                403 => "Access denied.",
                500 => "Internal server error.",
                _ => $"Error {code}"
            };
        }
    }
}
