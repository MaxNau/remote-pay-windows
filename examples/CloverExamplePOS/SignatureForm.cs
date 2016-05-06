﻿// Copyright (C) 2016 Clover Network, Inc.
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using com.clover.remotepay.sdk;
using com.clover.sdk.v3.payments;

namespace CloverExamplePOS
{
    public partial class SignatureForm : OverlayForm
    {
        private SignatureVerifyRequest signatureVerifyRequest;

        public SignatureVerifyRequest SignatureVerifyRequest {
            get {
                return signatureVerifyRequest;
            }
            set {
                signatureVerifyRequest = value;
                signaturePanel1.Signature = signatureVerifyRequest.Signature;
            }
        }
        public SignatureForm(Form toCover) : base(toCover)
        {
            InitializeComponent();
        }

        private void SignatureForm_Load(object sender, EventArgs e)
        {
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            POSCard storedCard = new POSCard();
            //storedCard.First6 = First6TextBox.Text;
            //storedCard.Name = CustomerName.Text;
            this.Dispose();
            SignatureVerifyRequest.Accept();
        }

        private void RejectButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
            SignatureVerifyRequest.Reject();
        }
    }
}
