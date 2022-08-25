using System.Xml.Serialization;

namespace Keyfactor.Extensions.Orchestrator.Kemp.Client.Models.SubModels
{
    [XmlRoot(ElementName = "Success")]
    public class CertSuccess
    {
        [XmlElement(ElementName = "Data")] public CertData Data { get; set; }
    }
}