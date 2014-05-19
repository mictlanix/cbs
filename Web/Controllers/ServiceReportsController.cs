﻿// 
// ServiceReportsController.cs
// 
// Author:
//   Eddy Zavaleta <eddy@mictlanix.com>
// 
// Copyright (C) 2014 Eddy Zavaleta, Mictlanix, and contributors.
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
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Castle.ActiveRecord;
using NHibernate;
using NHibernate.Exceptions;
using Mictlanix.BE.Model;
using Mictlanix.BE.Web.Models;
using Mictlanix.BE.Web.Helpers;

namespace Mictlanix.BE.Web.Controllers
{
	[Authorize]
	public class ServiceReportsController : Controller
    {
		public ActionResult Index ()
		{
			var search = Search (new Search<ServiceReport> {
				Limit = Configuration.PageSize
			});

			if (Request.IsAjaxRequest ()) {
				return PartialView ("_Index", search);
			} else {
				return View (search);
			}
		}

		[HttpPost]
		public ActionResult Index (Search<ServiceReport> search)
		{
			if (ModelState.IsValid) {
				search = Search (search);
			}

			if (Request.IsAjaxRequest ()) {
				return PartialView ("_Index", search);
			} else {
				return View (search);
			}
		}

		Search<ServiceReport> Search (Search<ServiceReport> search)
		{
			IQueryable<ServiceReport> query;
			var pattern = string.Format("{0}", search.Pattern).Trim ();

			if (string.IsNullOrWhiteSpace (pattern)) {
				query = from x in ServiceReport.Queryable
					orderby x.Date descending
				        select x;
			} else {
				query = from x in ServiceReport.Queryable
				        where x.Customer.Name.Contains (pattern) ||
				            x.Type.Contains (pattern)
						orderby x.Date descending
				        select x;
			}

			search.Total = query.Count ();
			search.Results = query.Skip (search.Offset).Take (search.Limit).ToList ();

			return search;
		}

		public ActionResult Create ()
        {
			return PartialView ("_Create");
		}

        [HttpPost]
		public ActionResult Create (ServiceReport item)
		{
			item.Customer = Customer.TryFind (item.CustomerId);
			item.Supplier = Supplier.TryFind (item.SupplierId);

			if (!ModelState.IsValid) {
				return PartialView ("_Create", item);
			}

			using (var scope = new TransactionScope ()) {
				item.CreateAndFlush ();
			}

			return PartialView ("_CreateSuccesful", item);
		}

		public ActionResult View (int id)
		{
			var item = ServiceReport.Find (id);
			return PartialView ("_View", item);
		}

		public ActionResult Edit (int id)
        {
        	var item = ServiceReport.Find (id);
			return PartialView ("_Edit", item);
        }

        [HttpPost]
        public ActionResult Edit (ServiceReport item)
		{
			item.Customer = Customer.TryFind (item.CustomerId);
			item.Supplier = Supplier.TryFind (item.SupplierId);

			if (!ModelState.IsValid) {
				return PartialView ("_Edit", item);
			}
			
			var entity = ServiceReport.Find (item.Id);

			entity.Date = item.Date;
			entity.Customer = item.Customer;
			entity.Type = item.Type;
			entity.Location = item.Location;
			entity.Supplier = item.Supplier;
			entity.Model = item.Model;
			entity.Brand = item.Brand;
			entity.UserReport = item.UserReport;
			entity.UserDescription = item.UserDescription;
			entity.Comment = item.Comment;

			using (var scope = new TransactionScope()) {
				entity.UpdateAndFlush ();
			}

			return PartialView ("_Refresh");
        }

		public ActionResult Delete (int id)
        {
            var item = ServiceReport.Find (id);
			return PartialView ("_Delete", item);
        }

        [HttpPost, ActionName ("Delete")]
		public ActionResult DeleteConfirmed (int id)
		{
			var item = ServiceReport.Find (id);

			try {
				using (var scope = new TransactionScope()) {
					item.DeleteAndFlush ();
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine (ex);
				return PartialView ("DeleteUnsuccessful");
			}

			return PartialView ("_DeleteSuccesful", item);
        }
    }
}