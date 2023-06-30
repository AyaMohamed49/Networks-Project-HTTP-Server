using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Call CreateRedirectionRulesFile() function to create the rules of redirection 
            CreateRedirectionRulesFile();

            //Start server
            // 1) Make server object on port 1000
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "redirectionRules.txt";
            Server server = new Server(1000, filePath);
            // 2) Start Server
            server.StartServer();
        }

        static void CreateRedirectionRulesFile()
        {
            // TODO: Create file named redirectionRules.txt
            // each line in the file specify a redirection rule
            // example: "aboutus.html,aboutus2.html"
            // means that when making request to aboustus.html,, it redirects me to aboutus2
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "redirectionRules.txt";
            FileStream fileStream = new FileStream(filePath,FileMode.OpenOrCreate);
            Console.WriteLine(filePath);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.WriteLine("aboutus.html,aboutus2.html");
            fileStream.Close();

        }
         
    }
}
