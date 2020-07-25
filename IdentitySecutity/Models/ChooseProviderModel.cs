using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdentitySecutity.Models
{
    public class ChooseProviderModel
    {
        public List<string> Providers { get; set; }
        public string ChosenProvider { get; set; }
    }
}