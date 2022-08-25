using System.Xml.Serialization;

namespace Keyfactor.Extensions.Orchestrator.Kemp.Client.Models.SubModels
{
    [XmlRoot(ElementName = "Success")]
    public class Success
    {
        [XmlElement(ElementName = "Data")] public Data Data { get; set; }
    }
}