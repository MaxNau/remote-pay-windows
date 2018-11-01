﻿// Copyright (C) 2018 Clover Network, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//
// You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using com.clover.remote.order;
using com.clover.remotepay.sdk;
using com.clover.remotepay.sdk.service.client;
using com.clover.remotepay.transport;

// Contains a set of classes to simplify using the Windows WebSocket service by providing
// container classes that can be serialized to invoke methods on the service.
namespace com.clover.sdk.remote.websocket
{
    public class ServicePayloadConstants
    {
        public const string PROP_METHOD = "method";
        public const string PROP_PAYLOAD = "payload";
        public const string PROP_ID = "id";
        public const string PROP_TYPE = "type";
        public const string PROP_packageName = "packageName";
    }

    public enum WebSocketMethod
    {
        Status,
        Sale,
        Auth,
        PreAuth,
        Cancel,
        Break, // deprectaed, should be ResetDevice
        ResetDevice,
        CapturePreAuth,
        TipAdjustAuth,
        VoidPayment,
        RefundPayment,
        ManualRefund,
        Closeout,
        DisplayPaymentReceiptOptions,
        PrintText,
        PrintImage,
        PrintImageFromURL,
        OpenCashDrawer,
        ShowMessage,
        ShowWelcomeScreen,
        ShowThankYouScreen,
        ShowDisplayOrder,
        LineItemAddedToDisplayOrder,
        LineItemRemovedFromDisplayOrder,
        DiscountAddedToDisplayOrder,
        DiscountRemovedFromDisplayOrder,
        InvokeInputOption,
        AcceptSignature,
        RejectSignature,
        DeviceActivityStart,
        DeviceActivityEnd,
        DeviceDisconnected,
        DeviceConnected,
        DeviceReady,
        DeviceError,
        Error,
        SaleResponse,
        AuthResponse,
        PreAuthResponse,
        CapturePreAuthResponse,
        CloseoutResponse,
        TipAdjustAuthResponse,
        RefundPaymentResponse,
        ManualRefundResponse,
        VoidPaymentResponse,
        TipAdded,
        VerifySignatureRequest,
        VaultCard,
        VaultCardResponse,
        ReadCardData,
        ReadCardDataResponse,
        LogMessage,
        ConfirmPaymentRequest,
        ConfirmPayment,
        RejectPayment,
        RetrievePendingPayments,
        RetrievePendingPaymentsResponse,
        StartCustomActivity,
        CustomActivityResponse,
        SendMessageToActivity,
        MessageFromActivity,
        RetrieveDeviceStatus,
        RetrieveDeviceStatusResponse,
        ResetDeviceResponse,
        PrintPaymentReceipt,
        PrintPaymentDeclinedReceipt,
        PrintManualRefundReceipt,
        PrintManualRefundDeclinedReceipt,
        PrintPaymentRefundReceipt,
        PrintPaymentMerchantCopyReceipt,
        RetrievePaymentRequest,
        RetrievePaymentResponse,
        PrintJobStatusRequest,
        PrintJobStatusResponse,
        RetrievePrintersRequest,
        RetrievePrintersResponse,
        OpenCashDrawerRequest,
        PrintRequest,
        DisplayReceiptOptionsRequest,
        DisplayReceiptOptionsResponse,
        RegisterForCustomerProvidedDataRequest,
        RegisterForCustomerProvidedDataResponse,
        SetCustomerInfoRequest,
        SetCustomerInfoResponse,
        CustomerProvidedDataResponse,
        VoidPaymentRefund,
        VoidPaymentRefundResponse
    }

    public class WebSocketMessage<T>
    {
        public WebSocketMessage() { }
        public WebSocketMessage(WebSocketMethod _method)
        {
            method = _method;
        }
        public string id { get; set; }
        public WebSocketMethod method { get; set; }
        public T payload { get; set; }
    }

    public class StatusRequestMessage : WebSocketMessage<object>
    {
        public StatusRequestMessage() : base(WebSocketMethod.Status)
        {
        }
    }

