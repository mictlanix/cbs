﻿// 
// PriceListsController.cs
// 
// Author:
//   Eddy Zavaleta <eddy@mictlanix.com>
//   Eduardo Nieto <enieto@mictlanix.com>
// 
// Copyright (C) 2011-2016 Eddy Zavaleta, Mictlanix, and contributors.
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
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Castle.ActiveRecord;
using Mictlanix.BE.Model;
using Mictlanix.BE.Web.Models;
using Mictlanix.BE.Web.Mvc;
using Mictlanix.BE.Web.Helpers;
using NHibernate.Exceptions;

namespace Mictlanix.BE.Web.Controllers.Mvc {
	[Authorize]
	public class PriceListsController : CustomController {
		public ViewResult Index ()
		{
			var qry = from x in PriceList.Queryable
				  where x.Id > 0
				  orderby x.Name
				  select x;

			var search = new Search<PriceList> ();
			search.Limit = WebConfig.PageSize;
			search.Results = qry.Skip (search.Offset).Take (search.Limit).ToList ();
			search.Total = qry.Count ();

			return View (search);
		}

		[HttpPost]
		public ActionResult Index (Search<PriceList> search)
		{
			if (ModelState.IsValid) {
				search = GetPriceList (search);
			}

			if (Request.IsAjaxRequest ()) {
				return PartialView ("_Index", search);
			} else {
				return View (search);
			}
		}

		Search<PriceList> GetPriceList (Search<PriceList> search)
		{
			var qry = from x in PriceList.Queryable
				  where x.Id > 0
				  orderby x.Name
				  select x;

			if (!string.IsNullOrEmpty (search.Pattern)) {
				qry = from x in PriceList.Queryable
				      where x.Id > 0 && x.Name.Contains (search.Pattern)
				      orderby x.Name
				      select x;
			}

			search.Total = qry.Count ();
			search.Results = qry.Skip (search.Offset).Take (search.Limit).ToList ();

			return search;
		}

		public ActionResult View (int id)
		{
			var item = PriceList.Find (id);
			return PartialView ("_View", item);
		}

		public ActionResult Create ()
		{
			return PartialView ("_Create");
		}

		[HttpPost]
		public ActionResult Create (PriceList item)
		{
			if (!ModelState.IsValid)
				return PartialView ("_Create", item);

			item.LowProfitMargin /= 100m;
			item.HighProfitMargin /= 100m;

			using (var scope = new TransactionScope ()) {
				item.CreateAndFlush ();
			}

			return PartialView ("_CreateSuccesful", item);
		}

		public ActionResult Edit (int id)
		{
			var item = PriceList.Find (id);

			item.LowProfitMargin *= 100m;
			item.HighProfitMargin *= 100m;

			return PartialView ("_Edit", item);
		}

		[HttpPost]
		public ActionResult Edit (PriceList item)
		{
			if (!ModelState.IsValid)
				return PartialView ("_Edit", item);

			var entity = PriceList.Find (item.Id);

			entity.Name = item.Name;
			entity.LowProfitMargin = item.LowProfitMargin;
			entity.HighProfitMargin = item.HighProfitMargin;

			entity.LowProfitMargin /= 100m;
			entity.HighProfitMargin /= 100m;

			using (var scope = new TransactionScope ()) {
				entity.UpdateAndFlush ();
			}

			return PartialView ("_Refresh");
		}

		public ActionResult Delete (int id)
		{
			var item = PriceList.Find (id);
			return PartialView ("_Delete", item);
		}

		[HttpPost, ActionName ("Delete")]
		public ActionResult DeleteConfirmed (int id)
		{
			var item = PriceList.Find (id);

			try {
				using (var scope = new TransactionScope ()) {
					item.DeleteAndFlush ();
				}

				return PartialView ("_DeleteSuccesful", item);
			} catch (GenericADOException) {
				return PartialView ("DeleteUnsuccessful");
			}
		}

		public JsonResult GetSuggestions (string pattern)
		{
			var qry = from x in PriceList.Queryable
				  where x.Id > 0 && x.Name.Contains (pattern)
				  select new { id = x.Id, name = x.Name };

			return Json (qry.Take (15).ToList (), JsonRequestBehavior.AllowGet);
		}
	}
}
