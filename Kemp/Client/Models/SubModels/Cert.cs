using System.Xml.Serialization;

namespace Keyfactor.Extensions.Orchestrator.Kemp.Client.Models.SubModels
{
    [XmlRoot(ElementName = "cert")]
    public class Cert
    {
        [XmlElement(ElementName = "name")] public string Name { get; set; }

        [XmlElement(ElementName = "type")] public string Type { get; set; }

        [XmlElement(ElementName = "modulus")] public string Modulus { get; set; }
    }
}