using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Autodiscover;

namespace EWSEditor.PropertyInformation.TypeValues
{
    public class AutodiscoverProtocolConnectionCollectionTypeValue : SimpleTypeValueBase<ProtocolConnectionCollection>
    {
        public override void GetValue(ProtocolConnectionCollection obj, StringBuilder sb)
        {
            foreach (ProtocolConnection oProtocolConnection in obj.Connections)
            {
                sb.AppendFormat("Hostname: {0} \r\n" +
                            "Port: {1}\r\n" +
                            "EncryptionMethod: {2}\r\n",
                            oProtocolConnection.Hostname,
                            oProtocolConnection.Port,
                            oProtocolConnection.EncryptionMethod);
            }
        }
    }
}
