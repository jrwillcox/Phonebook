using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Phonebook
{
    [ServiceContract]
    public interface IPhonebookService
    {
    	[OperationContract]
    	[WebInvoke(Method="GET")]
        string SubmitServiceRequest(string inputXml);       
    }
}
