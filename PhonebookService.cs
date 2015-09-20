using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Xml;
using System.Xml.Schema;

namespace Phonebook
{
    public class PhonebookService : IPhonebookService
    {
    	private XmlDocument m_InputXml;
        private bool m_XmlValidated;
        private string m_XmlValidationErrors, m_MessageType;    
        private XmlNamespaceManager m_XmlNamespaceManager;
        private Phonebook m_Phonebook;
        
        public string SubmitServiceRequest(string inputXml)
        {
        	return SubmitServiceRequestInner(inputXml).GetXMLString();
        }
    	
    	public ServiceResponse SubmitServiceRequestInner(string inputXml)
        {
        	ServiceResponse response = ValidateXMLAttack(inputXml);
        	if (response.m_Status != ResponseStatus.Success)
        		return response;    

        	response = ValidateXMLAgainstXSD(inputXml);
        	if (response.m_Status != ResponseStatus.Success)
        		return response;        	
        	
        	string error;
        	bool initialized = Initialize(inputXml, out error);
        	if (!initialized)
        		return new ServiceResponse(ResponseStatus.Failure, "Unable to initialize: " + error, "");
        	
        	switch (m_MessageType)
        	{
        		case "ListEntries":
        			response = ListEntries();
        			break;
        		case "CreateEntry":
        			response = CreateEntry();
        			break;
        		case "RemoveEntry":
        			response = RemoveEntry();
        			break;
        		case "UpdateEntry":
        			response = UpdateEntry();
        			break;
        		default:
        			return new ServiceResponse(ResponseStatus.Failure, "Message type " + m_MessageType + " not implemented", "");
        	}
        	     	
        	return response;
        }       

        private ServiceResponse ValidateXMLAttack(string inputXml)
        {
            try
            {
                StringReader textReader = new StringReader(inputXml);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.XmlResolver = null;
                settings.MaxCharactersInDocument = 0;

                XmlReader reader = XmlReader.Create(textReader, settings);
                while (reader.Read())
                {
                    ;
                }
            }
            catch (XmlException)
            {
                return new ServiceResponse(ResponseStatus.Failure, "XML rejected - failed XML entity expansion check", "");
            }

            return new ServiceResponse(ResponseStatus.Success, "", "");        	
        }
        
        private ServiceResponse ValidateXMLAgainstXSD(string inputXml)
        {
        	try
        	{
        		m_XmlValidated = true;
        		m_XmlValidationErrors = "";
        		        		
        		StringReader textReader = new StringReader(inputXml);        	
        		XmlReaderSettings settings = new XmlReaderSettings();
        		settings.Schemas.Add(null, "../../ServiceRequest.xsd");
        		settings.ValidationType = ValidationType.Schema;
        		settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
        		settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
        		
                XmlReader reader = XmlReader.Create(textReader, settings);
                while (reader.Read())
                {
                    ;
                } 

                if (!m_XmlValidated)
                    return new ServiceResponse(ResponseStatus.Failure, m_XmlValidationErrors, "");
        	}
        	catch (XmlException xe)
        	{
        		return new ServiceResponse(ResponseStatus.Failure, xe.ToString(), "");
        	}
        	
        	return new ServiceResponse(ResponseStatus.Success, "", "");
        }
        
        private void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            m_XmlValidated = false;
            if (args.Severity == XmlSeverityType.Warning)
            {
                if (m_XmlValidationErrors.Length > 0)
                    m_XmlValidationErrors += " - ";
                m_XmlValidationErrors += "Matching schema not found: " + args.Message;
            }
            else
            {
                if (m_XmlValidationErrors.Length > 0)
                    m_XmlValidationErrors += " - ";
                m_XmlValidationErrors += "XML validation error: " + args.Message;
            }
        }    
		
        private bool Initialize(string inputXml, out string error)
        {
        	m_InputXml = new XmlDocument();
        	m_InputXml.LoadXml(inputXml);
        	
        	m_XmlNamespaceManager = new XmlNamespaceManager(m_InputXml.NameTable);
        	m_XmlNamespaceManager.AddNamespace("x", "http://www.phonebook.com/ServiceRequest");
        	XmlNode messageTypeNode = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Header/x:MessageType", m_XmlNamespaceManager);
        	m_MessageType = messageTypeNode.InnerText;
        	
        	m_Phonebook = new Phonebook();
        	bool loaded = m_Phonebook.Load(out error);
        	if (!loaded)
        		return false;
        	
        	return true;
        }
        
