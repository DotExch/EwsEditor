using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Autodiscover;

namespace EWSEditor.PropertyInformation.TypeValues
{
    public class AutodiscoverWebClientUrlCollectionTypeValue : SimpleTypeValueBase<WebClientUrlCollection>
    {
        public override void GetValue(WebClientUrlCollection obj, StringBuilder sb)
        {
            foreach (WebClientUrl oUrl in obj.Urls)
            {
                sb.AppendFormat("Url: {0} \r\n" +
                            "Authentication: {1}\r\n",
                            oUrl.Url,
                            oUrl.AuthenticationMethods);
            }
        }
    }
}
