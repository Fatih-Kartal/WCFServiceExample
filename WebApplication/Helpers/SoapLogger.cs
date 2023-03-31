using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace WebApplication.Helper
{
    public class LoggingEndpointBehaviour : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new LoggingMessageInspector());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {

        }

        public void Validate(ServiceEndpoint endpoint)
        {

        }
    }

    public class LoggingMessageInspector : IClientMessageInspector
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string SoapLogRequestTemplate = "Action : {2}\n{0}Date : {1:dd/MM/yyyy HH:mm:ss - yyyy-MM-dd HH:mm:ss}\n \n{0} Message \n{3}";
        private static readonly string SoapLogResponseTemplate = /*         */"{0}Date : {1:dd/MM/yyyy HH:mm:ss - yyyy-MM-dd HH:mm:ss}\n \n{0} Message \n{2}";

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            try
            {
                using (var buffer = reply.CreateBufferedCopy(int.MaxValue))
                {
                    var document = GetDocument(buffer.CreateMessage());

                    Logger.Info(GetLogText(LogType.Response, reply.Headers.Action, document.OuterXml));

                    reply = buffer.CreateMessage();
                }
            }
            catch (System.Exception e)
            {
                throw;
            }
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            try
            {
                using (var buffer = request.CreateBufferedCopy(int.MaxValue))
                {
                    var document = GetDocument(buffer.CreateMessage());
                    Logger.Info(GetLogText(LogType.Request, request.Headers.Action, document.OuterXml));

                    request = buffer.CreateMessage();
                    return null;
                }
            }
            catch (System.Exception e)
            {
                throw;
            }
        }

        public enum LogType
        {
            Request,
            Response
        }

        public string GetLogText(LogType logType, string action, string message)
        {
            if (logType == LogType.Request)
            {
                return Draw(string.Format(SoapLogRequestTemplate, logType, DateTime.Now, action, PrettyXml(message)));
            }
            else
            {
                return Draw(string.Format(SoapLogResponseTemplate, logType, DateTime.Now, PrettyXml(message)));
            }
        }

        private XmlDocument GetDocument(Message request)
        {
            XmlDocument document = new XmlDocument();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // write request to memory stream
                XmlWriter writer = XmlWriter.Create(memoryStream);
                request.WriteMessage(writer);
                writer.Flush();
                memoryStream.Position = 0;

                // load memory stream into a document
                document.Load(memoryStream);
            }

            return document;
        }

        private static string PrettyXml(string xml)
        {
            return XDocument.Parse(xml).ToString();
        }

        public static string Draw(string s)
        {
            string corner = "+";
            string edge = "-";

            string[] lines = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);


            int longest = 0;
            foreach (string line in lines)
            {
                if (line.Length > longest)
                    longest = line.Length;
            }
            int width = longest + 2; // 1 space on each side


            string h = string.Empty;
            for (int i = 0; i < width; i++)
                h += edge;

            // box top
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(corner + h + corner);

            // box contents
            foreach (string line in lines)
            {
                // add the text line to the box
                sb.AppendLine(" " + line);
            }

            // box bottom
            sb.AppendLine(corner + h + corner);

            // the finished box
            return sb.ToString();
        }
    }
}