        private ServiceResponse ListEntries()
        {
        	string surname = "";
        	XmlNode surnameNode = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:ListEntries/x:surname", m_XmlNamespaceManager);
        	if (surnameNode != null)
	        	surname = surnameNode.InnerText;
        	XmlDocument payload = new XmlDocument();
        	XmlElement root = payload.CreateElement("ListEntries");        	
        	
			foreach (PhonebookEntry entry in m_Phonebook)
			{
				if (entry.Surname == surname || surname == "")
				{
					XmlElement entryXml = payload.CreateElement("entry");
					XmlElement element = payload.CreateElement("id");
					element.InnerText = entry.Id;						
					entryXml.AppendChild(element);
					element = payload.CreateElement("surname");
					element.InnerText = entry.Surname;
					entryXml.AppendChild(element);
					element = payload.CreateElement("firstname");
					element.InnerText = entry.Firstname;
					entryXml.AppendChild(element);					
					element = payload.CreateElement("phone");
					element.InnerText = entry.Phone;
					entryXml.AppendChild(element);	
					if (entry.Address.Length > 0)
					{
						element = payload.CreateElement("address");
						element.InnerText = entry.Address;
						entryXml.AppendChild(element);							
					}
					root.AppendChild(entryXml);
				}
			}			
			payload.AppendChild(root);

			return new ServiceResponse(ResponseStatus.Success, "", payload.OuterXml);
        }
        
        private ServiceResponse CreateEntry()
        {
        	string surname, firstname, phone, address = "", error;
        	XmlNode node = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:CreateEntry/x:surname", m_XmlNamespaceManager);
        	surname = node.InnerText;
        	node = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:CreateEntry/x:firstname", m_XmlNamespaceManager);
        	firstname = node.InnerText;
        	node = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:CreateEntry/x:phone", m_XmlNamespaceManager);
        	phone = node.InnerText;
        	node = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:CreateEntry/x:address", m_XmlNamespaceManager);
        	if (node != null)
        		address = node.InnerText;
        	PhonebookEntry entry = new PhonebookEntry(surname, firstname, phone, address);
        	m_Phonebook.AddEntry(entry);
        	bool saved = m_Phonebook.Save(out error);
        	if (!saved)
        		return new ServiceResponse(ResponseStatus.Failure, "Phonebook not saved: " + error, "");
        	
        	return new ServiceResponse(ResponseStatus.Success, "Entry added successfully", "");
        }

        private ServiceResponse RemoveEntry()
        {
        	string error;
        	XmlNode node = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:RemoveEntry/x:id", m_XmlNamespaceManager);
        	bool entryPresent = m_Phonebook.RemoveEntry(node.InnerText);
        	bool saved = m_Phonebook.Save(out error);
        	if (!saved)
        		return new ServiceResponse(ResponseStatus.Failure, "Phonebook not saved: " + error, "");        	
        	if (entryPresent)        	
        		return new ServiceResponse(ResponseStatus.Success, "Entry removed successfully", "");
        		
        	return new ServiceResponse(ResponseStatus.Success, "Entry did not exist", "");
        }

