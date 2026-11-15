using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ING.iDealAdvanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace iDealAdvancedConnector_10_0.Tests
{
    [TestClass]
    public sealed class AcquirerPostTests
    {
        private const string ContentType = "text/xml; charset=\"utf-8\"";

        [TestMethod]
        public async Task PostToAcquirer_SendsXmlAsPostAndReturnsTheReply()
        {
            // Arrange
            TestContext context = CreateContext();
            Task<string> serverReceived = ServeOnce(context.Listener, "200 OK", "<reply>ok</reply>");
            byte[] content = Encoding.UTF8.GetBytes("<request>é</request>");

            // Act
            string reply = Connector.PostToAcquirer(content, ContentType, context.Url, 10);

            // Assert
            string received = await serverReceived;
            Assert.AreEqual("<reply>ok</reply>", reply);
            StringAssert.StartsWith(received, "POST /ideal ");
            StringAssert.Contains(received.ToLowerInvariant(), "content-type: " + ContentType);
            StringAssert.EndsWith(received, "<request>é</request>");
        }

        [TestMethod]
        public async Task PostToAcquirer_ErrorStatus_Throws()
        {
            // Arrange
            TestContext context = CreateContext();
            Task<string> serverReceived = ServeOnce(context.Listener, "500 Internal Server Error", "<error />");
            byte[] content = Encoding.UTF8.GetBytes("<request />");

            // Act
            Exception exception = Assert.Throws<Exception>(() => Connector.PostToAcquirer(content, ContentType, context.Url, 10));

            // Assert
            await serverReceived;
            Assert.IsNotNull(exception);
        }

        private static async Task<string> ServeOnce(TcpListener listener, string status, string body)
        {
            using (TcpClient client = await listener.AcceptTcpClientAsync())
            {
                NetworkStream stream = client.GetStream();
                string head = await ReadHead(stream);

                if (head.IndexOf("Expect: 100-continue", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    await Write(stream, "HTTP/1.1 100 Continue\r\n\r\n");
                }

                string requestBody = await ReadBody(stream, ContentLength(head));
                byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
                await Write(stream, $"HTTP/1.1 {status}\r\nContent-Type: text/xml; charset=utf-8\r\nContent-Length: {bodyBytes.Length}\r\nConnection: close\r\n\r\n{body}");
                listener.Stop();

                return head + requestBody;
            }
        }

        private static async Task<string> ReadHead(Stream stream)
        {
            MemoryStream head = new MemoryStream();
            byte[] buffer = new byte[1];

            while (!EndsWithBlankLine(head))
            {
                await stream.ReadExactlyAsync(buffer, 0, 1);
                head.WriteByte(buffer[0]);
            }

            return Encoding.ASCII.GetString(head.ToArray());
        }

        private static bool EndsWithBlankLine(MemoryStream head)
        {
            byte[] bytes = head.ToArray();

            return bytes.Length >= 4 && Encoding.ASCII.GetString(bytes, bytes.Length - 4, 4) == "\r\n\r\n";
        }

        private static int ContentLength(string head)
        {
            foreach (string line in head.Split("\r\n"))
            {
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                {
                    return int.Parse(line.Substring("Content-Length:".Length).Trim());
                }
            }

            return 0;
        }

        private static async Task<string> ReadBody(Stream stream, int length)
        {
            byte[] body = new byte[length];
            await stream.ReadExactlyAsync(body, 0, length);

            return Encoding.UTF8.GetString(body);
        }

        private static async Task Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private static TestContext CreateContext()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            return new TestContext(listener, new Uri($"http://127.0.0.1:{port}/ideal"));
        }

        private sealed class TestContext
        {
            public TestContext(TcpListener listener, Uri url)
            {
                Listener = listener;
                Url = url;
            }

            public TcpListener Listener { get; }
            public Uri Url { get; }
        }
    }
}
