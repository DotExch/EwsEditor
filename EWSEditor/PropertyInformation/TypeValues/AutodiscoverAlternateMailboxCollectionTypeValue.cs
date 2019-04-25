using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Autodiscover;

namespace EWSEditor.PropertyInformation.TypeValues
{
    public class AutodiscoverAlternateMailboxCollectionTypeValue : SimpleTypeValueBase<AlternateMailboxCollection>
    {
        public override void GetValue(AlternateMailboxCollection obj, StringBuilder sb)
        {
            foreach (AlternateMailbox oAlternativeMailbox in obj.Entries)
            {
                sb.AppendFormat(
                                "Type: {0} \r\n" +
                                "DisplayName: {1} \r\n" +
                                "LegacyDN: {2} \r\n" +
                                "Server: {3} \r\n" +
                                "SmtpAddress: {4} \r\n" +
                                "OwnerSmtpAddress: {5} \r\n" +
                                "\r\n",

                    oAlternativeMailbox.Type,
                    oAlternativeMailbox.DisplayName,
                    oAlternativeMailbox.LegacyDN,
                    oAlternativeMailbox.Server,
                    oAlternativeMailbox.SmtpAddress,
                    oAlternativeMailbox.OwnerSmtpAddress

                    );
            }
        }
    }
}