    public class SaleRequestMessage : WebSocketMessage<SaleRequest>
    {
        public SaleRequestMessage() : base(WebSocketMethod.Sale)
        {

        }
    }

    public class AuthRequestMessage : WebSocketMessage<AuthRequest>
    {
        public AuthRequestMessage() : base(WebSocketMethod.Auth)
        {
        }
    }

    public class PreAuthRequestMessage : WebSocketMessage<PreAuthRequest>
    {
        public PreAuthRequestMessage() : base(WebSocketMethod.PreAuth)
        {
        }
    }

    public class CancelRequestMessage : WebSocketMessage<object>
    {
        public CancelRequestMessage() : base(WebSocketMethod.Cancel)
        {
        }
    }

    public class BreakRequestMessage : WebSocketMessage<object>
    {
        public BreakRequestMessage() : base(WebSocketMethod.Break)
        {
        }
    }

    public class ResetDeviceMessage : WebSocketMessage<object>
    {
        public ResetDeviceMessage() : base(WebSocketMethod.ResetDevice)
        {
        }
    }

    public class CapturePreAuthRequestMessage : WebSocketMessage<CapturePreAuthRequest>
    {
        public CapturePreAuthRequestMessage() : base(WebSocketMethod.CapturePreAuth)
        {
        }
    }

    public class TipAdjustAuthRequestMessage : WebSocketMessage<TipAdjustAuthRequest>
    {
        public TipAdjustAuthRequestMessage() : base(WebSocketMethod.TipAdjustAuth)
        {
        }
    }

    public class VoidPaymentRequestMessage : WebSocketMessage<VoidPaymentRequest>
    {
        public VoidPaymentRequestMessage() : base(WebSocketMethod.VoidPayment)
        {
        }
    }

    public class RefundPaymentRequestMessage : WebSocketMessage<RefundPaymentRequest>
    {
        public RefundPaymentRequestMessage() : base(WebSocketMethod.RefundPayment)
        {
        }
    }

    public class VoidPaymentRefundRequestMessage : WebSocketMessage<VoidPaymentRefundRequest>
    {
        public VoidPaymentRefundRequestMessage() : base(WebSocketMethod.VoidPaymentRefund)
        {
        }
    }

    public class ManualRefundRequestMessage : WebSocketMessage<ManualRefundRequest>
    {
        public ManualRefundRequestMessage() : base(WebSocketMethod.ManualRefund)
        {
        }
    }

    public class CloseoutRequestMessage : WebSocketMessage<CloseoutRequest>
    {
        public CloseoutRequestMessage() : base(WebSocketMethod.Closeout)
        {
        }
    }

    public class DisplayPaymentReceiptOptionsRequestMessage : WebSocketMessage<object>
    {
        public DisplayPaymentReceiptOptionsRequestMessage() : base(WebSocketMethod.DisplayPaymentReceiptOptions)
        {
        }
    }

    public class PrintTextRequestMessage : WebSocketMessage<PrintText>
    {
        public PrintTextRequestMessage() : base(WebSocketMethod.PrintText)
        {
        }
    }

    public class PrintRequestMessage : WebSocketMessage<PrintRequest64Message>
    {

        public PrintRequestMessage() : base(WebSocketMethod.PrintRequest)
        {
        }
    }

    public class PrintRequest64Message
    {
        public List<string> base64strings = new List<string>();
        public List<string> imageUrls = new List<string>();
        public List<string> textLines = new List<string>();
        public string externalPrintJobId { get; set; }
        public string printDeviceId { get; set; }

        public void setBase64Strings(string base64string)
        {
            this.base64strings.Add(base64string);
        }
        public void setImageUrl(string imageUrl)
        {
            this.imageUrls.Add(imageUrl);
        }
        public void setTextLines(List<string> textLines)
        {
            if (textLines.Count < 1)
            {
                return;
            }
            this.textLines = textLines;
        }
    }

    public class PrintImageRequestMessage : WebSocketMessage<PrintImage>
    {
        public PrintImageRequestMessage() : base(WebSocketMethod.PrintImage)
        {
        }
    }

