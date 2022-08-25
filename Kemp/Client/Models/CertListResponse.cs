using System.Xml.Serialization;
using Keyfactor.Extensions.Orchestrator.Kemp.Client.Models.SubModels;

namespace Keyfactor.Extensions.Orchestrator.Kemp.Client.Models
{
    [XmlRoot(ElementName = "Response")]
    public class CertListResponse
    {
        [XmlElement(ElementName = "Success")] public Success Success { get; set; }

        [XmlAttribute(AttributeName = "stat")] public string Stat { get; set; }

        [XmlAttribute(AttributeName = "code")] public string Code { get; set; }
    }
}