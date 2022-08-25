using System.Xml.Serialization;

namespace Keyfactor.Extensions.Orchestrator.Kemp.Client.Models.SubModels
{
    [XmlRoot(ElementName = "Data")]
    public class CertData
    {
        [XmlElement(ElementName = "certificate")]
        public string Certificate { get; set; }
    }
}