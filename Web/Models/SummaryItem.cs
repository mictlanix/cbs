﻿// 
// DateRange.cs
// 
// Author:
//   Eddy Zavaleta <eddy@mictlanix.com>
//   Eduardo Nieto <enieto@mictlanix.com>
//
// Copyright (C) 2011-2017 Eddy Zavaleta, Mictlanix, and contributors.
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
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using Mictlanix.BE.Model.Validation;
using Mictlanix.BE.Model;

namespace Mictlanix.BE.Web.Models {
	//FIXME: Discount
	public class SummaryItem {
		[Display(Name = "Id", ResourceType = typeof(Resources))]
		public string Id { get; set; }

		[Display(Name = "Code", ResourceType = typeof(Resources))]
		public string Code { get; set; }

		[Display(Name = "Name", ResourceType = typeof(Resources))]
		public string Name { get; set; }

		[DisplayFormat(DataFormatString = "{0:0.####}")]
		[Display(Name = "Units", ResourceType = typeof(Resources))]
		public decimal Units { get; set; }
		
		[DataType(DataType.Currency)]
		[Display(Name = "Total", ResourceType = typeof(Resources))]
		public decimal Total { get; set; }
		
		[DataType(DataType.Currency)]
		[Display(Name = "Subtotal", ResourceType = typeof(Resources))]
		public decimal Subtotal { get; set; }

		[DataType (DataType.Currency)]
		[Display (Name = "Discount", ResourceType = typeof (Resources))]
		public decimal Discount { get; set; }
		
		[DataType(DataType.Currency)]
		[Display(Name = "Taxes", ResourceType = typeof(Resources))]
		public decimal Taxes {
			get { return Total - Subtotal + Discount; }
		}

		[Display(Name = "Category", ResourceType = typeof(Resources))]
		public string Category { get; set; }
	}
}