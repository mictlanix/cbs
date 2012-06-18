﻿// 
// Configuration.cs
// 
// Author:
//   Eddy Zavaleta <eddy@mictlanix.org>
//   Eduardo Nieto <enieto@mictlanix.org>
// 
// Copyright (C) 2011 Eddy Zavaleta, Mictlanix, and contributors.
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
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;
using Mictlanix.BE.Model;

namespace Mictlanix.BE.Web.Helpers
{
	public static class Configuration
	{
		public const string StoreCookieKey = "Store";
		public const string PointOfSaleCookieKey = "PointOfSale";
		public const string CashDrawerCookieKey = "CashDrawer";

		#region Application Global Settings
		
		public static string ApplicationTitle {
			get { return ConfigurationManager.AppSettings ["ApplicationTitle"]; }
		}
		public static string LogoTitle {
			get { return ConfigurationManager.AppSettings ["LogoTitle"]; }
		}
		public static string LogoAlt {
			get { return ConfigurationManager.AppSettings ["LogoAlt"]; }
		}
		public static string Company {
			get { return ConfigurationManager.AppSettings ["Company"]; }
		}
		public static string CompanyAddress {
			get { return ConfigurationManager.AppSettings ["CompanyAddress"]; }
		}
		public static string CompanyTaxpayer {
			get { return ConfigurationManager.AppSettings ["CompanyTaxpayer"]; }
		}
		public static string CompanyTaxpayerName {
			get { return ConfigurationManager.AppSettings ["CompanyTaxpayerName"]; }
		}
		public static string PromissoryNoteContent {
			get { return ConfigurationManager.AppSettings ["PromissoryNoteContent"]; }
		}
		
		public static string PhotosPath {
			get { return ConfigurationManager.AppSettings ["PhotosPath"]; }
		}
		
		public static string DefaultPhotoFile {
			get { return ConfigurationManager.AppSettings ["DefaultPhotoFile"]; }
		}
		
		public static decimal VAT {
			get { return decimal.Parse (ConfigurationManager.AppSettings ["VAT"]); }
		}
		
		public static bool IsTaxIncluded {
			get { return Convert.ToBoolean (ConfigurationManager.AppSettings ["IsTaxIncluded"]); }
		}
		
		public static string FiscalFilesPath {
			get { return ConfigurationManager.AppSettings ["FiscalFilesPath"]; }
		}
		
		#endregion
		
		#region Request's (Local) Settings
		
		public static Store Store {
			get {
				var cookie = System.Web.HttpContext.Current.Request.Cookies [StoreCookieKey];
				
				if (cookie != null) {
					return Store.TryFind (int.Parse (cookie.Value));
				}
				
				return null;
			}
		}
		
		public static PointOfSale PointOfSale {
			get {
				var cookie = System.Web.HttpContext.Current.Request.Cookies [PointOfSaleCookieKey];
				
				if (cookie != null) {
					return PointOfSale.TryFind (int.Parse (cookie.Value));
				}
				
				return null;
			}
		}
		
		public static CashDrawer CashDrawer {
			get {
				var cookie = System.Web.HttpContext.Current.Request.Cookies [CashDrawerCookieKey];
				
				if (cookie != null) {
					return CashDrawer.TryFind (int.Parse (cookie.Value));
				}
				
				return null;
			}
		}
		
		#endregion
	}
}