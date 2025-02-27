﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using ING.iDealAdvanced.Data;
using ING.iDealAdvanced.Messages;
using Microsoft.Win32.SafeHandles;

namespace ING.iDealAdvanced
{
    /// <summary>
    /// This is the iDEAL Connector class that encapsulates the communication with the iDEAL service.
    /// </summary>
    /// <exception cref="XmlSchemaValidationException">Request Xml does not comply with schema.</exception>
    /// <exception cref="IDealException">Respons from iDEAL contains an error.</exception>
    /// <exception cref="UriFormatException">Url is not in correct format.</exception>
    /// <exception cref="InvalidCastException">Configuration setting has invalid format.</exception>
    /// <exception cref="ConfigurationErrorsException">One or more configuration settings are missing.</exception>
    /// <exception cref="WebException">Error getting reply from acquirer.</exception>
    /// <exception cref="CryptographicException">Error using client certificate.</exception>
    /// <exception cref="SecurityException">The iDEAL response signature is invalid.</exception>
    public class Connector : IConnector
    {
        /// <summary>
        /// This class holds the merchant configuration.
        /// </summary>
        private class MerchantConfig : ICloneable
        {
            /// <summary>
            /// The MerchantID
            /// </summary>
            public string MerchantId { get; set; }

            /// <summary>
            /// The Merchant SubId
            /// </summary>
            public string SubId { get; set; }

            /// <summary>
            /// The merchant return url
            /// </summary>
            public Uri merchantReturnUrl;

            /// <summary>
            /// The merchant certificate
            /// </summary>
            public X509Certificate2 ClientCertificate { get; set; }

            /// <summary>
            /// The expiration period
            /// </summary>
            public string ExpirationPeriod { get; set; }

            /// <summary>
            /// The acquirer certificate
            /// </summary>
            public X509Certificate2 aquirerCertificate;

            /// <summary>
            /// The acquirer url.
            /// </summary>
            public Uri acquirerURL;

            /// <summary>
            /// The acquirer directory url. Used for iTT simulation.
            /// </summary>
            public Uri acquirerUrlDIR;

            /// <summary>
            /// The acquirer transaction url. Used for iTT simulation.
            /// </summary>
            public Uri acquirerUrlTRA;

            /// <summary>
            /// The acquirer transaction status url. Used for iTT simulation.
            /// </summary>
            public Uri acquirerUrlSTA;

            /// <summary>
            /// The acquirer timeout
            /// </summary>
            public int acquirerTimeout;

            /// <summary>
            /// The currency
            /// </summary>
            public string currency;

            /// <summary>
            /// The language
            /// </summary>
            public string language;

            /// <summary>
            /// Clones the current object
            /// </summary>
            /// <returns></returns>
            public object Clone()
            {
                MerchantConfig merchantConfig = new MerchantConfig
                {
                    MerchantId = MerchantId,
                    SubId = SubId,
                    merchantReturnUrl = merchantReturnUrl,
                    ClientCertificate = ClientCertificate,
                    aquirerCertificate = aquirerCertificate,
                    acquirerURL = acquirerURL,
                    acquirerUrlDIR = acquirerUrlDIR,
                    acquirerUrlTRA = acquirerUrlTRA,
                    acquirerUrlSTA = acquirerUrlSTA,
                    acquirerTimeout = acquirerTimeout,
                    ExpirationPeriod = ExpirationPeriod,
                    currency = currency,
                    language = language
                };

                return merchantConfig;
            }
        }

