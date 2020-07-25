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

            TwilioClient.Init(sid, token);
            await MessageResource.CreateAsync(new PhoneNumber(message.Destination), from: new PhoneNumber(from),
                   body: message.Body);

        }
    }
}