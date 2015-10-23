﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using com.clover.remotepay.sdk;
using com.clover.remotepay.transport;
using com.clover.remote.order;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;

namespace CloverExamplePOS
{
    public partial class CloverExamplePOSForm : Form, CloverConnectorListener
    {
        CloverConnector cloverConnector;
        Store Store;
        SynchronizationContext uiThread;

        DisplayOrder DisplayOrder;
        Dictionary<POSLineItem, DisplayLineItem> posLineItemToDisplayLineItem = new Dictionary<POSLineItem, DisplayLineItem>();
        POSLineItem SelectedLineItem = null;

        CloverDeviceConfiguration USBConfig = new USBCloverDeviceConfiguration("__deviceID__");
        CloverDeviceConfiguration TestConfig = new TestCloverDeviceConfiguration();
        CloverDeviceConfiguration WebSocketConfig = new WebSocketCloverDeviceConfiguration("10.0.1.193", 14285);

        private Dictionary<string, object> TempObjectMap = new Dictionary<string, object>();

        string OriginalFormTitle;

        public CloverExamplePOSForm()
        {
            //new CaptureLog();

            InitializeComponent();
            uiThread = WindowsFormsSynchronizationContext.Current;
        }

        private void ExamplePOSForm_Load(object sender, EventArgs e)
        {
            // some UI cleanup...
            RegisterTabs.Appearance = TabAppearance.FlatButtons;
            RegisterTabs.ItemSize = new Size(0, 1);
            RegisterTabs.SizeMode = TabSizeMode.Fixed;
            // done hiding tabs

            OriginalFormTitle = this.Text;
            InitializeConnector(TestConfig);

            Store = new Store();
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Hamburger ", 439));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Cheeseburger ", 499));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Dbl. Hamburger ", 559));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Dbl. Cheeseburger ", 629));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Chicken Sandwich ", 699));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Deluxe Chicken Sandwich ", 749));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "French Fries - Small ", 189));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "French Fries - Medium ", 229));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "French Fries - Large ", 269));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Soft Drink - Small ", 174));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Soft Drink - Medium ", 189));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Soft Drink - Large ", 229));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Milk Shake - Vanilla ", 389));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Milk Shake - Chocolate ", 399));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Milk Shake - Strawberry ", 399));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Gift Card - $25 ", 2500, false));
            Store.AvailableItems.Add(new POSItem(Guid.NewGuid().ToString(), "Gift Card - $50 ", 5000, false));

            Store.AvailableDiscounts.Add(new POSDiscount("None", 0));
            Store.AvailableDiscounts.Add(new POSDiscount("10% Off", 0.1f));
            Store.AvailableDiscounts.Add(new POSDiscount("$5 Off", 500));


            foreach (POSItem item in Store.AvailableItems)
            {
                StoreItem si = new StoreItem();
                si.Item = item;
                si += StoreItems_ItemSelected;

                StoreItems.Controls.Add(si);
            }

            foreach (POSDiscount discount in Store.AvailableDiscounts)
            {
                StoreDiscount si = new StoreDiscount();
                si.Discount = discount;
                si += StoreItems_DiscountSelected;

                StoreDiscounts.Controls.Add(si);
            }

            NewOrder();

            UpdateUI();

        }

        //////////////// Sale methods /////////////
        private void PayButton_Click(object sender, EventArgs e)
        {
            StoreItems.BringToFront();
            StoreDiscounts.BringToFront();

            PayButton.Enabled = false;
            StoreItems.Enabled = false;
            newOrderBtn.Enabled = false;

            SaleRequest request = new SaleRequest();
            request.Amount = Store.CurrentOrder.Total;
            request.TipAmount = 0;
            if(cloverConnector.Sale(request)  < 0)
            {
                PaymentReset();
            }
        }
        public void OnSaleResponse(SaleResponse response)
        {
            if (TransactionResponse.SUCCESS.Equals(response.Code))
            {
                cloverConnector.ShowThankYouScreen();
                Store.CurrentOrder.Status = POSOrder.OrderStatus.CLOSED;
                POSPayment payment = new POSPayment(response.Payment.id, response.Payment.order.id, response.Payment.employee.id, response.Payment.amount, response.Payment.tipAmount, response.Payment.cashbackAmount);
                payment.PaymentStatus = POSPayment.Status.PAID;
                Store.CurrentOrder.AddPayment(payment);


                uiThread.Send(delegate (object state)
                {
                    if (payment.CashBackAmount > 0)
                    {
                        //cloverConnector.OpenCashDrawer("Cash Back"); // not needed here, as the device will automatically open cash drawer with cash back
                        MessageBox.Show(this, "Cash Back" + (payment.CashBackAmount / 100.0).ToString("C2"), "Cash Back");
                    }
                    RegisterTabs.SelectedIndex = 0;
                    PaymentReset();
                    NewOrder();
                }, null);

                BackgroundWorker bgWorker = new BackgroundWorker();
                bgWorker.DoWork += new DoWorkEventHandler(
                    delegate (object o, DoWorkEventArgs args)
                    {
                        Thread.Sleep(3000);// wait for 3 seconds, then switch to the welcome screen unless the next order has items
                        if(Store.CurrentOrder.Items.Count == 0)
                        {
                            cloverConnector.ShowWelcomeScreen();
                        }
                    }
                );

                bgWorker.RunWorkerAsync();
            }
            else if (TransactionResponse.FAIL.Equals(response.Code))
            {
                uiThread.Send(delegate (object state)
                {
                    MessageBox.Show("Card authentication failed or was declined.");
                    PaymentReset();
                }, null);
            }
            else if (TransactionResponse.CANCEL.Equals(response.Code))
            {
                uiThread.Send(delegate (object state)
                {
                    MessageBox.Show("User canceled transaction.");
                    PaymentReset();
                }, null);
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        //////////////// Auth methods /////////////
        private void AuthButton_Click(object sender, EventArgs e)
        {

        }

        public void OnAuthResponse(AuthResponse response)
        {

        }

        private void CaptureButton_Click(object sender, EventArgs e)
        {

        }

        public void OnAuthCaptureResponse(CaptureAuthResponse response)
        {

        }

        private void AdjustTipButton_Click(object sender, EventArgs e)
        {

        }

        public void OnAuthTipAdjustResponse(TipAdjustAuthResponse response)
        {

        }

        //////////////// Closeout methods /////////////
        private void CloseoutButton_Click(object sender, EventArgs e)
        {
            cloverConnector.Closeout(new CloseoutRequest());
        }
        public void OnCloseoutResponse(CloseoutResponse response)
        {
            MessageBox.Show("Got closeout response!");
        }

        //////////////// Void methods /////////////
        private void VoidButton_Click(object sender, EventArgs e)
        {
            VoidPaymentRequest request = new VoidPaymentRequest();
            if (OrderPaymentsView.SelectedItems.Count == 1)
            {
                POSPayment payment = ((POSPayment)OrderPaymentsView.SelectedItems[0].Tag);
                request.PaymentId = payment.PaymentID;
                request.EmployeeId = payment.EmployeeID;
                request.OrderId = payment.OrderID;
                request.VoidReason = "USER_CANCEL";

                cloverConnector.VoidPayment(request);
            }
        }
        public void OnVoidPaymentResponse(VoidPaymentResponse response)
        {
            bool voided = false;
            foreach (POSOrder order in Store.Orders)
            {
                foreach (POSPayment payment in order.Payments)
                {
                    if(payment.PaymentID == response.PaymentId)
                    {
                        payment.PaymentStatus = POSPayment.Status.VOIDED;
                        voided = true;
                        break;
                    }
                }
                if(voided)
                {
                    break;
                }
            }
            uiThread.Send(delegate (object state) {
                VoidButton.Enabled = false;
                // shortbut to refresh UI
                OrderPaymentsView.SelectedItems[0].SubItems[0].Text = POSPayment.Status.VOIDED.ToString();
            }, null);

        }

        private void VoidTransactionButton_Click(object sender, EventArgs e)
        {
            VoidTransactionRequest request = new VoidTransactionRequest();
            //request.OriginalRequestUUID = ;
            cloverConnector.VoidTransaction(new VoidTransactionRequest());
        }
        public void OnVoidTransactionResponse(VoidTransactionResponse response)
        {

        }


        //////////////// Manual Refund methods /////////////
        private void ManualRefundButton_Click(object sender, EventArgs e)
        {
            ManualRefundRequest request = new ManualRefundRequest();
            request.Amount = int.Parse(RefundAmount.Text);
            cloverConnector.ManualRefund(request);
        }
        public void OnManualRefundResponse(ManualRefundResponse response)
        {

            if (TransactionResponse.SUCCESS.Equals(response.Code))
            {
                uiThread.Send(delegate (object state) {
                    ListViewItem lvi = new ListViewItem();
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

                    lvi.SubItems[0].Text = (response.Credit.amount / 100.0).ToString("C2");
                    lvi.SubItems[1].Text = new DateTime(response.Credit.createdTime).ToLongDateString();
                    lvi.SubItems[2].Text = response.Credit.cardTransaction.last4;


                    MessageBox.Show("Refund of " + (response.Credit.amount / 100.0).ToString("C2") + " was applied to card ending with " + response.Credit.cardTransaction.last4);
                    RefundAmount.Text = "0";

                    TransactionsListView.Items.Add(lvi);
                }, null);
            }
            else if (TransactionResponse.FAIL.Equals(response.Code))
            {
                uiThread.Send(delegate (object state) {
                    MessageBox.Show("Card authentication failed");
                    PaymentReset();
                }, null);
            }
            else if (TransactionResponse.CANCEL.Equals(response.Code))
            {
                uiThread.Send(delegate (object state) {
                    MessageBox.Show("User canceled transaction.");
                    PaymentReset();
                }, null);
            }

        }



        //////////////// Payment Refund methods /////////////
        private void PaymentRefundButton_Click(object sender, EventArgs e)
        {
            RefundPaymentRequest request = new RefundPaymentRequest();
            
            if (OrderPaymentsView.SelectedItems.Count == 1)
            {
                POSPayment payment = ((POSPayment)OrderPaymentsView.SelectedItems[0].Tag);
                request.PaymentId = payment.PaymentID;
                POSOrder order = (POSOrder)OrdersListView.SelectedItems[0].Tag;
                request.OrderId = payment.OrderID;
                TempObjectMap.Add(payment.OrderID, order);
                cloverConnector.RefundPayment(request);
            }
        }
        public void OnRefundPaymentResponse(RefundPaymentResponse response)
        {

            if (TxState.SUCCESS.Equals(response.Code))
            {
                uiThread.Send(delegate (object state) {
                    object orderObj;
                    TempObjectMap.TryGetValue(response.OrderId, out orderObj);
                    TempObjectMap.Remove(response.OrderId);
                    POSRefund refund = new POSRefund(response.PaymentId, response.OrderId, null, response.RefundObj.amount);

                    ((POSOrder)orderObj).AddRefund(refund);

                    ListViewItem lvi = new ListViewItem();
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

                    lvi.SubItems[0].Text = "REFUND";
                    lvi.SubItems[3].Text = (response.RefundObj.amount / 100.0).ToString("C2");

                    OrderPaymentsView.Items.Add(lvi);
                }, null);
            }
            else if (TxState.FAIL.Equals(response.Code))
            {
                uiThread.Send(delegate (object state) {
                    MessageBox.Show("Card authentication failed");
                    PaymentReset();
                }, null);
            }

        }

        ////////////////// CloverDeviceLister Methods //////////////////////
        private void PrintPaymentButton_Click(object sender, EventArgs e)
        {
            //TODO: cloverConnector.ChooseReceipt(orderID, paymentID);
        }

        public void OnDisplayReceiptOptionsResponse(DisplayReceiptOptionsResponse response)
        {

        }

        ////////////////// CloverDeviceLister Methods //////////////////////

        public void OnDeviceConnected()
        {
            uiThread.Send(delegate (object state) {
                ConnectStatusLabel.Text = "Connecting...";
            }, null);
        }

        public void OnDeviceReady()
        {
            uiThread.Send(delegate (object state) {
                ConnectStatusLabel.Text = "Connected";
                if(DisplayOrder.lineItems.elements.Count > 0)
                {
                    UpdateDisplayOrderTotals();
                    cloverConnector.DisplayOrder(DisplayOrder);
                }
                PaymentReset();
            }, null);
        }

        public void OnDeviceDisconnected()
        {
            try
            {
                uiThread.Send(delegate (object state) {
                    ConnectStatusLabel.Text = "Disconnected";
                    PaymentReset();
                }, null);
                
            }
            catch(Exception e)
            {
                // uiThread is gone on shutdown
            }
        }




        ////////////////// CloverDeviceLister Methods //////////////////////
        public void OnDeviceActivityStart(CloverDeviceEvent deviceEvent)
        {
            uiThread.Send(delegate (object state) {
                UIStateButtonPanel.Controls.Clear();
                if (deviceEvent.InputOptions != null)
                {
                    foreach (InputOption io in deviceEvent.InputOptions)
                    {
                        Button b = new Button();
                        b.FlatStyle = FlatStyle.Flat;
                        b.BackColor = Color.White;
                        b.Text = io.description;
                        b.Click += io.handler;
                        UIStateButtonPanel.Controls.Add(b);
                    }
                    
                }
                UIStateButtonPanel.Parent.PerformLayout();
                DeviceCurrentStatus.Text = deviceEvent.Message;
            }, null);
        }

        public void OnDeviceActivityEnd(CloverDeviceEvent deviceEvent)
        {
            try
            {
                uiThread.Send(delegate (object state) {
                    UIStateButtonPanel.Controls.Clear();
                    DeviceCurrentStatus.Text = " ";
                }, null);
            }
            catch(Exception e)
            {
                // if UI goes away, uiThread may be disposed
            }
        }

        public void OnDeviceError(CloverDeviceErrorEvent deviceErrorEvent)
        {
            MessageBox.Show(deviceErrorEvent.Message);
        }




        ////////////////// CloverSignatureLister Methods //////////////////////
        /// <summary>
        /// Handle a request from the Clover device to verify a signature
        /// </summary>
        /// <param name="request"></param>
        public void OnSignatureVerifyRequest(SignatureVerifyRequest request)
        {
            uiThread.Send(delegate (object state)
            {
                SignatureForm sigForm = new SignatureForm();
                sigForm.SignatureVerifyRequest = request;
                sigForm.ShowDialog(this);
            }, null);
            
        }



        public void OnError(Exception e)
        {
            MessageBox.Show(this, e.Message, "Error");
        }

        ////////////////// CloverTipListener Methods //////////////////////
        public void OnTipAdded(TipAddedMessage message)
        {
            if (message.tipAmount > 0)
            {
                string msg = "Tip Added: " + (message.tipAmount / 100.0).ToString("C2");
                OnDeviceActivityStart(new CloverDeviceEvent(0, msg));
            }
        }


        ////////////////// UI Events and UI Management //////////////////////

        private void StoreItems_ItemSelected(object sender, EventArgs e)
        {
            POSItem item = ((StoreItem)((Control)sender).Parent).Item;
            POSLineItem lineItem = Store.CurrentOrder.AddItem(item, 1);

            DisplayLineItem displayLineItem = null;
            posLineItemToDisplayLineItem.TryGetValue(lineItem, out displayLineItem);
            if (displayLineItem == null)
            {
                displayLineItem = DisplayFactory.createDisplayLineItem();
                posLineItemToDisplayLineItem[lineItem] = displayLineItem;
                displayLineItem.quantity = "1";
                displayLineItem.name = lineItem.Item.Name;
                displayLineItem.price = (lineItem.Item.Price / 100.0).ToString("C2");
                DisplayOrder.addDisplayLineItem(displayLineItem);
                UpdateDisplayOrderTotals();
                cloverConnector.DisplayOrderLineItemAdded(DisplayOrder, displayLineItem);
            }
            else
            {
                displayLineItem.quantity = lineItem.Quantity.ToString();
                UpdateDisplayOrderTotals();
                cloverConnector.DisplayOrder(DisplayOrder);
            }

            UpdateUI();
        }



        ////////////////// UI Events and UI Management //////////////////////

        private void StoreItems_DiscountSelected(object sender, EventArgs e)
        {
            POSDiscount discount = ((StoreDiscount)((Control)sender).Parent).Discount;
            Store.CurrentOrder.Discount = discount;

            DisplayDiscount DisplayDiscount = new DisplayDiscount();
            DisplayDiscount.name = discount.Name;
            // our business rules say only 1 order discount
            while(DisplayOrder.discounts.elements.Count > 0)
            {
                DisplayDiscount RemovedDisplayDiscount = (DisplayDiscount)DisplayOrder.discounts.elements[0];
                DisplayOrder.discounts.Remove(RemovedDisplayDiscount);
                UpdateDisplayOrderTotals();
                cloverConnector.DisplayOrderDiscountRemoved(DisplayOrder, RemovedDisplayDiscount);
            }

            if(discount.Value(1000) != 0)
            {
                DisplayOrder.addDisplayDiscount(DisplayDiscount);
                UpdateDisplayOrderTotals();
                cloverConnector.DisplayOrderDiscountAdded(DisplayOrder, DisplayDiscount);
            }

            UpdateUI();
        }

        private void UpdateDisplayOrderTotals()
        {
            DisplayOrder.tax = (Store.CurrentOrder.TaxAmount / 100.0).ToString("C2");
            DisplayOrder.subtotal = (Store.CurrentOrder.PreTaxSubTotal / 100.0).ToString("C2");
            DisplayOrder.total = (Store.CurrentOrder.Total / 100.0).ToString("C2");
        }

        private void NewOrder_Click(object sender, EventArgs e)
        {
            NewOrder();
        }

        private void NewOrder()
        {
            Store.CreateOrder();
            StoreItems.BringToFront();
            StoreDiscounts.BringToFront();

            DisplayOrder = DisplayFactory.createDisplayOrder();
            DisplayOrder.title = Guid.NewGuid().ToString();
            posLineItemToDisplayLineItem.Clear();

            cloverConnector.ShowWelcomeScreen();
            //cloverConnector.DisplayOrder(DisplayOrder); // want the welcome screen until something is added to the order

            PayButton.Enabled = true;
            StoreItems.Enabled = true;
            TabControl.Enabled = true;

            RegisterTabs.SelectedIndex = 0;

            UpdateUI();
        }

        private void UpdateUI()
        {
            currentOrder.Text = Store.CurrentOrder.ID;
            OrderItems.Items.Clear();

            foreach (POSLineItem item in Store.CurrentOrder.Items)
            {
                ListViewItem lvi = new ListViewItem();

                lvi.Tag = item;
                lvi.Name = item.Item.Name;

                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

                lvi.SubItems[0].Text = "" + item.Quantity;
                lvi.SubItems[1].Text = item.Item.Name;
                lvi.SubItems[2].Text = (item.Item.Price / 100.0).ToString("C2");
                lvi.SubItems[3].ForeColor = Color.ForestGreen;
                lvi.SubItems[3].Text = (item.Discount == null) ? "" : "-" + (item.Discount.Value(item.Item) / 100.0).ToString("C2");
                

                OrderItems.Items.Add(lvi);
            }

            if(Store.CurrentOrder.Discount.Value(1000) != 0)
            {
                DiscountLabel.Text = (Store.CurrentOrder.Discount.Name) + "     -" + (Store.CurrentOrder.Discount.Value(Store.CurrentOrder.PreDiscountSubTotal) / 100.0).ToString("C2");
            }
            else
            {
                DiscountLabel.Text = " ";
            }
            SubTotal.Text = (Store.CurrentOrder.PreTaxSubTotal / 100.0).ToString("C2");
            TaxAmount.Text = (Store.CurrentOrder.TaxAmount / 100.0).ToString("C2");
            TotalAmount.Text = (Store.CurrentOrder.Total / 100.0).ToString("C2");
        }

        private void TabControl_SelectedIndexChanged(Object sender, EventArgs ev)
        {
            OrdersListView.Items.Clear();

            if (TabControl.SelectedIndex == 1)
            {
                foreach (POSOrder order in Store.Orders)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = order;
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

                    lvi.SubItems[0].Text = order.ID;
                    lvi.SubItems[1].Text = (order.Total / 100.0).ToString("C2");
                    lvi.SubItems[2].Text = order.Date.ToString();
                    lvi.SubItems[3].Text = order.Status.ToString();
                    lvi.SubItems[4].Text = (order.PreTaxSubTotal / 100.0).ToString("C2");
                    lvi.SubItems[5].Text = (order.TaxAmount / 100.0).ToString("C2");

                    OrdersListView.Items.Add(lvi);
                }
            }
        }

        private void OrdersListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrderDetailsListView.Items.Clear();
            if (OrdersListView.SelectedIndices.Count == 1)
            {
                ListViewItem lvi = OrdersListView.SelectedItems[0];

                POSOrder selOrder = (POSOrder)lvi.Tag;

                OrderDetailsListView.Items.Clear();

                // update order items table
                foreach (POSLineItem lineItem in selOrder.Items)
                {
                    lvi = new ListViewItem();
                    lvi.Tag = lineItem;
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

                    lvi.SubItems[0].Text = lineItem.Quantity + "";
                    lvi.SubItems[1].Text = lineItem.Item.Name;
                    lvi.SubItems[2].Text = (lineItem.Item.Price / 100.0).ToString("C2");

                    OrderDetailsListView.Items.Add(lvi);
                }

                // update order payments table
                OrderPaymentsView.Items.Clear();
                foreach (var Exchange in selOrder.Payments)
                {
                    lvi = new ListViewItem();
                    lvi.Tag = Exchange;
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

                    if (Exchange is POSPayment)
                    {
                        lvi.SubItems[0].Text = (Exchange is POSPayment) ? ((POSPayment)Exchange).PaymentStatus.ToString() : "";
                        lvi.SubItems[1].Text = (Exchange.Amount / 100.0).ToString("C2");
                        lvi.SubItems[2].Text = (Exchange is POSPayment) ? (((POSPayment)Exchange).TipAmount / 100.0).ToString("C2") : "";
                        lvi.SubItems[3].Text = (Exchange is POSPayment) ? ((((POSPayment)Exchange).TipAmount + Exchange.Amount) / 100.0).ToString("C2") : (Exchange.Amount / 100.0).ToString("C2");
                    }
                    else if (Exchange is POSRefund)
                    {
                        lvi.SubItems[0].Text = "REFUND";
                        lvi.SubItems[3].Text = (Exchange.Amount / 100.0).ToString("C2");
                    }

                    OrderPaymentsView.Items.Add(lvi);
                }
            }
        }
        public void PaymentReset()
        {
            PayButton.Enabled = true;
            StoreItems.Enabled = true;
            TabControl.Enabled = true;

            if(DisplayOrder.lineItems.elements.Count > 0)
            {
                cloverConnector.DisplayOrder(DisplayOrder);
            }

            UpdateUI();
        }
        private void OrderPaymentsView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OrderPaymentsView.SelectedIndices.Count == 1 && (OrderPaymentsView.SelectedItems[0].Tag is POSPayment) && ((POSPayment)OrderPaymentsView.SelectedItems[0].Tag).PaymentStatus == POSPayment.Status.PAID)
            {
                VoidButton.Enabled = true;
            }
            else
            {
                VoidButton.Enabled = false;
            }
        }
        // only allow numbers to be entered
        private void RefundAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.')/* && ((sender as TextBox).Text.IndexOf('.') > -1)*/)
            {
                e.Handled = true;
            }


            int result = 0;
            if (!int.TryParse(RefundAmount.Text, out result))
            {
                RefundAmount.BackColor = Color.Red;
                ManualRefundButton.Enabled = false;
            }
            else
            {
                RefundAmount.BackColor = Color.White;
                ManualRefundButton.Enabled = true;
            }
        }

        private void ExamplePOSForm_Closed(object sender, FormClosedEventArgs e)
        {
            cloverConnector.ShowWelcomeScreen();
        }

        private void OrderItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Need to show item panel?
            if (OrderItems.SelectedItems.Count == 1)
            {
                POSLineItem lineItem = (POSLineItem)((ListViewItem)OrderItems.SelectedItems[0]).Tag;
                SelectedLineItem = lineItem;
                ItemNameLabel.Text = lineItem.Item.Name;
                ItemQuantityTextbox.Text = lineItem.Quantity.ToString();
                // enable/disable Discount button. Can't add it twice...
                DiscountButton.Enabled = lineItem.Discount == null;
            }
            RegisterTabs.SelectedIndex = 1;

        }

        private void IncrementQuantityButton_Click(object sender, EventArgs e)
        {
            SelectedLineItem.Quantity++;
            ItemQuantityTextbox.Text = "" + SelectedLineItem.Quantity;
            UpdateDisplayOrderTotals();
            posLineItemToDisplayLineItem[SelectedLineItem].quantity = "" + SelectedLineItem.Quantity;
            cloverConnector.DisplayOrder(DisplayOrder);
            UpdateUI();
        }

        private void DecrementQuantityButton_Click(object sender, EventArgs e)
        {
            SelectedLineItem.Quantity--;
            if (SelectedLineItem.Quantity == 0)
            {
                RemoveSelectedItemFromCurrentOrder();
            }
            else
            {
                ItemQuantityTextbox.Text = "" + SelectedLineItem.Quantity;
                UpdateDisplayOrderTotals();
                posLineItemToDisplayLineItem[SelectedLineItem].quantity = "" + SelectedLineItem.Quantity;
                cloverConnector.DisplayOrder(DisplayOrder);
                UpdateUI();
            }
        }

        private void RemoveItemButton_Click(object sender, EventArgs e)
        {
            RemoveSelectedItemFromCurrentOrder();
        }

        private void RemoveSelectedItemFromCurrentOrder()
        {
            Store.CurrentOrder.RemoveItem(SelectedLineItem);
            DisplayLineItem dli = posLineItemToDisplayLineItem[SelectedLineItem];
            DisplayOrder.removeDisplayLineItem(dli);
            UpdateDisplayOrderTotals();
            cloverConnector.DisplayOrderLineItemRemoved(DisplayOrder, dli);
            RegisterTabs.SelectedIndex = 0;
            UpdateUI();
        }

        private void DiscountButton_Click(object sender, EventArgs e)
        {
            SelectedLineItem.Discount = new POSLineItemDiscount(0.1f, "10% Off");
            DisplayDiscount discount = new DisplayDiscount();
            DisplayLineItem displayLineItem = posLineItemToDisplayLineItem[SelectedLineItem];
            discount.lineItemId = displayLineItem.id;
            displayLineItem.addDiscount(discount);

            discount.name = SelectedLineItem.Discount.Name;
            discount.percentage = "10";
            discount.amount = (SelectedLineItem.Discount.Value(SelectedLineItem.Item) * SelectedLineItem.Quantity / 100.0).ToString("C2");

            UpdateDisplayOrderTotals();
            cloverConnector.DisplayOrder(DisplayOrder);

            DiscountButton.Enabled = false;
            UpdateUI();
        }

        private void DoneEditingLineItem_Click(object sender, EventArgs e)
        {
            RegisterTabs.SelectedIndex = 0;
            UpdateUI();
        }

        private void InitializeConnector(CloverDeviceConfiguration config)
        {
            if(cloverConnector != null)
            {
                cloverConnector -= (this);


                OnDeviceDisconnected(); // for any disabling, messaging, etc.
                PayButton.Enabled = false; // everything can work except Pay
            }

            cloverConnector = new CloverConnector(config);

            cloverConnector += this;

            //ui cleanup
            this.Text = OriginalFormTitle + " - " + config.getName();
            if (config is TestCloverDeviceConfiguration)
            {
                TestDeviceMenuItem.Checked = true;
                CloverMiniUSBMenuItem.Checked = false;
                WebSocketMenuItem.Checked = false;
            }
            else if (config is USBCloverDeviceConfiguration)
            {
                TestDeviceMenuItem.Checked = false;
                CloverMiniUSBMenuItem.Checked = true;
                WebSocketMenuItem.Checked = false;
            }
            else if (config is WebSocketCloverDeviceConfiguration)
            {
                TestDeviceMenuItem.Checked = false;
                CloverMiniUSBMenuItem.Checked = false;
                WebSocketMenuItem.Checked = true;
            }
        }

        private void PrintTextButton_Click(object sender, EventArgs e)
        {
            List<string> messages = new List<string>();
            messages.Add(PrintTextBox.Text);
            cloverConnector.PrintText(messages);
        }

        private void BrowseImageButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();

            open.Filter = "Image Files(*.jpg, *.jpeg, *.gif, *.bmp, *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            if (open.ShowDialog() == DialogResult.OK)
            {
                string filename = open.FileName;
                try
                {
                    Bitmap img = (Bitmap)Bitmap.FromFile(filename);
                    PrintImage.Image = img;
                }
                catch (FileNotFoundException fnfe)
                {
                    MessageBox.Show("Invalid file: " + filename);
                }
                catch (ArgumentException ae)
                {
                    MessageBox.Show("Invalid Image file");
                }
            }
        }

        private void PrintImageButton_Click(object sender, EventArgs e)
        {
            if (PrintImage.Image != null && PrintImage.Image is Bitmap)
            {
                cloverConnector.PrintImage((Bitmap)PrintImage.Image);
            }
            else
            {
                MessageBox.Show("Invalid Image");
            }
        }

        private void DisplayMessageButton_Click(object sender, EventArgs e)
        {
            cloverConnector.ShowMessage(DisplayMessageTextbox.Text);
        }

        private void ShowWelcomeButton_Click(object sender, EventArgs e)
        {
            cloverConnector.ShowWelcomeScreen();
        }

        private void ShowReceiptButton_Click(object sender, EventArgs e)
        {
            cloverConnector.DisplayReceiptOptions();
        }

        private void ShowThankYouButton_Click(object sender, EventArgs e)
        {
            cloverConnector.ShowThankYouScreen();
        }

        private void OpenCashDrawerButton_Click(object sender, EventArgs e)
        {
            cloverConnector.OpenCashDrawer("Test");
        }



        private void TestDeviceMenuItem_Click(object sender, EventArgs e)
        {
            InitializeConnector(TestConfig);
        }

        private void CloverMiniUSBMenuItem_Click(object sender, EventArgs e)
        {
            InitializeConnector(USBConfig);
        }

        private void WebSocketMenuItem_Click(object sender, EventArgs e)
        {
            InputForm iform = new InputForm();
            iform.Title = "WebSocket Host Configuration";
            iform.Label = "Enter Device IP:Port(ex: 10.0.1.13:8080)";
            iform.Value = ((WebSocketCloverDeviceConfiguration)WebSocketConfig).hostname + ":" + ((WebSocketCloverDeviceConfiguration)WebSocketConfig).port;
            iform.FormClosed += WSForm_Closed;
            iform.Show();
        }

        private void WSForm_Closed(object sender, EventArgs e)
        {
            if (((InputForm)sender).Status == DialogResult.OK)
            {
                string val = ((InputForm)sender).Value;
                string[] tokens = val.Split(':');
                if (tokens.Length == 2)
                {
                    //TODO: validate IP and port
                    string ip = tokens[0];
                    int port = Int32.Parse(tokens[1]);
                    WebSocketConfig = new WebSocketCloverDeviceConfiguration(ip, port);
                    InitializeConnector(WebSocketConfig);
                }
            }
        }

    }
}