        /// <summary>
        /// Gets the default merchant config.
        /// </summary>
        /// <exception cref="InvalidCastException">Configuration setting has invalid format.</exception>
        /// <exception cref="ConfigurationErrorsException">Configuration setting is missing.</exception>
        /// <exception cref="CryptographicException">Error getting certificate from the store.</exception>
        /// <exception cref="UriFormatException">Url is not in correct format.</exception>
        private MerchantConfig DefaultMerchantConfig(X509Certificate2 acquirerCertificate = null, X509Certificate2 clientCertificate = null,
            string merchantId = null, string merchantSubId = null, string acquirerUrl = null)
        {          
                if (defaultMerchantConfig == null)
                {
                    MerchantConfig newMerchant = new MerchantConfig
                    {
                        MerchantId = merchantId ?? GetAppSetting("MerchantID"),
                        SubId = merchantSubId ?? GetAppSetting("SubID")
                    };
                
                    string merchantReturnUrl = GetAppSetting("MerchantReturnURL");
                    if (!Uri.TryCreate(merchantReturnUrl, UriKind.Absolute, out newMerchant.merchantReturnUrl))
                        throw new UriFormatException("MerchantReturnURL is not in correct format.");

                    newMerchant.ClientCertificate = clientCertificate ?? GetCertificate(GetAppSetting("Privatecert"));
                    newMerchant.aquirerCertificate = acquirerCertificate ?? GetCertificate(GetAppSetting("Acquirercert"));

                    var acquirerUrlConfig = acquirerUrl ?? ConfigurationManager.AppSettings["AcquirerURL"];
                    var acquirerDirectoryUrl = ConfigurationManager.AppSettings["AcquirerDirectoryURL"];
                    var acquirerTransactionUrl = ConfigurationManager.AppSettings["AcquirerTransactionURL"];
                    var acquirerTransactionStatusUrl = ConfigurationManager.AppSettings["AcquirerTransactionStatusURL"];

                    if (!string.IsNullOrEmpty(acquirerUrlConfig))
                    {
                        //Check to see if other urls are given.

                        if (!string.IsNullOrEmpty(acquirerDirectoryUrl) ||
                            !string.IsNullOrEmpty(acquirerTransactionUrl) ||
                            !string.IsNullOrEmpty(acquirerTransactionStatusUrl))
                        {
                            throw new NotSupportedException("When acquirerURL is given then other URLs should not be supplied");
                        }

                        //We have acquirerURL. Use this url for all innner urls.
                        if (!Uri.TryCreate(acquirerUrlConfig, UriKind.Absolute, out newMerchant.acquirerURL)) throw new UriFormatException("AcquirerURL is not in correct format.");

                        newMerchant.acquirerUrlDIR = newMerchant.acquirerURL;
                        newMerchant.acquirerUrlTRA = newMerchant.acquirerURL;
                        newMerchant.acquirerUrlSTA = newMerchant.acquirerURL;
                    }
                    else
                    {
                        //Acquirer URL is not supplied. Try to get specific acquirer URLs
                        if (!Uri.TryCreate(acquirerDirectoryUrl, UriKind.Absolute, out newMerchant.acquirerUrlDIR)) throw new UriFormatException("AcquirerDirectoryURL is not in correct format.");
                        if (!Uri.TryCreate(acquirerTransactionUrl, UriKind.Absolute, out newMerchant.acquirerUrlTRA)) throw new UriFormatException("AcquirerTransactionURL is not in correct format.");
                        if (!Uri.TryCreate(acquirerTransactionStatusUrl, UriKind.Absolute, out newMerchant.acquirerUrlSTA)) throw new UriFormatException("AcquirerTransactionStatusURL is not in correct format.");
                    }
                    
                    string acquirerTimeout = GetAppSetting("AcquirerTimeout");
                    if (!int.TryParse(acquirerTimeout, out newMerchant.acquirerTimeout))
                        throw new InvalidCastException("AcquirerTimeout is not in correct format.");

                    newMerchant.ExpirationPeriod = GetOptionalAppSetting("ExpirationPeriod", null);

                    newMerchant.currency = "EUR";
                    newMerchant.language = "nl";

                    XmlSignature.XmlSignature.RegisterSignatureAlghorighm();

                    defaultMerchantConfig = newMerchant;
                }

                return defaultMerchantConfig;           
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets the merchant Id.
        /// </summary>
        public string MerchantId
        {
            get
            {
                return merchantConfig.MerchantId;
            }
            set
            {
                merchantConfig.MerchantId = value;
            }
        }

        /// <summary>
        /// Gets or sets the merchant SubId.
        /// </summary>
        public string SubId
        {
            get
            {
                return merchantConfig.SubId;
            }
            set
            {
                merchantConfig.SubId = value;
            }
        }

        /// <summary>
        /// The merchant return URL
        /// </summary>
        public Uri MerchantReturnUrl
        {
            get
            {
                return merchantConfig.merchantReturnUrl;
            }
            set
            {
                merchantConfig.merchantReturnUrl = value;
            }
        }

        /// <summary>
        /// The client certificate
        /// </summary>
        public X509Certificate2 ClientCertificate
        {
            get
            {
                return merchantConfig.ClientCertificate;
            }
            set
            {
                merchantConfig.ClientCertificate = value;
            }
        }

        /// <summary>
        /// The expiration period
        /// </summary>
        public string ExpirationPeriod
        {
            get
            {
                return merchantConfig.ExpirationPeriod;
            }
            set
            {
                merchantConfig.ExpirationPeriod = value;
            }
        }

        #endregion Public Properties

        private static MerchantConfig defaultMerchantConfig;

        private MerchantConfig merchantConfig;

        private static TraceSwitch traceSwitch = new TraceSwitch("iDealConnector", string.Empty);

        private List<string> validationErrors;

        private IConnector _connector;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="acquirerCertificate"><see cref="AcquirerCertificate" /> to be used, reverts to configuration if not set.</param>
        /// <param name="clientCertificate"><see cref="ClientCertificate" /> to be used, reverts to configuration if not set.</param>
        /// <param name="merchantId"></param>
        /// <param name="merchantSubId"></param>
        /// <param name="acquirerUrl"></param>
        /// <exception cref="UriFormatException">Url is not in correct format.</exception>
        /// <exception cref="InvalidCastException">Configuration setting has invalid format.</exception>
        /// <exception cref="ConfigurationErrorsException">Configuration setting is missing.</exception>
        private Connector(X509Certificate2 acquirerCertificate = null, X509Certificate2 clientCertificate = null,
            string merchantId = null, string merchantSubId = null, string acquirerUrl = null)
        {
            // Copy configuration from default configuration
            merchantConfig = (MerchantConfig)DefaultMerchantConfig(acquirerCertificate, clientCertificate, merchantId, merchantSubId, acquirerUrl).Clone();

            Resources.Culture = CultureInfo.CurrentCulture;
        }

        public static IConnector CreateConnector(X509Certificate2 acquirerCertificate = null, X509Certificate2 clientCertificate = null,
            string merchantId = null, string merchantSubId = null, string acquirerUrl = null, IConnector connectorMock = null)
        {
            var connector = new Connector(acquirerCertificate, clientCertificate, merchantId, merchantSubId, acquirerUrl);

            connector._connector = connectorMock ?? connector;

            return connector;
        }

        /// <summary>
        /// Retrieves both the short list and the long list of issuers.
        /// </summary>
        /// <returns><see cref="Issuers" /> containing the long list and short list of issuers, and the datetime stamp of the last change to the lists.</returns>
        /// <exception cref="XmlSchemaValidationException">Request Xml does not comply with schema.</exception>
        /// <exception cref="IDealException">Respons from iDEAL contains an error.</exception>
        /// <exception cref="ConfigurationErrorsException">Errors in configuration file.</exception>
        /// <exception cref="WebException">Error getting reply from acquirer.</exception>
        /// <exception cref="CryptographicException">Error using client certificate.</exception>
        public Issuers GetIssuerList()
        {
            if (traceSwitch.TraceInfo) TraceLine("Start of GetIssuerList()");

            DirectoryReq request = CreateRequest<DirectoryReq>();

            request.Merchant = CreateMerchant<DirectoryReqMerchant>();

            // Serialize the request to an XML string
            string xmlRequest = SerializationHelper.SerializeObject<DirectoryReq>(request);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlRequest);

            var _ = XmlSignature.XmlSignature.Sign(ref xmlDoc, _connector.GetMerchantRSACryptoServiceProvider(), merchantConfig.ClientCertificate.Thumbprint);

            xmlRequest = xmlDoc.OuterXml;

            // Validate the request before sending it to the service
            ValidateXML(xmlRequest);

            // Send request / get respons
            string xmlResponse = GetReplyFromAcquirer(xmlRequest, merchantConfig.acquirerUrlDIR);

            // Validate respons
            ValidateXML(xmlResponse);

            if (!XmlSignature.XmlSignature.CheckSignature(xmlResponse, merchantConfig.aquirerCertificate.GetRSAPublicKey()))
            {
                if (traceSwitch.TraceInfo) TraceLine("Xml response was not well signed " + xmlResponse);
                throw new ArgumentException("Response from server is not well signed");
            }

            if (traceSwitch.TraceInfo) TraceLine("Response from get issuer list was : " + xmlResponse);

            // Check respons for errors
            CheckError(xmlResponse, Resources.iDealUnavailable);

            DirectoryRes response = SerializationHelper.DeserializeObject<DirectoryRes>(xmlResponse);

            // Create the return object and initialze it with the iDEAL respons Directory
            var issuers = new Issuers(response.Directory);

            if (traceSwitch.TraceInfo) TraceLine("End of GetIssuerList()");
            if (traceSwitch.TraceVerbose) TraceLine(Format("Returnvalue: {0}", issuers.ToString()));

            return issuers;
        }


