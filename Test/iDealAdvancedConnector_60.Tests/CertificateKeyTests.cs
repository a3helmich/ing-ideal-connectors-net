using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using ING.iDealAdvanced;
using ING.iDealAdvanced.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlSignatureHelper = ING.iDealAdvanced.XmlSignature.XmlSignature;

namespace iDealAdvancedConnector_10_0.Tests
{
    [TestClass]
    public sealed class CertificateKeyTests
    {
        [TestMethod]
        public void GetClientPrivateKey_ReturnsTheKeyOfTheCertificate()
        {
            // Arrange
            TestContext context = CreateContext();

            // Act
            RSA privateKey = Connector.GetClientPrivateKey(context.Certificate);

            // Assert
            CollectionAssert.AreEqual(context.Certificate.GetRSAPublicKey().ExportParameters(false).Modulus, privateKey.ExportParameters(false).Modulus);
        }

        [TestMethod]
        public void GetClientPrivateKey_ExportableCertificate_ExportsThePrivateKeyAsXml()
        {
            // Arrange
            TestContext context = CreateContext();

            // Act
            string xml = Connector.GetClientPrivateKey(context.Certificate).ToXmlString(true);

            // Assert
            StringAssert.Contains(xml, "<D>");
        }

        [TestMethod]
        public void CheckSignature_SignedWithClientKey_VerifiesWithThePublicKeyOfTheCertificate()
        {
            // Arrange
            TestContext context = CreateContext();
            XmlDocument document = SignedDocument(context);

            // Act
            bool valid = XmlSignatureHelper.CheckSignature(document.OuterXml, context.Certificate.GetRSAPublicKey());

            // Assert
            Assert.IsTrue(valid);
        }

        [TestMethod]
        public void CheckSignature_ChangedAfterSigning_IsRejected()
        {
            // Arrange
            TestContext context = CreateContext();
            string tampered = SignedDocument(context).OuterXml.Replace("<amount>10.00</amount>", "<amount>99.00</amount>");

            // Act
            bool valid = XmlSignatureHelper.CheckSignature(tampered, context.Certificate.GetRSAPublicKey());

            // Assert
            Assert.IsFalse(valid);
        }

        [TestMethod]
        public void CreateDigest_HashesWithSha256()
        {
            // Arrange
            RSAPKCS1SHA256SignatureDescription description = new RSAPKCS1SHA256SignatureDescription();

            // Act
            byte[] hash = description.CreateDigest().ComputeHash(System.Text.Encoding.ASCII.GetBytes("abc"));

            // Assert
            Assert.AreEqual("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", Convert.ToHexString(hash));
        }

        private static XmlDocument SignedDocument(TestContext context)
        {
            XmlDocument document = new XmlDocument { PreserveWhitespace = true };
            document.LoadXml("<transaction><amount>10.00</amount></transaction>");
            XmlSignatureHelper.Sign(ref document, context.Certificate.GetRSAPrivateKey(), context.Certificate.GetCertHash());

            return document;
        }

        private static TestContext CreateContext()
        {
            XmlSignatureHelper.RegisterSignatureAlghorighm();

            using (RSA key = RSA.Create(2048))
            {
                CertificateRequest request = new CertificateRequest("CN=iDEAL test merchant", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                using (X509Certificate2 selfSigned = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1)))
                {
                    X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12(selfSigned.Export(X509ContentType.Pfx), null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);

                    return new TestContext(certificate);
                }
            }
        }

        private sealed class TestContext
        {
            public TestContext(X509Certificate2 certificate)
            {
                Certificate = certificate;
            }

            public X509Certificate2 Certificate { get; }
        }
    }
}
