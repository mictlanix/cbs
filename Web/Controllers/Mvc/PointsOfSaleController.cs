﻿// 
// PointsOfSaleController.cs
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

namespace Mictlanix.BE.Web.Controllers.Mvc {
	[Authorize]
	public class PointsOfSaleController : CustomController {
		//
		// GET: /PointSale/

		public ViewResult Index ()
		{
			var qry = from x in PointOfSale.Queryable
				  orderby x.Name
				  select x;

			Search<PointOfSale> search = new Search<PointOfSale> ();
			search.Limit = WebConfig.PageSize;
			search.Results = qry.Skip (search.Offset).Take (search.Limit).ToList ();
			search.Total = qry.Count ();

			return View (search);
		}

		// POST: /PointsOfSale/

		[HttpPost]
		public ActionResult Index (Search<PointOfSale> search)
		{
			if (ModelState.IsValid) {
				search = GetPointsOfSale (search);
			}

			if (Request.IsAjaxRequest ()) {
				return PartialView ("_Index", search);
			} else {
				return View (search);
			}
		}

		Search<PointOfSale> GetPointsOfSale (Search<PointOfSale> search)
		{
			if (search.Pattern == null) {
				var qry = from x in PointOfSale.Queryable
					  orderby x.Name
					  select x;

				search.Total = qry.Count ();
				search.Results = qry.Skip (search.Offset).Take (search.Limit).ToList ();
			} else {
				var qry = from x in PointOfSale.Queryable
					  where x.Name.Contains (search.Pattern) ||
					  x.Code.Contains (search.Pattern) ||
					  x.Store.Name.Contains (search.Pattern)
					  orderby x.Name
					  select x;

				search.Total = qry.Count ();
				search.Results = qry.Skip (search.Offset).Take (search.Limit).ToList ();
			}

			return search;
		}

		//
		// GET: /PointSale/Details/5

		public ActionResult View (int id)
		{
			var item = PointOfSale.Find (id);
			return PartialView ("_View", item);
		}

		//
		// GET: /PointSale/Create

		public ActionResult Create ()
		{
			return PartialView ("_Create");
		}

		//
		// POST: /PointSale/Create

		[HttpPost]
		public ActionResult Create (PointOfSale item)
		{
			if (!ModelState.IsValid)
				return PartialView ("_Create", item);

			item.Store = Store.Find (item.StoreId);
			item.Warehouse = Warehouse.Find (item.WarehouseId);

			using (var scope = new TransactionScope ()) {
				item.CreateAndFlush ();
			}

			return PartialView ("_CreateSuccesful", item);
		}

		//
		// GET: /Warehouses/Edit/5

		public ActionResult Edit (int id)
		{
			var item = PointOfSale.Find (id);
			return PartialView ("_Edit", item);
		}

		//
		// POST: /PointSale/Edit/5

		[HttpPost]
		public ActionResult Edit (PointOfSale item)
		{
			item.Store = Store.TryFind (item.StoreId);
			item.Warehouse = Warehouse.TryFind (item.WarehouseId);

			if (!ModelState.IsValid)
				return PartialView ("_Edit", item);

			var entity = PointOfSale.Find (item.Id);

			entity.Code = item.Code;
			entity.Name = item.Name;
			entity.Comment = item.Comment;
			entity.Store = item.Store;
			entity.Warehouse = item.Warehouse;

			using (var scope = new TransactionScope ()) {
				entity.UpdateAndFlush ();
			}

			return PartialView ("_Refresh");
		}

		//
		// GET: /PointSale/Delete/5

		public ActionResult Delete (int id)
		{
			return PartialView ("_Delete", PointOfSale.Find (id));
		}

		//
		// POST: /Warehouses/Delete/5

		[HttpPost, ActionName ("Delete")]
		public ActionResult DeleteConfirmed (int id)
		{
			var item = PointOfSale.Find (id);
			//PointOfSale item = PointOfSale.Find (id);
			try {
				using (var scope = new TransactionScope ()) {
					item.DeleteAndFlush ();
				}
				return PartialView ("_DeleteSuccesful", item);
			} catch (Exception) {
				return PartialView ("DeleteUnsuccessful");
			}
		}

		public JsonResult GetSuggestions (int store, string pattern)
		{
			var qry = from x in PointOfSale.Queryable
				  where x.Store.Id == store &&
								    (x.Code.Contains (pattern) ||
								      x.Name.Contains (pattern))
				  select new { id = x.Id, name = x.Name };

			return Json (qry.Take (15).ToList (), JsonRequestBehavior.AllowGet);
		}
	}
}