        /// <summary>
        /// Requests a transaction.
        /// </summary>
        /// <param name="transaction"><see cref="Transaction" /> to send.</param>
        /// <returns><see cref="Transaction" /> with added transaction ID and Issuer authentication URL.</returns>
        /// <exception cref="XmlSchemaValidationException">Request Xml does not comply with schema.</exception>
        /// <exception cref="IDealException">Respons from iDEAL contains an error.</exception>
        /// <exception cref="ConfigurationErrorsException">Errors in configuration file.</exception>
        /// <exception cref="UriFormatException">Returned issuer authentication Url is in invalid format.</exception>
        /// <exception cref="WebException">Error getting reply from acquirer.</exception>
        /// <exception cref="CryptographicException">Error using client certificate.</exception>
        public Transaction RequestTransaction(Transaction transaction)
        {
            if (traceSwitch.TraceInfo) TraceLine("Start of RequestTransaction()");
            if (traceSwitch.TraceVerbose) TraceLine(Format("Parameters: transaction: {0}", transaction == null ? "NULL" : transaction.ToString()));

            // Check input
            CheckMandatory("transaction", transaction);
            CheckMandatory("transaction.Amount", transaction.Amount);
            CheckMandatory("transaction.PurchaseId", transaction.PurchaseId);
            CheckMandatory("transaction.Description", transaction.Description);
            CheckMandatory("transaction.EntranceCode", transaction.EntranceCode);

            // Prepare the transaction request
            AcquirerTrxReq request = CreateRequest<AcquirerTrxReq>();

            request.Merchant = CreateMerchant<AcquirerTrxReqMerchant>();

            request.Transaction = new AcquirerTrxReqTransaction();

            request.Transaction.amount = transaction.Amount;

            request.Transaction.currency = merchantConfig.currency;
            request.Transaction.description = transaction.Description;
            request.Transaction.entranceCode = transaction.EntranceCode;
            request.Transaction.expirationPeriod = merchantConfig.ExpirationPeriod;

            request.Transaction.language = merchantConfig.language;

            request.Transaction.purchaseID = transaction.PurchaseId;

            request.Issuer = new AcquirerTrxReqIssuer();
            request.Issuer.issuerID = transaction.IssuerId;

            // Serialize the transaction request to an XML string
            string xmlRequest = SerializationHelper.SerializeObject<AcquirerTrxReq>(request);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlRequest);

