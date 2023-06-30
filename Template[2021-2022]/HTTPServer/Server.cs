using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;

        public Server(int portNumber, string redirectionMatrixPath)
        {
            //TODO: call this.LoadRedirectionRules passing redirectionMatrixPath to it
            LoadRedirectionRules(redirectionMatrixPath);
            //TODO: initialize this.serverSocket
            serverSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portNumber);
            serverSocket.Bind(ipEnd);
        }

        public void StartServer()
        {
            // TODO: Listen to connections, with large backlog.
            serverSocket.Listen(1000);
            // TODO: Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                //TODO: accept connections and start thread for each accepted connection.
                Socket client = this.serverSocket.Accept();
                Console.WriteLine("New Client: {0}", client.RemoteEndPoint);
                Thread newThread = new Thread(new ParameterizedThreadStart(HandleConnection));
                newThread.Start(client);

            }
        }

        public void HandleConnection(object obj)
        {
            // TODO: Create client socket 
            Socket socket = (Socket)obj;
            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            socket.ReceiveTimeout = 0;
           
            // TODO: receive requests in while true until remote client closes the socket.
            while (true)
            {
                try
                {
                    // TODO: Receive request
                    byte[] data = new byte[1024];
                    int messageLength = socket.Receive(data);
                    // TODO: break the while loop if receivedLen==0
                    if (messageLength == 0)
                    {
                        Console.WriteLine("Client: {0} ended the connection", socket.RemoteEndPoint);
                        break;
                    }
                    // TODO: Create a Request object using received request string
                    Request requestObj = new Request(Encoding.ASCII.GetString(data, 0, messageLength));
                    // TODO: Call HandleRequest Method that returns the response
                    Response response =  this.HandleRequest(requestObj);
                    // TODO: Send Response back to client
                    socket.Send(Encoding.ASCII.GetBytes(response.ResponseString));
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    // TODO: log exception using Logger class
                }
            }
            // TODO: close client socket
            socket.Close();
        }

        Response HandleRequest(Request request)
        {
            //throw new NotImplementedException();
            string content;
            Response response = null;
            try
            {
                //throw new Exception();
                bool requestSuccess = request.ParseRequest();
                if (!requestSuccess)
                {
                    content = LoadDefaultPage(Configuration.BadRequestDefaultPageName);
                    return new Response(StatusCode.BadRequest, "text/html", content, string.Empty);
                }

                //TODO: map the relativeURI in request to get the physical path of the resource.
                string physicalPath = Configuration.RootPath + '\\' + request.relativeURI;
                //TODO: check for redirect
                string redirectpath = GetRedirectionPagePathIFExist(request.relativeURI);
                if (redirectpath != string.Empty)
                {
                    Console.WriteLine("redirecting");
                    content = LoadDefaultPage(Configuration.RedirectionDefaultPageName);
                    return new Response(StatusCode.Redirect, "text/html", content, redirectpath);
                }

                //TODO: check file exists
                bool fileExist = File.Exists(physicalPath);
                if (!fileExist)
                {

                    content = LoadDefaultPage(Configuration.NotFoundDefaultPageName);
                    return new Response(StatusCode.NotFound, "text/html", content, string.Empty);
                }
                //TODO: read the physical file

                // Create OK response
                content = LoadDefaultPage(request.relativeURI);
               // Console.WriteLine(content);
                response = new Response(StatusCode.OK, "text/html", content, string.Empty);
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                // TODO: in case of exception, return Internal Server Error.
                content = LoadDefaultPage(Configuration.InternalErrorDefaultPageName);
                return new Response(StatusCode.InternalServerError, "text/html", content, string.Empty);
            }
            return response;
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
            Console.WriteLine("relative path: " + relativePath);
            string redirectedPath = string.Empty;
            if (Configuration.RedirectionRules.ContainsKey(relativePath))
                redirectedPath = Configuration.RedirectionRules[relativePath];

            return redirectedPath;
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            //string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            string filePath = Configuration.RootPath + '\\' + defaultPageName;
            // TODO: check if filepath not exist log exception using Logger class and return empty string
            if (!File.Exists(filePath))
            {
                Logger.LogException(new FileNotFoundException("file not exist"));
                return string.Empty;
            }
            // else read file and return its content
            else {
                return File.ReadAllText(filePath);
            }
        }

        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                // TODO: using the filepath paramter read the redirection rules from file 
                // then fill Configuration.RedirectionRules dictionary 
                string [] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] rules = line.Split(',');
                    // then fill Configuration.RedirectionRules dictionary 
                    Configuration.RedirectionRules.Add(rules[0], rules[1]);
                }

            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }
    }
}