    public class PrintImageFromURLRequestMessage : WebSocketMessage<PrintImage>
    {
        public PrintImageFromURLRequestMessage() : base(WebSocketMethod.PrintImageFromURL)
        {
        }
    }

    public class OpenCashDrawerRequestMessage : WebSocketMessage<OpenCashDrawerRequest>
    {
        public OpenCashDrawerRequestMessage() : base(WebSocketMethod.OpenCashDrawer) { }
    }

    public class ShowMessageRequestMessage : WebSocketMessage<ShowMessage>
    {
        public ShowMessageRequestMessage() : base(WebSocketMethod.ShowMessage)
        {
        }
    }

    public class ShowWelcomeScreenRequestMessage : WebSocketMessage<object>
    {
        public ShowWelcomeScreenRequestMessage() : base(WebSocketMethod.ShowWelcomeScreen)
        {
        }
    }

    public class ShowThankYouScreenRequestMessage : WebSocketMessage<object>
    {
        public ShowThankYouScreenRequestMessage() : base(WebSocketMethod.ShowThankYouScreen)
        {
        }
    }

    public class DisplayOrderRequestMessage : WebSocketMessage<DisplayOrder>
    {
        public DisplayOrderRequestMessage() : base(WebSocketMethod.ShowDisplayOrder)
        {
        }
    }

    public class LineItemAddedToDisplayOrderRequestMessage : WebSocketMessage<LineItemAddedToDisplayOrder>
    {
        public LineItemAddedToDisplayOrderRequestMessage() : base(WebSocketMethod.LineItemAddedToDisplayOrder)
        {
        }
    }

    public class LineItemRemovedFromDisplayOrderRequestMessage : WebSocketMessage<LineItemRemovedFromDisplayOrder>
    {
        public LineItemRemovedFromDisplayOrderRequestMessage() : base(WebSocketMethod.LineItemRemovedFromDisplayOrder)
        {
        }
    }

    public class DiscountAddedToDisplayOrderRequestMessage : WebSocketMessage<DiscountAddedToDisplayOrder>
    {
        public DiscountAddedToDisplayOrderRequestMessage() : base(WebSocketMethod.DiscountAddedToDisplayOrder)
        {
        }
    }

    public class DiscountRemovedFromDisplayOrderRequestMessage : WebSocketMessage<DiscountRemovedFromDisplayOrder>
    {
        public DiscountRemovedFromDisplayOrderRequestMessage() : base(WebSocketMethod.DiscountRemovedFromDisplayOrder)
        {
        }
    }

    public class InvokeInputOptionRequestMessage : WebSocketMessage<InputOption>
    {
        public InvokeInputOptionRequestMessage() : base(WebSocketMethod.InvokeInputOption)
        {
        }
    }

    public class AcceptSignatureRequestMessage : WebSocketMessage<VerifySignatureRequest>
    {
        public AcceptSignatureRequestMessage() : base(WebSocketMethod.AcceptSignature)
        {
        }
    }

    public class RejectSignatureRequestMessage : WebSocketMessage<VerifySignatureRequest>
    {
        public RejectSignatureRequestMessage() : base(WebSocketMethod.RejectSignature)
        {
        }
    }

    public class AcceptPaymentRequestMessage : WebSocketMessage<AcceptPayment>
    {
        public AcceptPaymentRequestMessage() : base(WebSocketMethod.ConfirmPayment)
        {
        }
    }

    public class RejectPaymentRequestMessage : WebSocketMessage<RejectPayment>
    {
        public RejectPaymentRequestMessage() : base(WebSocketMethod.RejectPayment)
        {
        }
    }

    public class VaultCardRequestMessage : WebSocketMessage<VaultCardMessage>
    {
        public VaultCardRequestMessage() : base(WebSocketMethod.VaultCard)
        {
        }
    }

    public class ReadCardDataRequestMessage : WebSocketMessage<ReadCardDataRequest>
    {
        public ReadCardDataRequestMessage() : base(WebSocketMethod.ReadCardData)
        {
        }
    }