            var signatureElement = XmlSignature.XmlSignature.Sign(ref xmlDoc, _connector.GetMerchantRSACryptoServiceProvider(), merchantConfig.ClientCertificate.Thumbprint);

            xmlRequest = xmlDoc.OuterXml;

            // Validate the request before sending it to the service
            ValidateXML(xmlRequest);

            // Send request / get respons
            string xmlRespons = GetReplyFromAcquirer(xmlRequest, merchantConfig.acquirerUrlTRA);

            // Validate respons
            ValidateXML(xmlRespons);

            if (!XmlSignature.XmlSignature.CheckSignature(xmlRespons, (RSA)merchantConfig.aquirerCertificate.PublicKey.Key))
            {
                Trace.WriteLine("Xml response was not well signed " + xmlRespons);
                throw new ArgumentException("Response from server is not well signed");
            }

            if (traceSwitch.TraceInfo) TraceLine("Response from RequestTransaction() was : " + xmlRespons);

            // Check respons for errors
            CheckError(xmlRespons, Resources.iDealUnavailable);

            AcquirerTrxRes respons = SerializationHelper.DeserializeObject<AcquirerTrxRes>(xmlRespons);

            transaction.Id = respons.Transaction.transactionID;
            // added in v3.3.x
            transaction.TransactionCreateDateTimestamp = respons.Transaction.transactionCreateDateTimestamp;