        private ServiceResponse UpdateEntry()
        {
        	string id, surname, firstname, phone, address = "", error;
        	XmlNode node = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:UpdateEntry/x:id", m_XmlNamespaceManager);
        	id = node.InnerText;
        	node = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:UpdateEntry/x:surname", m_XmlNamespaceManager);
        	surname = node.InnerText;
        	node = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:UpdateEntry/x:firstname", m_XmlNamespaceManager);
        	firstname = node.InnerText;
        	node = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:UpdateEntry/x:phone", m_XmlNamespaceManager);
        	phone = node.InnerText;
        	node = m_InputXml.SelectSingleNode("//x:ServiceRequest/x:Payload/x:UpdateEntry/x:address", m_XmlNamespaceManager);
        	if (node != null)
        		address = node.InnerText;   	   
        	PhonebookEntry entry = new PhonebookEntry(id, surname, firstname, phone, address);
        	bool entryPresent = m_Phonebook.UpdateEntry(entry);
        	bool saved = m_Phonebook.Save(out error);
        	if (!saved)
				return new ServiceResponse(ResponseStatus.Failure, "Phonebook not saved: " + error, "");        		        	
        	if (entryPresent)
        		return new ServiceResponse(ResponseStatus.Success, "Entry updated successfully", "");
        		
       		return new ServiceResponse(ResponseStatus.Failure, "Entry does not exist", "");
        }        
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            WebServiceHost host = new WebServiceHost(typeof(PhonebookService), new Uri("http://localhost:8000/"));
            ServiceEndpoint ep = host.AddServiceEndpoint(typeof(IPhonebookService), new WebHttpBinding(), "");
            host.Open();
            
            /* Send request and receive response in code
            using (ChannelFactory<IPhonebookService> cf = new ChannelFactory<IPhonebookService>(new WebHttpBinding(), "http://localhost:8000"))
            {
                cf.Endpoint.Behaviors.Add(new WebHttpBehavior());                              
                IPhonebookService channel = cf.CreateChannel();
                
                string request, response;                
                // List all entries
                //request = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><ServiceRequest xmlns=\"http://www.phonebook.com/ServiceRequest\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Header><MessageID>1</MessageID><MessageType>ListEntries</MessageType></Header><Payload><ListEntries/></Payload></ServiceRequest>";             
                // List entries by surname
                //request = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><ServiceRequest xmlns=\"http://www.phonebook.com/ServiceRequest\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Header><MessageID>1</MessageID><MessageType>ListEntries</MessageType></Header><Payload><ListEntries><surname>Willcox</surname></ListEntries></Payload></ServiceRequest>";                
                // Create Entry
                //request = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><ServiceRequest xmlns=\"http://www.phonebook.com/ServiceRequest\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Header><MessageID>1</MessageID><MessageType>CreateEntry</MessageType></Header><Payload><CreateEntry><surname>Willcox</surname><firstname>John</firstname><phone>+44 207 184 8309</phone><address>1 London Wall</address></CreateEntry></Payload></ServiceRequest>";
                // Update Entry
                //request = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><ServiceRequest xmlns=\"http://www.phonebook.com/ServiceRequest\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Header><MessageID>1</MessageID><MessageType>UpdateEntry</MessageType></Header><Payload><UpdateEntry><id>E6209833-C5FD-4B67-AFB9-E1A02E2AD8D8</id><surname>Brown</surname><firstname>John</firstname><phone>+44 207 184 8309</phone><address>1 London Wall</address></UpdateEntry></Payload></ServiceRequest>";
				// Remove Entry
				//request = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><ServiceRequest xmlns=\"http://www.phonebook.com/ServiceRequest\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Header><MessageID>1</MessageID><MessageType>RemoveEntry</MessageType></Header><Payload><RemoveEntry><id>E6209833-C5FD-4B67-AFB9-E1A02E2AD8D8</id></RemoveEntry></Payload></ServiceRequest>";
                	
                response = channel.SubmitServiceRequest(request);
                
                Console.WriteLine("Request:{0}\n\nResponse:{1}\n", request, response);
            }
            */
               
           	// Or navigate to this address in browser
           	// http://localhost:8000/SubmitServiceRequest?inputXml=%3C?xml%20version=%221.0%22%20encoding=%22UTF-8%22?%3E%3CServiceRequest%20xmlns=%22http://www.phonebook.com/ServiceRequest%22%20xmlns:xsi=%22http://www.w3.org/2001/XMLSchema-instance%22%3E%3CHeader%3E%3CMessageID%3E1%3C/MessageID%3E%3CMessageType%3EListEntries%3C/MessageType%3E%3C/Header%3E%3CPayload%3E%3CListEntries/%3E%3C/Payload%3E%3C/ServiceRequest%3E

            Console.WriteLine("Listening...");
            Console.ReadLine();

            host.Close();
        }
    }
}
