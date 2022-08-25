using System.Collections.Generic;
using System.Xml.Serialization;

namespace Keyfactor.Extensions.Orchestrator.Kemp.Client.Models.SubModels
{
    [XmlRoot(ElementName = "Data")]
    public class Data
    {
        [XmlElement(ElementName = "cert")] public List<Cert> Certs { get; set; }
    }
}