            string issuerAuthenticationURL = respons.Issuer.issuerAuthenticationURL;

            Uri outUri;
            if (!Uri.TryCreate(issuerAuthenticationURL, UriKind.Absolute, out outUri)) throw new UriFormatException("IssuerAuthenticationUrl is not in correct format.");
            transaction.IssuerAuthenticationUrl = outUri;

            transaction.AcquirerId = respons.Acquirer.acquirerID;

            if (traceSwitch.TraceInfo) TraceLine("End of RequestTransaction()");
            if (traceSwitch.TraceVerbose) TraceLine(Format("Returnvalue: {0}", transaction.ToString()));

            return transaction;
        }


        /// <summary>
        /// Requests the status of a transaction.
        /// </summary>
        /// <param name="transactionId">Id of the <see cref="Transaction" /> to check status for.</param>
        /// <returns><see cref="Transaction" /> holding status for the transaction.</returns>
        /// <exception cref="XmlSchemaValidationException">Request Xml does not comply with schema.</exception>
        /// <exception cref="IDealException">Respons from iDEAL contains an error.</exception>
        /// <exception cref="ConfigurationErrorsException">Errors in configuration file.</exception>
        /// <exception cref="WebException">Error getting reply from acquirer.</exception>
        /// <exception cref="CryptographicException">Error using client certificate.</exception>
        public Transaction RequestTransactionStatus(string transactionId)
        {
            if (traceSwitch.TraceInfo) TraceLine("Start of RequestTransactionStatus()");
            if (traceSwitch.TraceVerbose) TraceLine(Format("Parameters: transactionId: {0}", transactionId == null ? "NULL" : transactionId));

            // Check input
            CheckMandatory("transactionId", transactionId);

            AcquirerStatusReq request = CreateRequest<AcquirerStatusReq>();

            request.Merchant = CreateMerchant<AcquirerStatusReqMerchant>();

            request.Transaction = new AcquirerStatusReqTransaction();

            request.Transaction.transactionID = transactionId;

            // Serialize the request to an XML string
            string xmlRequest = SerializationHelper.SerializeObject<AcquirerStatusReq>(request);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlRequest);

            var _ = XmlSignature.XmlSignature.Sign(ref xmlDoc, _connector.GetMerchantRSACryptoServiceProvider(), merchantConfig.ClientCertificate.Thumbprint);

            xmlRequest = xmlDoc.OuterXml;

            // Validate the request before sending it to the service
            ValidateXML(xmlRequest);

            // Send request / get respons
            string xmlResponse = GetReplyFromAcquirer(xmlRequest, merchantConfig.acquirerUrlSTA);

            // Validate respons
            ValidateXML(xmlResponse);

            if (!XmlSignature.XmlSignature.CheckSignature(xmlResponse, (RSA)merchantConfig.aquirerCertificate.PublicKey.Key))
            {
                Trace.WriteLine("Xml response was not well signed " + xmlResponse);
                throw new ArgumentException("Response from server is not well signed");
            }

            if (traceSwitch.TraceInfo) TraceLine("Response from RequestTransactionStatus() was : " + xmlResponse);

            // Check respons for errors
            CheckError(xmlResponse, Resources.iDealStatusCheckFailed);

            AcquirerStatusRes response = SerializationHelper.DeserializeObject<AcquirerStatusRes>(xmlResponse);

            Transaction transaction = new Transaction();

            transaction.Id = response.Transaction.transactionID;
            transaction.AcquirerId = response.Acquirer.acquirerID;
            transaction.Status = (Transaction.TransactionStatus)Enum.Parse(typeof(Transaction.TransactionStatus), response.Transaction.status);
            transaction.ConsumerIBAN = response.Transaction.consumerIBAN;
            transaction.ConsumerBIC = response.Transaction.consumerBIC;
            transaction.StatusDateTimestamp = response.Transaction.statusDateTimestamp;
            transaction.ConsumerName = response.Transaction.consumerName;
            transaction.SignatureValue = response.Signature.SignatureValue.Value;
            transaction.Amount = response.Transaction.amount;
            transaction.Currency = response.Transaction.currency;

