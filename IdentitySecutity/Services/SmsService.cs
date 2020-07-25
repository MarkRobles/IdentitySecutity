using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;


namespace IdentitySecutity.Services
{
    public class SmsService : IIdentityMessageService
    {

        public async Task SendAsync(IdentityMessage message)
        {
            var sid = ConfigurationManager.AppSettings["twilio:Sid"];
            var token = ConfigurationManager.AppSettings["twilio:Token"];
            var from = ConfigurationManager.AppSettings["twilio:From"];
            // Find your Account SID and Auth Token at twilio.com/console
            //var sid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            //var token = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            //var from = Environment.GetEnvironmentVariable("TWILIO_FROM");


            TwilioClient.Init(sid, token);
            await MessageResource.CreateAsync(new PhoneNumber(message.Destination), from: new PhoneNumber(from),
                   body: message.Body);

        }
    }
}