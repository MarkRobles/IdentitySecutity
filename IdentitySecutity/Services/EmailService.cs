using Microsoft.AspNet.Identity;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;

namespace IdentitySecutity.Services
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
         
            //var client = new SendGridClient(Environment.GetEnvironmentVariable("SENDGRID_API_KEY"));
            var client = new SendGridClient(ConfigurationManager.AppSettings["sengrid:Key"]);
            var from = new EmailAddress("aliveahuehuete@gmail.com");
            var to = new EmailAddress(message.Destination);
            var email = MailHelper.CreateSingleEmail(from,to,message.Subject,message.Body,message.Body);

            await client.SendEmailAsync(email);
        }
    }
}