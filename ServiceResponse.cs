using System;
using System.Xml;

namespace Phonebook
{
	public enum ResponseStatus { Success, Failure };
	
	public class ServiceResponse
	{
		public ResponseStatus m_Status;
		public string m_Information, m_Payload;
		
		public ServiceResponse(ResponseStatus status, string information, string payload)
		{
			m_Status = status;
			m_Information = information;
			m_Payload = payload;
		}
		
		public string GetXMLString()
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
			xmlDocument.AppendChild(xmlDeclaration);
			XmlElement root = xmlDocument.CreateElement("ServiceResponse", "http://www.phonebook.com/ServiceResponse");
			xmlDocument.AppendChild(root);
            XmlElement status = xmlDocument.CreateElement("Status", xmlDocument.DocumentElement.NamespaceURI);
            status.InnerText = m_Status.ToString();
            root.AppendChild(status);	
            if (m_Information.Length > 0)
            {
	            XmlElement information = xmlDocument.CreateElement("Information", xmlDocument.DocumentElement.NamespaceURI);
	            information.InnerText = m_Information;
	            root.AppendChild(information);	
            }
            if (m_Payload.Length > 0)
            {
                XmlElement payload = xmlDocument.CreateElement("Payload", xmlDocument.DocumentElement.NamespaceURI);
                XmlDocument payloadXmlDocument = new XmlDocument();
                payloadXmlDocument.LoadXml(m_Payload);
                XmlNode payloadXmlNode = xmlDocument.ImportNode(payloadXmlDocument.DocumentElement, true);
                payload.AppendChild(payloadXmlNode);
                root.AppendChild(payload);   
            }    

			return xmlDocument.OuterXml;
		}
	}
}
