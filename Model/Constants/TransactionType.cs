// 
// TransactionType.cs
// 
// Author:
//   Eddy Zavaleta <eddy@mictlanix.com>
//   Eduardo Nieto <enieto@mictlanix.com>
// 
// Copyright (C) 2011-2013 Eddy Zavaleta, Mictlanix, and contributors.
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;

namespace Mictlanix.BE.Model {
	public enum TransactionType : int {
		[Display (Name = "SalesOrder", ResourceType = typeof (Resources))]
		SalesOrder = 1,
		[Display (Name = "CustomerRefund", ResourceType = typeof (Resources))]
		CustomerRefund,
		[Display (Name = "InventoryIssue", ResourceType = typeof (Resources))]
		InventoryIssue,
		[Display (Name = "InventoryReceipt", ResourceType = typeof (Resources))]
		InventoryReceipt,
		[Display (Name = "InventoryTransfer", ResourceType = typeof (Resources))]
		InventoryTransfer,
		[Display (Name = "PurchaseOrder", ResourceType = typeof (Resources))]
		PurchaseOrder,
		[Display (Name = "SupplierReturn", ResourceType = typeof (Resources))]
		SupplierReturn,
		[Display (Name = "InventoryAdjustment", ResourceType = typeof (Resources))]
		InventoryAdjustment
	}
}