    public class LogMessageRequestMessage : WebSocketMessage<LogMessage>
    {
        public LogMessageRequestMessage() : base(WebSocketMethod.LogMessage)
        {
        }
    }

    public class RetrievePendingPaymentsRequestMessage : WebSocketMessage<RetrievePendingPaymentsMessage>
    {
        public RetrievePendingPaymentsRequestMessage() : base(WebSocketMethod.RetrievePendingPayments)
        {
        }
    }

    public class CustomActivityRequestMessage : WebSocketMessage<CustomActivityRequest>
    {
        public CustomActivityRequestMessage() : base(WebSocketMethod.StartCustomActivity)
        {
        }
    }

    public class MessageToActivityMessage : WebSocketMessage<MessageToActivity>
    {
        public MessageToActivityMessage() : base(WebSocketMethod.SendMessageToActivity)
        {
        }
    }

    public class RetrieveDeviceStatusMessage : WebSocketMessage<RetrieveDeviceStatusRequest>
    {
        public RetrieveDeviceStatusMessage() : base(WebSocketMethod.RetrieveDeviceStatus)
        {
        }
    }

    public class RetrievePaymentRequestMessage : WebSocketMessage<RetrievePaymentRequest>
    {
        public RetrievePaymentRequestMessage() : base(WebSocketMethod.RetrievePaymentRequest)
        {
        }
    }

    public class RetrievePrintJobStatusRequestMessage : WebSocketMessage<PrintJobStatusRequest>
    {
        public RetrievePrintJobStatusRequestMessage() : base(WebSocketMethod.PrintJobStatusRequest) { }
    }

    public class RetrievePrintersRequestMessage : WebSocketMessage<RetrievePrintersRequest>
    {
        public RetrievePrintersRequestMessage() : base(WebSocketMethod.RetrievePrintersRequest) { }
    }

    public class DisplayReceiptOptionsRequestMessage : WebSocketMessage<DisplayReceiptOptionsRequest>
    {
        public DisplayReceiptOptionsRequestMessage() : base(WebSocketMethod.DisplayReceiptOptionsRequest) { }
    }

    public class RegisterForCustomerProvidedDataRequestMessage : WebSocketMessage<RegisterForCustomerProvidedDataRequest>
    {
        public RegisterForCustomerProvidedDataRequestMessage() : base(WebSocketMethod.RegisterForCustomerProvidedDataRequest) { }
    }

    public class SetCustomerInfoRequestMessage : WebSocketMessage<SetCustomerInfoRequest>
    {
        public SetCustomerInfoRequestMessage() : base(WebSocketMethod.SetCustomerInfoRequest) { }
    }


    // callback methods

    public class OnVaultCardResponseMessage : WebSocketMessage<VaultCardResponse>
    {
        public OnVaultCardResponseMessage() : base(WebSocketMethod.VaultCardResponse)
        {
        }
    }

    public class OnReadCardDataResponseMessage : WebSocketMessage<ReadCardDataResponse>
    {
        public OnReadCardDataResponseMessage() : base(WebSocketMethod.ReadCardDataResponse)
        {
        }
    }

    public class OnCustomActivityResponseMessage : WebSocketMessage<CustomActivityResponse>
    {
        public OnCustomActivityResponseMessage() : base(WebSocketMethod.CustomActivityResponse)
        {
        }
    }

    public class OnTipAddedMessage : WebSocketMessage<TipAddedMessage>
    {
        public OnTipAddedMessage() : base(WebSocketMethod.TipAdded)
        {
        }
    }

    public class OnDeviceDisconnectedMessage : WebSocketMessage<object>
    {
        public OnDeviceDisconnectedMessage() : base(WebSocketMethod.DeviceDisconnected)
        {
        }
    }

    public class OnDeviceConnectedMessage : WebSocketMessage<object>
    {
        public OnDeviceConnectedMessage() : base(WebSocketMethod.DeviceConnected)
        {
        }
    }

