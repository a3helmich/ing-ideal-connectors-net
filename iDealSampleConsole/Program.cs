using System;
using System.Security.Cryptography;
using System.Xml;
using ING.iDealAdvanced.Security;
using ING.iDealAdvanced.XmlSignature;

namespace iDealSampleConsole
{
    class Program
    {
        static void Main()
        {
            Method2();
        }

        private static void Method2()
        {
            Console.WriteLine(@"Press enter to start the test");
            Console.ReadLine();
            CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");

            // Create a new XML document.
            var doc = new XmlDocument();
            // Format the document to ignore white spaces.
            doc.PreserveWhitespace = false;
            // Load the passed XML 
            var my_xml = "<root><test>test</test></root>";
            doc.LoadXml(my_xml);

            var conn = ING.iDealAdvanced.Connector.CreateConnector();

            var cert = conn.ClientCertificate;
            var key = conn.GetMerchantRSACryptoServiceProvider();

            XmlSignature.Sign(ref doc, key, cert.Thumbprint);

            Console.WriteLine(doc.OuterXml);
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine(@"Ended");
            Console.ReadLine();
        }
    }
}
