﻿
namespace Hardware.Extension.EPSPaymentConnector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.HardwareStation;
    using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;
    using Microsoft.Dynamics.Commerce.HardwareStation.PeripheralRequests;
    using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
    using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities;
    using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.PaymentTerminal;
    using Microsoft.Dynamics.Commerce.Runtime.Handlers;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.VirtualPeripherals.Framework;
    using Microsoft.Dynamics.Commerce.VirtualPeripherals.MessagePipelineHandler;
    using Microsoft.Dynamics.Commerce.VirtualPeripherals.MessagePipelineProxy;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using Newtonsoft.Json.Linq;
    using PSDK = Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Commerce.Runtime;
    using System.Net;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// <c>Simulator</c> manager payment device class.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "long running task, so the file system watcher will be disposed when the program ends")]
    public class EPSConnector : INamedRequestHandler //, IRequestHandler//,IPaymentDevice
    {
        private const string PaymentTerminalDevice = "GSPAYMENTTERMINAL";
        private const string PaymentDeviceSimulatorFileName = "PaymentDeviceSimulator";
        private const int TaskDelayInMilliSeconds = 10;

        #region File Logger variables
        private const string FILE_EXT = ".log";
        private readonly string datetimeFormat;
        private readonly string logFilename;
        #endregion
        // Cache to store credit card number, the key will be returned to client in Authorization payment sdk blob.
        private static Dictionary<Guid, TemporaryCardMemoryStorage<string>> cardCache = new Dictionary<Guid, TemporaryCardMemoryStorage<string>>();
        private readonly string deviceSimFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
        private CancellationTokenSource timeoutTask;
        private PSDK.PaymentProperty[] merchantProperties;
        private SettingsInfo terminalSettings;
        private string paymentConnectorName;
        private TenderInfo tenderInfo;
        private PSDK.IPaymentProcessor processor;
        
        #region ctor
        public EPSConnector()
        {
            datetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
            logFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + FILE_EXT;

            // Log file header line
            string logHeader = logFilename + " is created.";
            if (!System.IO.File.Exists(logFilename))
            {
                WriteLog(System.DateTime.Now.ToString(datetimeFormat) + " " + logHeader, false);
            }

            WriteLog("Entered constructor for method: EPSConnector ", true);
        }
        #endregion

        /// <summary>
        /// Gets the collection of supported request types by this handler.
        /// </summary>
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                        typeof(LockPaymentTerminalDeviceRequest),
                        typeof(ReleasePaymentTerminalDeviceRequest),
                        typeof(OpenPaymentTerminalDeviceRequest),
                        typeof(BeginTransactionPaymentTerminalDeviceRequest),
                        typeof(UpdateLineItemsPaymentTerminalDeviceRequest),
                        typeof(CancelOperationPaymentTerminalDeviceRequest),
                        typeof(AuthorizePaymentTerminalDeviceRequest),
                        typeof(CapturePaymentTerminalDeviceRequest),
                        typeof(VoidPaymentTerminalDeviceRequest),
                        typeof(RefundPaymentTerminalDeviceRequest),
                        typeof(FetchTokenPaymentTerminalDeviceRequest),
                        typeof(EndTransactionPaymentTerminalDeviceRequest),
                        typeof(ClosePaymentTerminalDeviceRequest),
                        typeof(ActivateGiftCardPaymentTerminalRequest),
                        typeof(AddBalanceToGiftCardPaymentTerminalRequest),
                        typeof(GetGiftCardBalancePaymentTerminalRequest),
                        typeof(GetPrivateTenderPaymentTerminalDeviceRequest)
                    };
            }
        }

        /// <summary>
        /// Gets the specify the name of the request handler.
        /// </summary>
        public string HandlerName
        {
            get
            {
                return PaymentTerminalDevice;
            }
        }

        /// <summary>
        /// Executes the payment device simulator operation based on the incoming request type.
        /// </summary>
        /// <param name="request">The payment terminal device simulator request message.</param>
        /// <returns>Returns the payment terminal device simulator response.</returns>
        public Microsoft.Dynamics.Commerce.Runtime.Messages.Response Execute(Microsoft.Dynamics.Commerce.Runtime.Messages.Request request)
        {
            Microsoft.Dynamics.Commerce.Runtime.ThrowIf.Null(request, "request");

            Type requestType = request.GetType();

            if (requestType == typeof(OpenPaymentTerminalDeviceRequest))
            {
                this.Open((OpenPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(BeginTransactionPaymentTerminalDeviceRequest))
            {
                this.BeginTransaction((BeginTransactionPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(UpdateLineItemsPaymentTerminalDeviceRequest))
            {
                this.UpdateLineItems((UpdateLineItemsPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(AuthorizePaymentTerminalDeviceRequest))
            {
                return this.AuthorizePayment((AuthorizePaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(CapturePaymentTerminalDeviceRequest))
            {
                return this.CapturePayment((CapturePaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(VoidPaymentTerminalDeviceRequest))
            {
                return this.VoidPayment((VoidPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(RefundPaymentTerminalDeviceRequest))
            {
                return this.RefundPayment((RefundPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(FetchTokenPaymentTerminalDeviceRequest))
            {
                return this.FetchToken((FetchTokenPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(EndTransactionPaymentTerminalDeviceRequest))
            {
                this.EndTransaction((EndTransactionPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(ClosePaymentTerminalDeviceRequest))
            {
                this.Close((ClosePaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(CancelOperationPaymentTerminalDeviceRequest))
            {
                this.CancelOperation((CancelOperationPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(ActivateGiftCardPaymentTerminalRequest))
            {
                return this.ActivateGiftCard((ActivateGiftCardPaymentTerminalRequest)request);
            }
            else if (requestType == typeof(AddBalanceToGiftCardPaymentTerminalRequest))
            {
                return this.AddBalanceToGiftCard((AddBalanceToGiftCardPaymentTerminalRequest)request);
            }
            else if (requestType == typeof(GetGiftCardBalancePaymentTerminalRequest))
            {
                return this.GetGiftCardBalance((GetGiftCardBalancePaymentTerminalRequest)request);
            }
            else
            {
                WriteLog($"Request : '{request.GetType()}' is not supported.");
                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }

            return new NullResponse();
        }

        /// <summary>
        /// Opens the payment terminal device.
        /// </summary>
        /// <param name="request">The open payment terminal device request.</param>
        public void Open(OpenPaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: Open()", true);
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Utilities.WaitAsyncTask(() => this.OpenAsync(request.DeviceName, request.TerminalSettings, request.DeviceConfig));
        }

        /// <summary>
        ///  Begins the transaction.
        /// </summary>
        /// <param name="request">The begin transaction request.</param>
        public void BeginTransaction(BeginTransactionPaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: BeginTransaction()");
            Utilities.WaitAsyncTask(() => Task.Run(async () =>
            {
                PSDK.PaymentProperty[] merchantProperties = CardPaymentManager.ToLocalProperties(request.MerchantInformation);

                await this.BeginTransactionAsync(merchantProperties, request.PaymentConnectorName, request.InvoiceNumber, request.IsTestMode);
            }));
        }

        /// <summary>
        /// Update the line items.
        /// </summary>
        /// <param name="request">The update line items request.</param>
        public void UpdateLineItems(UpdateLineItemsPaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: UpdateLineItems");
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Utilities.WaitAsyncTask(() => this.UpdateLineItemsAsync(request.TotalAmount, request.TaxAmount, request.DiscountAmount, request.SubTotalAmount, request.Items));
        }

        /// <summary>
        /// Authorize payment.
        /// </summary>
        /// <param name="request">The authorize payment request.</param>
        /// <returns>The authorize payment response.</returns>
        public AuthorizePaymentTerminalDeviceResponse AuthorizePayment(AuthorizePaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: AuthorizePayment");
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            //PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.AuthorizePaymentAsync(request.Amount, request.Currency, request.VoiceAuthorization, request.IsManualEntry, request.ExtensionTransactionProperties));
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.AuthorizePaymentAsync(request));
            return new AuthorizePaymentTerminalDeviceResponse(paymentInfo);
        }

        /// <summary>
        /// Capture payment.
        /// </summary>
        /// <param name="request">The capture payment request.</param>
        /// <returns>The capture payment response.</returns>
        public CapturePaymentTerminalDeviceResponse CapturePayment(CapturePaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: CapturePayment");
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            PSDK.PaymentProperty[] merchantProperties = CardPaymentManager.ToLocalProperties(request.PaymentPropertiesXml);
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.CapturePaymentAsync(request.Amount, request.Currency, merchantProperties, request.ExtensionTransactionProperties));

            return new CapturePaymentTerminalDeviceResponse(paymentInfo);
        }

        /// <summary>
        /// Voids payment.
        /// </summary>
        /// <param name="request">The void payment request.</param>
        /// <returns>The void payment response.</returns>
        public VoidPaymentTerminalDeviceResponse VoidPayment(VoidPaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: VoidPayment");
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            PSDK.PaymentProperty[] merchantProperties = CardPaymentManager.ToLocalProperties(request.PaymentPropertiesXml);
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.VoidPaymentAsync(request.Amount, request.Currency, merchantProperties, request.ExtensionTransactionProperties));

            return new VoidPaymentTerminalDeviceResponse(paymentInfo);
        }

        /// <summary>
        /// Refund payment.
        /// </summary>
        /// <param name="request">The refund payment request.</param>
        /// <returns>The refund payment response.</returns>
        public RefundPaymentTerminalDeviceResponse RefundPayment(RefundPaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: RefundPayment");
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.RefundPaymentAsync(request.Amount, request.Currency, request.IsManualEntry, request.ExtensionTransactionProperties));

            return new RefundPaymentTerminalDeviceResponse(paymentInfo);
        }

        /// <summary>
        /// Fetch token.
        /// </summary>
        /// <param name="request">The fetch token request.</param>
        /// <returns>The fetch token response.</returns>
        public FetchTokenPaymentTerminalDeviceResponse FetchToken(FetchTokenPaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: FetchToken");
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.FetchTokenAsync(request.IsManualEntry, request.ExtensionTransactionProperties));

            return new FetchTokenPaymentTerminalDeviceResponse(paymentInfo);
        }

        /// <summary>
        /// Ends the transaction.
        /// </summary>
        /// <param name="request">The end transaction request.</param>
        public void EndTransaction(EndTransactionPaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: EndTransaction");
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Utilities.WaitAsyncTask(() => this.EndTransactionAsync());
        }

        /// <summary>
        /// Closes the payment terminal device.
        /// </summary>
        /// <param name="request">The close payment terminal request.</param>
        public void Close(ClosePaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: Close");
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Utilities.WaitAsyncTask(() => this.CloseAsync());
        }

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        /// <param name="request">The cancel operation request.</param>
        public void CancelOperation(CancelOperationPaymentTerminalDeviceRequest request)
        {
            WriteLog("Entered method: CancelOperation");
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Utilities.WaitAsyncTask(() => this.CancelOperationAsync());
        }

        /// <summary>
        /// Activate the gift card.
        /// </summary>
        /// <param name="request">The activate gift card request.</param>
        /// <returns>The gift card payment response.</returns>
        public GiftCardPaymentResponse ActivateGiftCard(ActivateGiftCardPaymentTerminalRequest request)
        {
            Microsoft.Dynamics.Commerce.Runtime.ThrowIf.Null(request, "request");

            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.ActivateGiftCardAsync(request.PaymentConnectorName, request.Amount, request.Currency, request.TenderInfo, request.ExtensionTransactionProperties));

            return new GiftCardPaymentResponse(paymentInfo);
        }

        /// <summary>
        /// Add balance to the gift card.
        /// </summary>
        /// <param name="request">The add balance to gift card request.</param>
        /// <returns>The gift card payment response.</returns>
        public GiftCardPaymentResponse AddBalanceToGiftCard(AddBalanceToGiftCardPaymentTerminalRequest request)
        {
            Microsoft.Dynamics.Commerce.Runtime.ThrowIf.Null(request, "request");
            Microsoft.Dynamics.Commerce.Runtime.ThrowIf.Null(request.TenderInfo, "tenderInfo");

            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.AddBalanceToGiftCardAsync(request.PaymentConnectorName, request.Amount, request.Currency, request.TenderInfo, request.ExtensionTransactionProperties));

            return new GiftCardPaymentResponse(paymentInfo);
        }

        /// <summary>
        /// Get gift card balance.
        /// </summary>
        /// <param name="request">The get gift card balance request.</param>
        /// <returns>The gift card payment response.</returns>
        public GiftCardPaymentResponse GetGiftCardBalance(GetGiftCardBalancePaymentTerminalRequest request)
        {
            Microsoft.Dynamics.Commerce.Runtime.ThrowIf.Null(request, "request");
            Microsoft.Dynamics.Commerce.Runtime.ThrowIf.Null(request.TenderInfo, "tenderInfo");

            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.GetGiftCardBalanceAsync(request.PaymentConnectorName, request.Currency, request.TenderInfo, request.ExtensionTransactionProperties));

            return new GiftCardPaymentResponse(paymentInfo);
        }

        /// <summary>
        /// Make authorization payment.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="voiceAuthorization">The voice approval code (optional).</param>
        /// <param name="isManualEntry">If manual credit card entry is required.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the authorization has completed.</returns>
        //public async Task<PaymentInfo> AuthorizePaymentAsync(decimal amount, string currency, string voiceAuthorization, bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
        public async Task<PaymentInfo> AuthorizePaymentAsync(AuthorizePaymentTerminalDeviceRequest paymentRequest)
        {
            PaymentInfo paymentInfo = new PaymentInfo();
            try
            {
                #region LegacyCode
                //dynamic info = new JObject();

                //info.Amount = amount;
                //info.Currency = currency;
                //info.VoiceAuthorization = voiceAuthorization;
                //info.IsManualEntry = isManualEntry;

                //string serializedInfo = info.ToString();

                ///* this.CardSwipeHandler(); */

                //var paymentTerminalPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.AuthorizationData));
                //paymentTerminalPipeline.AuthorizePayment(serializedInfo);

                //PaymentInfo paymentInfo = new PaymentInfo();

                //// Get tender
                //TenderInfo maskedTenderInfo = await this.FetchTenderInfoAsync();
                //if (maskedTenderInfo == null)
                //{
                //    return paymentInfo;
                //}

                //paymentInfo.CardNumberMasked = Utilities.GetMaskedCardNumber(maskedTenderInfo.CardNumber);
                //paymentInfo.CashbackAmount = maskedTenderInfo.CashBackAmount;
                //paymentInfo.CardType = (Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType)maskedTenderInfo.CardTypeId;

                //if (paymentInfo.CashbackAmount > this.terminalSettings.DebitCashbackLimit)
                //{
                //    throw new CardPaymentException(CardPaymentException.CashbackAmountExceedsLimit, "Cashback amount exceeds the maximum amount allowed.");
                //}

                //// Authorize
                //PSDK.Response response = CardPaymentManager.InvokeAuthorizationCall(this.merchantProperties, this.paymentConnectorName, this.tenderInfo, amount, currency, this.terminalSettings.Locale, true, this.terminalSettings.TerminalId, extensionTransactionProperties);

                //Guid cardStorageKey = Guid.NewGuid();
                //CardPaymentManager.MapAuthorizeResponse(response, paymentInfo, cardStorageKey, this.terminalSettings.TerminalId);

                //if (paymentInfo.IsApproved)
                //{
                //    // Backup credit card number
                //    TemporaryCardMemoryStorage<string> cardStorage = new TemporaryCardMemoryStorage<string>(DateTime.UtcNow, this.tenderInfo.CardNumber);
                //    cardStorage.StorageInfo = paymentInfo.PaymentSdkData;
                //    cardCache.Add(cardStorageKey, cardStorage);

                //    // need signature?
                //    if (this.terminalSettings.SignatureCaptureMinimumAmount < paymentInfo.ApprovedAmount)
                //    {
                //        paymentInfo.SignatureData = await this.RequestTenderApprovalAsync(paymentInfo.ApprovedAmount);
                //    }
                //}

                //return paymentInfo;
                #endregion
                WriteLog("Entered method: AuthorizePaymentAsync", true);

                //return _helper.PostXMLData(Helper.RequestType.Authorize, DateTime.UtcNow, "Clerk123", request.IsManualEntry, "TRNX234", request.Currency, request.Amount.ToString());
                string urlwithPortAndPath = "http://127.0.0.1:8900/Capture";//_config.EndPointIp + destinationPath;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlwithPortAndPath);
                byte[] bytes;
                string _xmlString = BuildSampleCardRequestXML();
                bytes = System.Text.Encoding.ASCII.GetBytes(_xmlString);//
                request.ContentType = "text/xml; encoding='utf-8'";
                request.ContentLength = bytes.Length;
                request.Method = "POST";
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                HttpWebResponse response;
                WriteLog($"Placeholder before GetResponse call ");
                response = (HttpWebResponse)request.GetResponse();
                WriteLog($"HTTP Response status code :{response.StatusCode}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    WriteLog($"HTTP Response status code :{response.StatusCode}");
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    paymentInfo = MapCardPaymentResponse(responseStr);
                    WriteLog($"Response :{responseStr}");
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Exception in : AuthorizePaymentAsync .InnerException :{ex.InnerException},Message{ex.Message},StackTrace:{ex.StackTrace},Ex Type:{ex.GetType()}");
                var paymentTerminalPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.Error));
                paymentTerminalPipeline.SendError(ex.Message);
                throw;
            }
            return paymentInfo;
        }

        public PaymentInfo MapCardPaymentResponse(string responseStr)
        {
            PaymentInfo paymentInfo = new PaymentInfo {
                IsApproved=false
            };

           // var xmlRequest = BuildSampleCardRequestXML();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseStr);

           
            XmlNodeList cardServiceResponseNode = xmlDoc.GetElementsByTagName("CardServiceResponse");
            string requestType = cardServiceResponseNode[0].Attributes["RequestType"].Value;
            string workstationID = cardServiceResponseNode[0].Attributes["WorkstationID"].Value;
            string requestID = cardServiceResponseNode[0].Attributes["RequestID"].Value;
            string overallResult = cardServiceResponseNode[0].Attributes["OverallResult"].Value;

            var terminalNode = xmlDoc.GetElementsByTagName("Terminal");
            string terminalID = terminalNode[0].Attributes["TerminalID"].Value;
            string terminalBatch = terminalNode[0].Attributes["TerminalBatch"].Value;

            var tenderNode = xmlDoc.GetElementsByTagName("Tender");

            var totalAmountNode = xmlDoc.GetElementsByTagName("TotalAmount");
            string totalAmount = totalAmountNode[0].InnerText;

            var authorisationNode = xmlDoc.GetElementsByTagName("Authorisation");
            string acquirerID = authorisationNode[0].Attributes["AcquirerID"].Value;
            string startDate = authorisationNode[0].Attributes["StartDate"].Value;
            string expiryDate = authorisationNode[0].Attributes["ExpiryDate"].Value;
            string timeStamp = authorisationNode[0].Attributes["TimeStamp"].Value;

            #region Different Action codes as per EPS doc
            // '004' is undefined by IFSF so should be treated as a decline.   
            //'000' Approved 
            //'001' Honour, with Identification Approved 
            //'002' Approved for partial amount Approved 
            //'003' Approved(VIP) Approved 
            //'005' Approved, account type specified by card issuer 
            //'006' Approved for partial amount, account Approved 
            //'007' Approved, update ICC Approved
            #endregion

            string actionCode = authorisationNode[0].Attributes["ActionCode"].Value;
            string approvalCode = authorisationNode[0].Attributes["ApprovalCode"].Value;
            string acquirerBatch = authorisationNode[0].Attributes["AcquirerBatch"].Value;
            string panprint = authorisationNode[0].Attributes["PANprint"].Value;
            string merchant = authorisationNode[0].Attributes["Merchant"].Value;
            string authorisationType = authorisationNode[0].Attributes["AuthorisationType"].Value;
            string captureReference = authorisationNode[0].Attributes["CaptureReference"].Value;

            if (overallResult.Equals(OverallResult.Success))
                paymentInfo.IsApproved = true;
               
            return paymentInfo;
        }
        enum OverallResult
        {
            Success,
            CommunicationError
        }
        enum EpsRequestType
        {
            CardPayment,
            PaymentRefund,
            AbortRequest
        } 
        /// <summary>
        ///  Begins the transaction.
        /// </summary>
        /// <param name="merchantProperties">The merchant provider payment properties for the peripheral device.</param>
        /// <param name="paymentConnectorName">The payment connector name.</param>
        /// <param name="invoiceNumber">The invoice number associated with the transaction (6 characters long).</param>
        /// <param name="isTestMode">Is test mode for payments enabled for the peripheral device.</param>
        /// <returns>A task that can be awaited until the begin transaction screen is displayed.</returns>
        public async Task BeginTransactionAsync(PSDK.PaymentProperty[] merchantProperties, string paymentConnectorName, string invoiceNumber, bool isTestMode)
        {
            WriteLog("Entered method: BeginTransactionAsync");
            var beginTransactionTask = Task.Factory.StartNew(() =>
            {
                this.merchantProperties = merchantProperties;
                this.paymentConnectorName = paymentConnectorName;

                dynamic info = new JObject();

                info.PaymentConnectorName = paymentConnectorName;
                info.InvoiceNumber = invoiceNumber;
                info.IsTestMode = isTestMode;

                FillPaymentProperties(merchantProperties, info);

                string serializedInfo = info.ToString();

                var paymentTerminalPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.PaymentProperties));
                paymentTerminalPipeline.BeginTransaction(serializedInfo);
            });

            await beginTransactionTask;
        }

        /// <summary>
        ///  Cancels an existing GetTender or RequestTenderApproval  operation on the payment terminal.
        /// </summary>
        /// <returns>A task that can be awaited until the operation is cancelled.</returns>
        public async Task CancelOperationAsync()
        {
            await Task.Delay(TaskDelayInMilliSeconds);
        }

        /// <summary>
        /// Make settlement of a payment.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="paymentProperties">The payment properties of the authorization response.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the settlement has completed.</returns>
        public Task<PaymentInfo> CapturePaymentAsync(decimal amount, string currency, PSDK.PaymentProperty[] paymentProperties, ExtensionTransaction extensionTransactionProperties)
        {
            WriteLog("Entered method: CapturePaymentAsync");
            try
            {
                dynamic info = new JObject();

                info.Amount = amount;
                info.Currency = currency;
                FillPaymentProperties(paymentProperties, info);

                string serializedInfo = info.ToString();

                var paymentTerminalPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.PaymentDetails));
                paymentTerminalPipeline.CapturePayment(serializedInfo);

                if (amount < this.terminalSettings.MinimumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountLessThanMinimumLimit, "Amount does not meet minimum amount allowed.");
                }

                if (this.terminalSettings.MaximumAmountAllowed > 0 && amount > this.terminalSettings.MaximumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountExceedsMaximumLimit, "Amount exceeds the maximum amount allowed.");
                }

                if (this.processor == null)
                {
                    this.processor = CardPaymentManager.GetPaymentProcessor(this.merchantProperties, this.paymentConnectorName);
                }

                PaymentInfo paymentInfo = new PaymentInfo();

                // Handle multiple chain connectors by returning single instance used in capture.
                PSDK.IPaymentProcessor currentProcessor = null;
                PSDK.PaymentProperty[] currentMerchantProperties = null;
                CardPaymentManager.GetRequiredConnector(this.merchantProperties, paymentProperties, this.processor, out currentProcessor, out currentMerchantProperties);

                PSDK.Request request = CardPaymentManager.GetCaptureRequest(currentMerchantProperties, paymentProperties, amount, currency, this.terminalSettings.Locale, true, this.terminalSettings.TerminalId, cardCache, extensionTransactionProperties);
                PSDK.Response response = currentProcessor.Capture(request);
                CardPaymentManager.MapCaptureResponse(response, paymentInfo);

                return Task.FromResult(paymentInfo);
            }
            catch (Exception ex)
            {
                WriteLog($"Exception in CapturePaymentAsync with message :{ex.Message}, inner exception :{ex.InnerException}, stacktrace :{ex.StackTrace}");
                var paymentTerminalPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.Error));
                paymentTerminalPipeline.SendError(ex.Message);
                throw;
            }
        }

        /// <summary>
        ///  Closes a connection to the payment terminal.
        /// </summary>
        /// <returns>A task that can be awaited until the connection is closed.</returns>
        public async Task CloseAsync()
        {
            await Task.Delay(TaskDelayInMilliSeconds);
        }

        /// <summary>
        ///  Ends the transaction.
        /// </summary>
        /// <returns>A task that can be awaited until the end transaction screen is displayed.</returns>
        public async Task EndTransactionAsync()
        {
            await Task.Delay(TaskDelayInMilliSeconds);
        }

        /// <summary>
        /// Fetch token for credit card.
        /// </summary>
        /// <param name="isManualEntry">The value indicating whether credit card should be entered manually.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the token generation has completed.</returns>
        public async Task<PaymentInfo> FetchTokenAsync(bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
        {
            PaymentInfo paymentInfo = new PaymentInfo();

            // Get tender
            TenderInfo maskedTenderInfo = await this.FetchTenderInfoAsync();

            PSDK.PaymentProperty[] defaultMerchantProperties = this.merchantProperties;

            paymentInfo.CardNumberMasked = maskedTenderInfo.CardNumber;
            paymentInfo.CashbackAmount = maskedTenderInfo.CashBackAmount;
            paymentInfo.CardType = (Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType)maskedTenderInfo.CardTypeId;

            if (this.merchantProperties != null &&
                this.merchantProperties.Length > 0 &&
                this.merchantProperties[0].Namespace.Equals(GenericNamespace.Connector) &&
                this.merchantProperties[0].Name.Equals(ConnectorProperties.Properties))
            {
                defaultMerchantProperties = this.merchantProperties[0].PropertyList;
            }

            if (this.processor == null)
            {
                this.processor = CardPaymentManager.GetPaymentProcessor(this.merchantProperties, this.paymentConnectorName);
            }

            // Generate card token
            PSDK.Request request = CardPaymentManager.GetTokenRequest(defaultMerchantProperties, this.tenderInfo, this.terminalSettings.Locale, extensionTransactionProperties);
            PSDK.Response response = this.processor.GenerateCardToken(request, null);
            CardPaymentManager.MapTokenResponse(response, paymentInfo);

            return paymentInfo;
        }

        /// <summary>
        /// Open payment device using simulator.
        /// </summary>
        /// <param name="peripheralName">Name of peripheral device.</param>
        /// <param name="terminalSettings">The terminal settings for the peripheral device.</param>
        /// <param name="deviceConfig">Device Configuration parameters.</param>
        /// <returns>A task that can be awaited until the connection is opened.</returns>
        [Obsolete("This method will be removed once IPaymentDevice is deprecated.")]
        public async Task OpenAsync(string peripheralName, SettingsInfo terminalSettings, IDictionary<string, string> deviceConfig)
        {
            await Task.Delay(TaskDelayInMilliSeconds);
        }

        /// <summary>
        /// Open payment device using simulator.
        /// </summary>
        /// <param name="peripheralName">Name of peripheral device.</param>
        /// <param name="terminalSettings">The terminal settings for the peripheral device.</param>
        /// <param name="deviceConfig">Device Configuration parameters.</param>
        /// <returns>A task that can be awaited until the connection is opened.</returns>
        public async Task OpenAsync(string peripheralName, SettingsInfo terminalSettings, Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.PeripheralConfiguration deviceConfig)
        {
            var openTask = Task.Factory.StartNew(() =>
            {
                this.terminalSettings = terminalSettings;

                dynamic info = new JObject();

                info.PeripheralName = peripheralName;
                info.TerminalSettings = new JObject() as dynamic;

                if (terminalSettings != null)
                {
                    info.TerminalSettings.SignatureCaptureMinimumAmount = terminalSettings.SignatureCaptureMinimumAmount;
                    info.TerminalSettings.MinimumAmountAllowed = terminalSettings.MinimumAmountAllowed;
                    info.TerminalSettings.MaximumAmountAllowed = terminalSettings.MaximumAmountAllowed;
                    info.TerminalSettings.DebitCashbackLimit = terminalSettings.DebitCashbackLimit;
                    info.TerminalSettings.Locale = terminalSettings.Locale;
                    info.TerminalSettings.TerminalId = terminalSettings.TerminalId;
                }

                string serializedInfo = info.ToString();

                var paymentTerminalPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.PaymentOpenTerminal));
                paymentTerminalPipeline.OpenTransaction(serializedInfo);
            });

            await openTask;
        }

        /// <summary>
        /// Make refund payment.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="isManualEntry">If manual credit card entry is required.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the refund has completed.</returns>
        public async Task<PaymentInfo> RefundPaymentAsync(decimal amount, string currency, bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
        {
            try
            {
                dynamic info = new JObject();

                info.Amount = amount;
                info.Currency = currency;
                info.IsManualEntry = isManualEntry;

                string serializedInfo = info.ToString();
                File.WriteAllText(string.Format(@"{0}\{1}_RefundPayment.json", this.deviceSimFolderPath, PaymentDeviceSimulatorFileName), serializedInfo);

                if (amount < this.terminalSettings.MinimumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountLessThanMinimumLimit, "Amount does not meet minimum amount allowed.");
                }

                if (this.terminalSettings.MaximumAmountAllowed > 0 && amount > this.terminalSettings.MaximumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountExceedsMaximumLimit, "Amount exceeds the maximum amount allowed.");
                }

                if (this.processor == null)
                {
                    this.processor = CardPaymentManager.GetPaymentProcessor(this.merchantProperties, this.paymentConnectorName);
                }

                PaymentInfo paymentInfo = new PaymentInfo();

                // Get tender
                TenderInfo maskedTenderInfo = await this.FetchTenderInfoAsync();
                if (maskedTenderInfo == null)
                {
                    return paymentInfo;
                }

                paymentInfo.CardNumberMasked = maskedTenderInfo.CardNumber;
                paymentInfo.CashbackAmount = maskedTenderInfo.CashBackAmount;
                paymentInfo.CardType = (Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType)maskedTenderInfo.CardTypeId;

                if (paymentInfo.CashbackAmount > this.terminalSettings.DebitCashbackLimit)
                {
                    throw new CardPaymentException(CardPaymentException.CashbackAmountExceedsLimit, "Cashback amount exceeds the maximum amount allowed.");
                }

                // Refund
                PSDK.Response response = CardPaymentManager.ChainedRefundCall(this.merchantProperties, string.Empty, this.tenderInfo, amount, currency, this.terminalSettings.Locale, true, this.terminalSettings.TerminalId, extensionTransactionProperties);

                CardPaymentManager.MapRefundResponse(response, paymentInfo);

                if (paymentInfo.IsApproved)
                {
                    // need signature?
                    if (this.terminalSettings.SignatureCaptureMinimumAmount < paymentInfo.ApprovedAmount)
                    {
                        paymentInfo.SignatureData = await this.RequestTenderApprovalAsync(paymentInfo.ApprovedAmount);
                    }
                }

                return paymentInfo;
            }
            catch (Exception ex)
            {
                var paymentTerminalPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.Error));
                paymentTerminalPipeline.SendError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Update the line items on the current open session.  This method will compare against previous lines specified and make the appropriate device calls.
        /// </summary>
        /// <param name="totalAmount">The total amount of the transaction, including tax.</param>
        /// <param name="taxAmount">The total tax amount on the transaction.</param>
        /// <param name="discountAmount">The total discount amount on the transaction.</param>
        /// <param name="subTotalAmount">The sub-total amount on the transaction.</param>
        /// <param name="items">The items in the transaction.</param>
        /// <returns>A task that can be awaited until the text is displayed on the screen.</returns>
        public async Task UpdateLineItemsAsync(string totalAmount, string taxAmount, string discountAmount, string subTotalAmount, IEnumerable<ItemInfo> items)
        {
            var updateLineItemsTask = Task.Factory.StartNew(() =>
            {
                dynamic info = new JObject();

                info.TotalAmount = totalAmount;
                info.TaxAmount = taxAmount;
                info.DiscountAmount = discountAmount;
                info.SubTotalAmount = subTotalAmount;

                FillItemInfo(items.ToArray(), info);

                string serializedInfo = info.ToString();

                var paymentCardPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.LineItems));
                paymentCardPipeline.UpdateCartLines(serializedInfo);
            });

            await updateLineItemsTask;
        }

        /// <summary>
        /// Make reversal/void a payment.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="paymentProperties">The payment properties of the authorization response.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the void has completed.</returns>
        public Task<PaymentInfo> VoidPaymentAsync(decimal amount, string currency, PSDK.PaymentProperty[] paymentProperties, ExtensionTransaction extensionTransactionProperties)
        {
            try
            {
                dynamic info = new JObject();

                info.Amount = amount;
                info.Currency = currency;
                FillPaymentProperties(paymentProperties, info);

                string serializedInfo = info.ToString();
                File.WriteAllText(string.Format(@"{0}\{1}_VoidPayment.json", this.deviceSimFolderPath, PaymentDeviceSimulatorFileName), serializedInfo);

                if (amount < this.terminalSettings.MinimumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountLessThanMinimumLimit, "Amount does not meet minimum amount allowed.");
                }

                if (this.terminalSettings.MaximumAmountAllowed > 0 && amount > this.terminalSettings.MaximumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountExceedsMaximumLimit, "Amount exceeds the maximum amount allowed.");
                }

                if (this.processor == null)
                {
                    this.processor = CardPaymentManager.GetPaymentProcessor(this.merchantProperties, this.paymentConnectorName);
                }

                PaymentInfo paymentInfo = new PaymentInfo();

                // Handle multiple chain connectors by returning single instance used in capture.
                PSDK.IPaymentProcessor currentProcessor = null;
                PSDK.PaymentProperty[] currentMerchantProperties = null;
                CardPaymentManager.GetRequiredConnector(this.merchantProperties, paymentProperties, this.processor, out currentProcessor, out currentMerchantProperties);

                PSDK.Request request = CardPaymentManager.GetCaptureRequest(currentMerchantProperties, paymentProperties, amount, currency, this.terminalSettings.Locale, true, this.terminalSettings.TerminalId, cardCache, extensionTransactionProperties);
                PSDK.Response response = currentProcessor.Void(request);
                CardPaymentManager.MapVoidResponse(response, paymentInfo);

                return Task.FromResult(paymentInfo);
            }
            catch (Exception ex)
            {
                var paymentTerminalPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.Error));
                paymentTerminalPipeline.SendError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// We don't have the payment provider authorization piece for this function, here we just
        /// assume we get the bank authorization.
        /// </summary>
        /// <param name="amount">Required payment amount.</param>
        /// <returns>TenderInfo object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "amount", Justification = "Other devices support the amount for signature approval.")]
        public async Task<string> RequestTenderApprovalAsync(decimal amount)
        {
            /*this.SignatureCaptureHandler();*/

            string signature = await this.GetSignatureData();

            // Show processing info here...
            return signature;
        }

        private static void FillItemInfo(ItemInfo[] items, dynamic info)
        {
            info.Items = new JArray() as dynamic;
            foreach (ItemInfo item in items)
            {
                dynamic itemInfo = new JObject();

                itemInfo.LineItemId = item.LineItemId;
                itemInfo.Sku = item.Sku;
                itemInfo.Upc = item.Upc;
                itemInfo.Description = item.Description;
                itemInfo.Quantity = item.Quantity;
                itemInfo.UnitPrice = item.UnitPrice;
                itemInfo.ExtendedPriceWithTax = item.ExtendedPriceWithTax;
                itemInfo.IsVoided = item.IsVoided;
                itemInfo.Discount = item.Discount;

                info.Items.Add(itemInfo);
            }
        }

        private static void FillPaymentProperties(PSDK.PaymentProperty[] merchantProperties, dynamic info)
        {
            if (merchantProperties == null || merchantProperties.Length == 0)
            {
                throw new CardPaymentException(CardPaymentException.EmptyPaymentProperties, "The merchant payment properties are empty.");
            }

            info.MerchantProperties = new JArray() as dynamic;
            foreach (PSDK.PaymentProperty merchant in merchantProperties)
            {
                dynamic merchantInfo = new JObject();

                merchantInfo.Namespace = merchant.Namespace;
                merchantInfo.Name = merchant.Name;
                merchantInfo.ValueType = merchant.ValueType;
                merchantInfo.StringValue = merchant.StringValue;
                merchantInfo.StoredStringValue = merchant.StoredStringValue;
                merchantInfo.DecimalValue = merchant.DecimalValue;
                merchantInfo.DateValue = merchant.DateValue;
                merchantInfo.DisplayName = merchant.DisplayName;
                merchantInfo.Description = merchant.Description;
                merchantInfo.SecurityLevel = merchant.SecurityLevel;
                merchantInfo.IsEncrypted = merchant.IsEncrypted;
                merchantInfo.IsPassword = merchant.IsPassword;
                merchantInfo.IsReadOnly = merchant.IsReadOnly;
                merchantInfo.IsHidden = merchant.IsHidden;
                merchantInfo.DisplayHeight = merchant.DisplayHeight;
                merchantInfo.SequenceNumber = merchant.SequenceNumber;

                info.MerchantProperties.Add(merchantInfo);
            }
        }

        private async Task<TenderInfo> FetchTenderInfoAsync()
        {
            TenderInfo tenderInfo = await this.FillTenderInfo();

            // Show processing info here...
            return tenderInfo;
        }

        private void SignatureCaptureHandler()
        {
            var paymentTerminalSignatureCapStatePipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.SignatureCaptureState));

            // Register the event handler for enable signature capture state device activities.
            EventHandler<VirtualPeripheralsEventArgs> signatureCaptureInfoEventHandler = null;
            signatureCaptureInfoEventHandler = (sender, args) =>
            {
                // Perform the device activity of entering the signature, once the card information is captured.
                var signatureCapturePipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.SignatureCaptureData));
                string signatureData = this.GetSignatureDataByBytes();

                signatureCapturePipeline.CaptureSignatureCaptureData(signatureData);

                paymentTerminalSignatureCapStatePipeline.PaymentTerminalMessageHandler.OnPaymentTerminalSignatureCaptureStateMessage -= signatureCaptureInfoEventHandler;
            };

            paymentTerminalSignatureCapStatePipeline.PaymentTerminalMessageHandler.OnPaymentTerminalSignatureCaptureStateMessage += signatureCaptureInfoEventHandler;
        }

        private void CardSwipeHandler()
        {
            var paymentTerminalCardSwipeState = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.CardState));

            // Register the event handler for enable card swipe state device activities.
            EventHandler<VirtualPeripheralsEventArgs> cardStateInfoEventHandler = null;
            cardStateInfoEventHandler = (sender, args) =>
            {
                // Perform the device activity of swiping the card, once the update lines is complete.
                var paymentCardPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.CardData));

                PaymentCardData paymentCardData = this.GetCardInfo();
                string xmlPaymentCardData = XmlDataHelper.ConvertTypeToXml(paymentCardData);

                paymentCardPipeline.CaptureCardInfo(xmlPaymentCardData);

                paymentTerminalCardSwipeState.PaymentTerminalMessageHandler.OnPaymentTerminalCardSwipeStateMessage -= cardStateInfoEventHandler;
            };

            paymentTerminalCardSwipeState.PaymentTerminalMessageHandler.OnPaymentTerminalCardSwipeStateMessage += cardStateInfoEventHandler;
        }

        private string GetSignatureDataByBytes()
        {
            // The signature capture data will be moved to test xml file.
            return string.Empty;
        }

        private PaymentCardData GetCardInfo()
        {
            // The card information is hard coded only for test cases. 
            // The card details will be moved to test xml file.
            return null;
        }

        private async Task<TenderInfo> FillTenderInfo()
        {
            var tenderInfoTask = Task<TenderInfo>.Factory.StartNew(() =>
            {
                var paymentTerminalPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.CardData));

                var getCardTrackData = new TaskCompletionSource<PaymentCardData>();

                EventHandler<VirtualPeripheralsEventArgs> cardInfoEventHandler = null;
                cardInfoEventHandler = (sender, args) =>
                {
                    try
                    {
                        PaymentCardData cardInfo = XmlDataHelper.ConvertXmlToType<PaymentCardData>(args.Contents.FirstOrDefault());
                        getCardTrackData.SetResult(cardInfo);
                    }
                    catch
                    {
                        // Ignoring the exception - An attempt was made to transition a task to a final state when it had already completed.
                        // The exception occurs inspite of unregistering the call back event in the finally. 
                    }
                    finally
                    {
                        paymentTerminalPipeline.PaymentTerminalMessageHandler.OnPaymentTerminalCardSwipeMessage -= cardInfoEventHandler;
                    }
                };

                paymentTerminalPipeline.PaymentTerminalMessageHandler.OnPaymentTerminalCardSwipeMessage += cardInfoEventHandler;

                // Enable the card swipe in the device.
                var paymentTerminalCardSwipePipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.CardState));
                paymentTerminalCardSwipePipeline.EnableCardSwipe();

                PaymentCardData cardSwipeData = getCardTrackData.Task.Result;

                var tenderInfo = new TenderInfo();

                tenderInfo.CardTypeId = (int)Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType.InternationalCreditCard;
                tenderInfo.Track1 = cardSwipeData.Track1Data;
                tenderInfo.Track2 = cardSwipeData.Track2Data;
                tenderInfo.CardNumber = cardSwipeData.AccountNumber;
                tenderInfo.ExpirationMonth = cardSwipeData.ExpirationMonth;
                tenderInfo.ExpirationYear = cardSwipeData.ExpirationYear;

                this.tenderInfo = tenderInfo;

                return tenderInfo;
            });

            return await tenderInfoTask;
        }

        /// <summary>
        /// Get the signature data from the device.
        /// </summary>
        /// <returns>Returns the signature data task.</returns>
        private async Task<string> GetSignatureData()
        {
            var signatureInfoTask = Task<string>.Factory.StartNew(() =>
            {
                var paymentTerminalPipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.SignatureCaptureData));

                var signatureData = new TaskCompletionSource<string>();

                EventHandler<VirtualPeripheralsEventArgs> signatureCaptureEventHandler = null;
                signatureCaptureEventHandler = (sender, args) =>
                {
                    try
                    {
                        signatureData.SetResult(SignatureCaptureHelper.ParsePointArray(args.Contents.FirstOrDefault()).Signature);
                    }
                    catch
                    {
                        // Ignoring the exception - An attempt was made to transition a task to a final state when it had already completed.
                        // The exception occurs inspite of unregistering the call back event in the finally. 
                    }
                    finally
                    {
                        paymentTerminalPipeline.PaymentTerminalMessageHandler.OnPaymentTerminalSignatureCaptureDataMessage -= signatureCaptureEventHandler;
                    }
                };

                paymentTerminalPipeline.PaymentTerminalMessageHandler.OnPaymentTerminalSignatureCaptureDataMessage += signatureCaptureEventHandler;

                // Enable the signature device form.
                var paymentTerminalSignatureCaptureStatePipeline = new PaymentTerminalPipeline(string.Format("{0}{1}", PaymentTerminalDevice, PaymentTerminalMessageHandler.SignatureCaptureState));
                paymentTerminalSignatureCaptureStatePipeline.EnableSignatureForm();

                string signature = signatureData.Task.Result;

                return signature;
            });

            return await signatureInfoTask;
        }

        /// <summary>
        /// Activate gift card.
        /// </summary>
        /// <param name="paymentConnectorName">The payment connector name.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="currencyCode">The currency.</param>
        /// <param name="tenderInfo">The tender information.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the void has completed.</returns>
        private async Task<PaymentInfo> ActivateGiftCardAsync(string paymentConnectorName, decimal? amount, string currencyCode, TenderInfo tenderInfo, ExtensionTransaction extensionTransactionProperties)
        {
            await Task.Delay(TaskDelayInMilliSeconds);
            throw new PeripheralException(PeripheralException.PaymentTerminalError, "Operation is not supported by payment terminal.", inner: null);
        }

        /// <summary>
        /// Add balance to gift card.
        /// </summary>
        /// <param name="paymentConnectorName">The payment connector name.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="currencyCode">The currency.</param>
        /// <param name="tenderInfo">The tender information.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the void has completed.</returns>
        private async Task<PaymentInfo> AddBalanceToGiftCardAsync(string paymentConnectorName, decimal? amount, string currencyCode, TenderInfo tenderInfo, ExtensionTransaction extensionTransactionProperties)
        {
            await Task.Delay(TaskDelayInMilliSeconds);
            throw new PeripheralException(PeripheralException.PaymentTerminalError, "Operation is not supported by payment terminal.", inner: null);
        }

        /// <summary>
        /// Get gift card balance.
        /// </summary>
        /// <param name="paymentConnectorName">The payment connector name.</param>
        /// <param name="currencyCode">The currency.</param>
        /// <param name="tenderInfo">The tender information.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the void has completed.</returns>
        private async Task<PaymentInfo> GetGiftCardBalanceAsync(string paymentConnectorName, string currencyCode, TenderInfo tenderInfo, ExtensionTransaction extensionTransactionProperties)
        {
            await Task.Delay(TaskDelayInMilliSeconds);
            throw new PeripheralException(PeripheralException.PaymentTerminalError, "Operation is not supported by payment terminal.", inner: null);
        }

        /// <summary>
        /// Task that handles closing the connection after a timeout period.
        /// </summary>
        /// <param name="timeout">The timeout period in seconds.</param>
        private async void Timeout(int timeout)
        {
            this.timeoutTask = new CancellationTokenSource();

            try
            {
                await Task.Delay(timeout * 1000, this.timeoutTask.Token);
            }
            catch (TaskCanceledException)
            {
                RetailLogger.Log.GenericInformationEvent("Task is canceled.");
            }

            await this.EndTransactionAsync();
        }

        #region PrivateMethods
        private void WriteLog(string text, bool append = true)
        {
            //TODO:Make this configurable based on setting
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilename, append, System.Text.Encoding.UTF8))
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        writer.WriteLine(DateTime.Now.ToString(datetimeFormat) + " " + text);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string BuildSampleCardRequestXML()
        {
            // string sampleXmlAsString1 = "<?xml version=\"1.0\"?> <CardServiceRequest xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\"xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"CardPayment\"  WorkstationID=\"Dave's Workstation\" RequestID=\"19\"> <POSdata> <POSTimeStamp>2020-02-05T15:02:59.6303603+01:00</POSTimeStamp> <ManualPAN>false</ManualPAN> </POSdata> < TotalAmount Currency = \"GBP\" > 9 </ TotalAmount > </ CardServiceRequest >";

            //{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}
            string sampleXmlAsString = $"<?xml version =\"1.0\"?> <CardServiceRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"CardPayment\" WorkstationID=\"TestPos\" RequestID=\"2\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\">  <POSdata> <POSTimeStamp>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</POSTimeStamp> <ClerkID> 123456 </ClerkID> <ManualPAN> false </ManualPAN> <TransactionNumber> TXNDEMONo5 </TransactionNumber>   </POSdata>   <TotalAmount Currency=\"GBP\"> 7 </TotalAmount> </CardServiceRequest>";

            return sampleXmlAsString;
        }
        #endregion

    }
}