    public class OnDeviceReadyMessage : WebSocketMessage<object>
    {
        public OnDeviceReadyMessage() : base(WebSocketMethod.DeviceReady)
        {
        }
    }

    public class OnDeviceErrorMessage : WebSocketMessage<remotepay.sdk.CloverDeviceErrorEvent>
    {
        public OnDeviceErrorMessage() : base(WebSocketMethod.DeviceError)
        {
        }
    }

    public class OnDeviceActivityStartMessage : WebSocketMessage<CloverDeviceEvent>
    {
        public OnDeviceActivityStartMessage() : base(WebSocketMethod.DeviceActivityStart)
        {
        }
    }

    public class OnDeviceActivityEndMessage : WebSocketMessage<CloverDeviceEvent>
    {
        public OnDeviceActivityEndMessage() : base(WebSocketMethod.DeviceActivityEnd)
        {
        }
    }

    public class OnSaleResponseMessage : WebSocketMessage<SaleResponse>
    {
        public OnSaleResponseMessage() : base(WebSocketMethod.SaleResponse)
        {
        }
    }
    public class OnPreAuthResponseMessage : WebSocketMessage<PreAuthResponse>
    {
        public OnPreAuthResponseMessage() : base(WebSocketMethod.PreAuthResponse)
        {
        }
    }

    public class OnAuthResponseMessage : WebSocketMessage<AuthResponse>
    {
        public OnAuthResponseMessage() : base(WebSocketMethod.AuthResponse)
        {
        }
    }

    public class OnCloseoutResponseMessage : WebSocketMessage<CloseoutResponse>
    {
        public OnCloseoutResponseMessage() : base(WebSocketMethod.CloseoutResponse)
        {
        }
    }

    public class OnRefundPaymentResponseMessage : WebSocketMessage<RefundPaymentResponse>
    {
        public OnRefundPaymentResponseMessage() : base(WebSocketMethod.RefundPaymentResponse)
        {
        }
    }

    public class OnManualRefundResponseMessage : WebSocketMessage<ManualRefundResponse>
    {
        public OnManualRefundResponseMessage() : base(WebSocketMethod.ManualRefundResponse)
        {
        }
    }

    public class OnVoidPaymentResponseMessage : WebSocketMessage<VoidPaymentResponse>
    {
        public OnVoidPaymentResponseMessage() : base(WebSocketMethod.VoidPaymentResponse)
        {
        }
    }

    public class OnVoidPaymentRefundResponseMessage : WebSocketMessage<VoidPaymentRefundResponse>
    {
        public OnVoidPaymentRefundResponseMessage() : base(WebSocketMethod.VoidPaymentRefundResponse)
        {
        }
    }

    public class OnCapturePreAuthResponseMessage : WebSocketMessage<CapturePreAuthResponse>
    {
        public OnCapturePreAuthResponseMessage() : base(WebSocketMethod.CapturePreAuthResponse)
        {
        }
    }

    public class OnTipAdjustAuthResponseMessage : WebSocketMessage<TipAdjustAuthResponse>
    {
        public OnTipAdjustAuthResponseMessage() : base(WebSocketMethod.TipAdjustAuthResponse)
        {
        }
    }

    public class OnVerifySignatureRequestMessage : WebSocketMessage<VerifySignatureRequest>
    {
        public OnVerifySignatureRequestMessage() : base(WebSocketMethod.VerifySignatureRequest)
        {
        }
    }

    public class OnConfirmPaymentRequestMessage : WebSocketMessage<ConfirmPaymentRequest>
    {
        public OnConfirmPaymentRequestMessage() : base(WebSocketMethod.ConfirmPaymentRequest)
        {
        }
    }

    public class OnRetrievePendingPaymentsResponseMessage : WebSocketMessage<RetrievePendingPaymentsResponse>
    {
        public OnRetrievePendingPaymentsResponseMessage() : base(WebSocketMethod.RetrievePendingPaymentsResponse)
        {
        }
    }

    public class CustomActivityResponseMessage : WebSocketMessage<CustomActivityResponseMessage>
    {
        public CustomActivityResponseMessage() : base(WebSocketMethod.CustomActivityResponse)
        {
        }
    }

