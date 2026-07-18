using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

using ECommerce.Infrastructure.Settings;

namespace ECommerce.Infrastructure.Services;

public class SendGridEmailService : IEmailSenderService
{
    private readonly SendGridSettings _sendGridSettings;

    public SendGridEmailService(IOptions<SendGridSettings> sendGridSettings)
    {
        _sendGridSettings = sendGridSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        // 1. ÅÚÏÇÏ Úãíá SendGrid ÈÇáãİÊÇÍ ÇáÓÑí
        var client = new SendGridClient(_sendGridSettings.ApiKey);

        // 2. ÊÍÏíÏ ÇáÑÇÓá æÇáãÓÊŞÈá
        var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
        var to = new EmailAddress(toEmail);

        // 3. ÊÌåíÒ ãÍÊæì ÇáÑÓÇáÉ (HTML æÇáÜ Plain Text ááÍãÇíÉ)
        var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlMessage);

        // 4. ÅÑÓÇá ÇáÅíãíá
        var response = await client.SendEmailAsync(msg);

        // áæ ÇáÓíÑİÑ ÑÌÚ ÎØÃ¡ ÇÑãí Exception ÚÔÇä íÈÇä ãÚÇß Ìæå ÇááæÌÇÊ
        if (!response.IsSuccessStatusCode)
        {
            throw new AppException($"SendGrid failed with status code: {response.StatusCode}");
        }
    }
}