            if (traceSwitch.TraceInfo) TraceLine("End of RequestTransactionStatus()");
            if (traceSwitch.TraceVerbose) TraceLine(Format("Returnvalue: {0}", transaction.ToString()));

            return transaction;
        }


        /// <summary>
        /// Sends a request to the merchant acquirerUrl and returns the reply it receives.
        /// </summary>
        /// <param name="request">Request to send.</param>
        /// <param name="url">The url used to send the request</param>
        /// <returns>Reply from merchant acquirerUrl.</returns>
        /// <exception cref="WebException">Error getting reply from acquirer.</exception>
        private string GetReplyFromAcquirer(string request, Uri url)
        {
            if (traceSwitch.TraceInfo) TraceLine("Start of GetReplyFromAcquirer()");
            if (traceSwitch.TraceVerbose) TraceLine(Format("Parameters: request: {0}", request));

            string reply;

            Encoding encoding = new UTF8Encoding(false);

            try
            {
                byte[] bytesToSend = encoding.GetBytes(request);

                // Send the xml to remote server
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

#if RUNNING_ON_4_5
                // Bypass service certificate validation
                var disableAcquirerSSLCertificateValidation = GetOptionalAppSetting("DisableAcquirerSSLCertificateValidation", "False");
                if (disableAcquirerSSLCertificateValidation.ToLowerInvariant().Equals("true"))
                {
                    httpWebRequest.ServerCertificateValidationCallback += DisableAcquirerSSLCertificateValidation;
                }                            
#endif

                // Set timeout in milliseconds (ms)
                httpWebRequest.Timeout = merchantConfig.acquirerTimeout * 1000;

                // Do a HTTP POST, content is the xml message
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType =
                    Format("text/xml; charset=\"{0}\"", encoding.WebName);
                httpWebRequest.ContentLength = bytesToSend.Length;

                using (Stream sendStream = httpWebRequest.GetRequestStream())
                {
                    sendStream.Write(bytesToSend, 0, bytesToSend.Length);
                }

                // Get the response from the server
                WebResponse response = httpWebRequest.GetResponse();

                // Create a readable stream
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    // Read in all the server answer
                    reply = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                if (traceSwitch.TraceError) TraceLine("Exception caught in SendXMLRequest()");
                if (traceSwitch.TraceError) TraceLine(Format("Exception message: {0}", e.Message));
                throw new WebException("Error getting reply from acquirer, look at inner exception for details.", e);
            }


            if (traceSwitch.TraceInfo) TraceLine("End of GetReplyFromAcquirer()");
            if (traceSwitch.TraceVerbose) TraceLine(Format("Returnvalue: {0}", reply));
            return reply;
        }

#if RUNNING_ON_4_5
        /// <summary>
        /// Disables the the acquirer's certificate validation
        /// </summary>
        private static bool DisableAcquirerSSLCertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }
#endif

        /// <summary>
        /// Creates a Merchant object.
        /// </summary>
        /// <typeparam name="T">Type for the Merchant object to create</typeparam>
        /// <returns>Merchant object created from this instance's <see cref="MerchantConfig"/>.</returns>
        private T CreateMerchant<T>() where T : IMerchant, new()
        {
            T merchant = new T();

            CheckMandatory("MerchantId", merchantConfig.MerchantId);
            CheckMandatory("SubId", merchantConfig.SubId);

            merchant.merchantID = merchantConfig.MerchantId;
            merchant.subID = merchantConfig.SubId;

            AcquirerTrxReqMerchant acquirerTrxReqMerchant = merchant as AcquirerTrxReqMerchant;
            if (acquirerTrxReqMerchant != null)
                acquirerTrxReqMerchant.merchantReturnURL = merchantConfig.merchantReturnUrl.ToString();

            return merchant;
        }


        /// <summary>
        /// Creates a service request.
        /// </summary>
        /// <typeparam name="T">Type of tyhe service request to create.</typeparam>
        /// <returns>Service request with current timestamp and default version number.</returns>
        private static T CreateRequest<T>() where T : IRequest, new()
        {
            T request = new T();

            request.createDateTimestamp = DateTime.Now;
            request.version = GlobalConstants.VersionNumber;

            return request;
        }


        /// <summary>
        /// Gets an X509 certificate.
        /// </summary>
        /// <param name="subjectOrThumbprint">Subject or Thumbprint string for the certificate to get.</param>
        /// <returns><see cref="X509Certificate2"/>.</returns>
        /// <exception cref="CryptographicException">Error getting certificate from the store.</exception>
        /// <exception cref="ConfigurationErrorsException">Number of certificates found is not exactly one.</exception>
        internal static X509Certificate2 GetCertificate(string subjectOrThumbprint)
        {
            X509Certificate2 cert = null;

            WindowsIdentity.RunImpersonated(
                SafeAccessTokenHandle.InvalidHandle,
                () =>
                {
                    cert = TryGetCertificateFromStore(subjectOrThumbprint);
                }
            );

            return cert;
        }

        internal static X509Certificate2 TryGetCertificateFromStore(string subjectOrThumbprint)
        {
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);

                // Change: Product Backlog Item 10247: .NET Connector - support loading certificate by Thumbprint
                // By default find certificates using SubjectName
                X509FindType findType = X509FindType.FindBySubjectName;

                // Check to see if finding certificates by Thumbprint is activated in the configuration settings
                var findCertificatesByThumbprint = GetOptionalAppSetting("FindCertificatesByThumbprint", "False");

                if (findCertificatesByThumbprint.ToLowerInvariant().Equals("true"))
                    findType = X509FindType.FindByThumbprint;

                X509Certificate2Collection certs = store.Certificates.Find(findType, subjectOrThumbprint, false);
                if (certs.Count != 1)
                {
                    string errMsg = Format("Found {0} certificates by subject/thumbprint {1}, expected 1.", certs.Count, subjectOrThumbprint);
                    if (traceSwitch.TraceError) TraceLine(errMsg);
                    throw new ConfigurationErrorsException(errMsg);
                }

                return certs[0];
            }
        }

        /// <summary>
        /// Gets the merchant crypto service provider.
        /// </summary>
        /// <returns></returns>
        RSA IConnector.GetMerchantRSACryptoServiceProvider()
        {
            RSA rsa;

            try
            {
                var useCertificateWithEnhancedAESCryptoProvider = GetOptionalAppSetting("UseCertificateWithEnhancedAESCryptoProvider", "False");
                if (useCertificateWithEnhancedAESCryptoProvider.ToLowerInvariant().Equals("true"))
                {
                    rsa = (RSA) merchantConfig.ClientCertificate.PrivateKey;
                }
                else
                {
                    var cspParams = new CspParameters(24) {KeyContainerName = "XML_DSIG_RSA_KEY"};

                    // Change: Product Backlog Item 10248: .NET Connector - support loading certificate from machine key stores instead of user’s
                    // Use machine key store if this is specified in the configuration settings
                    var useMachineKeyStore = GetOptionalAppSetting("UseCspMachineKeyStore", "False");
                    if (useMachineKeyStore.ToLowerInvariant().Equals("true"))
                    {
                        cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
                    }

                    rsa = new RSACryptoServiceProvider(cspParams);

                    //The following line may fail if PrivateKey throws an Exception
                    //If the exception is "keyset not found" then check if the hosted website runs on IIS impersonation level
                    //Go to the certificate and manage private keys and add read access to the website application pool.
                    rsa.FromXmlString(merchantConfig.ClientCertificate.PrivateKey.ToXmlString(true));
                }
            }
            catch (CryptographicException ex)
            {
                if (traceSwitch.TraceError)
                {
                    string errMsg = Format("Error while reading private key: The key value is not an RSA or DSA key, or the key is unreadable. Current windows account: {0}, Original message: {1}", WindowsIdentity.GetCurrent().Name, ex.Message);
                    TraceLine(errMsg);
                }
                throw;
            }

            return rsa;
        }


        /// <summary>
        /// Validates Xml against the schema.
        /// </summary>
        /// <param name="xml">Xml to validate.</param>
        /// <exception cref="XmlSchemaValidationException">Schema validation error.</exception>
        private void ValidateXML(string xml)
        {
            // Reset validation error list
            validationErrors = new List<string>();

            XmlDocument xmlDoc = new XmlDocument();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.XmlResolver = null;
            settings.DtdProcessing = DtdProcessing.Ignore;



            using (StringReader rdr = new StringReader(xml))
            {
                using (XmlReader reader = XmlReader.Create(rdr, settings))
                {
                    xmlDoc.Load(reader);
                }
            }

            xmlDoc.Schemas.Add(Security.XsdValidation.AllSchemas);

            xmlDoc.Validate(ValidationError);

            // Check validation error list
            if (validationErrors.Count > 0)
            {
                string errors = string.Empty;

                foreach (string validationError in validationErrors)
                    errors += (validationError + Environment.NewLine);

                throw new XmlSchemaValidationException(Format("XML message contains errors: {0}", errors));
            }
        }

        /// <summary>
        /// ValidationEventHandler call-back method.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="arguments"><see cref="ValidationEventArgs"/> containing detailed information about the validation error.</param>
        private void ValidationError(object sender, ValidationEventArgs arguments)
        {
            validationErrors.Add(arguments.Message);
        }

        /// <summary>
        /// Checks if Xml contains an ErrorRes element, if so deserialize the ErrorRes, wrap it in an iDealException and throw.
        /// </summary>
        /// <param name="xml">Xml to check.</param>
        /// <param name="consumerMessage">Consumer message to use if errorRes.Error.consumerMessage is empty or null.</param>
        private static void CheckError(string xml, string consumerMessage)
        {
            if (xml.Contains("<AcquirerErrorRes"))
            {
                AcquirerErrorRes errorRes = SerializationHelper.DeserializeObject<AcquirerErrorRes>(xml);

                // Set consumerMessage if it has not been set by the iDEAL service
                if (string.IsNullOrEmpty(errorRes.Error.consumerMessage))
                    errorRes.Error.consumerMessage = consumerMessage;

                throw new IDealException(errorRes);
            }
        }

        /// <summary>
        /// ValidationEventHandler call-back method.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="arguments"><see cref="ValidationEventArgs"/> containing detailed information about the validation error.</param>
        /// <exception cref="XmlSchemaValidationException">Schema validation error.</exception>
        private static void SchemaError(object sender, ValidationEventArgs arguments)
        {
            throw new XmlSchemaValidationException(Format("XML schema contains error: {0}", arguments.Message));
        }

        /// <summary>
        /// Traces a line of text.
        /// </summary>
        /// <param name="line">Text to trace.</param>
        private static void TraceLine(string line)
        {
            Trace.WriteLine(line);
            Trace.Flush();
        }

        /// <summary>
        /// Gets a value from the AppSettings.
        /// </summary>
        /// <param name="key">Key for the value to get.</param>
        /// <returns>Value as a string.</returns>
        /// <exception cref="ConfigurationErrorsException">Key does not exist.</exception>
        private static string GetAppSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrEmpty(value))
                throw new ConfigurationErrorsException(Format("Cannot find key in appSettings for \"{0}\", check your web.config file.", key));

            return value;
        }

        /// <summary>
        /// Gets an optional value from the AppSettings.
        /// </summary>
        /// <param name="key">Key for the value to get.</param>
        /// <param name="defaultValue">Value to use if key is not available.</param>
        /// <returns>Value as a string.</returns>
        private static string GetOptionalAppSetting(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return value;
        }

        /// <summary>
        /// Checks if a mandatory object is not null.
        /// </summary>
        /// <param name="name">Name of the object to use in exception message/</param>
        /// <param name="value">Object to check.</param>
        /// <exception cref="InvalidOperationException">Object contains null.</exception>
        private static void CheckMandatory(string name, object value)
        {
            if (value == null) throw new InvalidOperationException(Format("{0} should not be null.", name));
        }

        /// <summary>
        /// Removes all white space from a string.
        /// </summary>
        /// <param name="text">String to remove white space from.</param>
        /// <returns>String with white space removed.</returns>
        private static string RemoveWhiteSpace(string text)
        {
            return Regex.Replace(text, @"\s+", string.Empty);

        }

        /// <summary>
        /// Formats a string using the current culture.
        /// </summary>
        /// <param name="stringToFormat">String to format.</param>
        /// <param name="args">Replace parameters.</param>
        /// <returns>Formatted string.</returns>
        /// <exception cref="FormatException">Error while formatting string.</exception>
        private static string Format(string stringToFormat, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, stringToFormat, args);
        }
    }
}