    public class OnPrintPaymentReceiptMessage : WebSocketMessage<PrintPaymentReceiptMessage>
    {
        public OnPrintPaymentReceiptMessage() : base(WebSocketMethod.PrintPaymentReceipt)
        {
        }
    }

    public class OnPrintPaymentDeclinedReceiptMessage : WebSocketMessage<PrintPaymentDeclineReceiptMessage>
    {
        public OnPrintPaymentDeclinedReceiptMessage() : base(WebSocketMethod.PrintPaymentDeclinedReceipt)
        {
        }
    }

    public class OnPrintManualRefundReceiptMessage : WebSocketMessage<PrintManualRefundReceiptMessage>
    {
        public OnPrintManualRefundReceiptMessage() : base(WebSocketMethod.PrintManualRefundReceipt)
        {
        }
    }

    public class OnPrintManualRefundDeclinedReceiptMessage : WebSocketMessage<PrintManualRefundDeclineReceiptMessage>
    {
        public OnPrintManualRefundDeclinedReceiptMessage() : base(WebSocketMethod.PrintManualRefundDeclinedReceipt)
        {
        }
    }

    public class OnPrintPaymentMerchatCopyReceiptMessage : WebSocketMessage<PrintPaymentMerchantCopyReceiptMessage>
    {
        public OnPrintPaymentMerchatCopyReceiptMessage() : base(WebSocketMethod.PrintPaymentMerchantCopyReceipt)
        {
        }
    }

    public class OnPrintPaymentRefundReceiptMessage : WebSocketMessage<PrintRefundPaymentReceiptMessage>
    {
        public OnPrintPaymentRefundReceiptMessage() : base(WebSocketMethod.PrintPaymentRefundReceipt)
        {
        }
    }

    public class OnMessageFromActivityMessage : WebSocketMessage<MessageFromActivity>
    {
        public OnMessageFromActivityMessage() : base(WebSocketMethod.MessageFromActivity)
        {
        }
    }

    public class OnResetDeviceResponseMessage : WebSocketMessage<ResetDeviceResponse>
    {
        public OnResetDeviceResponseMessage() : base(WebSocketMethod.ResetDeviceResponse)
        {
        }
    }

    public class OnRetrieveDeviceStatusResponseMessage : WebSocketMessage<RetrieveDeviceStatusResponse>
    {
        public OnRetrieveDeviceStatusResponseMessage() : base(WebSocketMethod.RetrieveDeviceStatusResponse)
        {
        }
    }

    public class OnRetrievePaymentResponseMessage : WebSocketMessage<RetrievePaymentResponse>
    {
        public OnRetrievePaymentResponseMessage() : base(WebSocketMethod.RetrievePaymentResponse)
        {
        }
    }

    public class OnPrintJobStatusResponseMessage : WebSocketMessage<PrintJobStatusResponse>
    {
        public OnPrintJobStatusResponseMessage() : base(WebSocketMethod.PrintJobStatusResponse)
        {
        }
    }

    public class OnPrintJobStatusRequestMessage : WebSocketMessage<PrintJobStatusRequest>
    {
        public OnPrintJobStatusRequestMessage() : base(WebSocketMethod.PrintJobStatusRequest) { }
    }

    public class OnRetrievePrintersResponseMessage : WebSocketMessage<RetrievePrintersResponse>
    {
        public OnRetrievePrintersResponseMessage() : base(WebSocketMethod.RetrievePrintersResponse) { }
    }

    public class OnDisplayReceiptOptionsResponseMessage : WebSocketMessage<DisplayReceiptOptionsResponse>
    {
        public OnDisplayReceiptOptionsResponseMessage() : base(WebSocketMethod.DisplayReceiptOptionsResponse) { }
    }

    public class OnCustomerProvidedDataResponseMessage : WebSocketMessage<CustomerProvidedDataEvent>
    {
        public OnCustomerProvidedDataResponseMessage() : base(WebSocketMethod.CustomerProvidedDataResponse) { }
    }
}
