// 
// InventoryController.cs
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
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Castle.ActiveRecord;
using NHibernate;
using NHibernate.Exceptions;
using Mictlanix.BE.Model;
using Mictlanix.BE.Web.Models;
using Mictlanix.BE.Web.Mvc;
using Mictlanix.BE.Web.Helpers;

namespace Mictlanix.BE.Web.Controllers.Mvc {
	[Authorize]
	public class InventoryController : CustomController {
		#region Receipts

		public ActionResult Receipts ()
		{
			var search = SearchReceipts (new Search<InventoryReceipt> {
				Limit = WebConfig.PageSize
			});

			return View ("Receipts/Index", search);
		}

		[HttpPost]
		public ActionResult Receipts (Search<InventoryReceipt> search)
		{
			if (ModelState.IsValid) {
				search = SearchReceipts (search);
			}

			if (Request.IsAjaxRequest ()) {
				return PartialView ("Receipts/_Index", search);
			}

			return View ("Receipts/Index", search);
		}

		Search<InventoryReceipt> SearchReceipts (Search<InventoryReceipt> search)
		{
			IQueryable<InventoryReceipt> qry;

			if (search.Pattern == null) {
				qry = from x in InventoryReceipt.Queryable
				      orderby x.Id descending
				      select x;
			} else {
				qry = from x in InventoryReceipt.Queryable
				      where x.Warehouse.Name.Contains (search.Pattern)
				      orderby x.Id descending
				      select x;
			}

			search.Total = qry.Count ();
			search.Results = qry.Skip (search.Offset).Take (search.Limit).ToList ();

			return search;
		}

		public ViewResult Receipt (int id)
		{
			var item = InventoryReceipt.Find (id);
			return View ("Receipts/View", item);
		}

		public ViewResult PrintReceipt (int id)
		{
			var item = InventoryReceipt.Find (id);
			return View ("Receipts/Print", item);
		}

		public ActionResult NewReceipt ()
		{
			return PartialView ("Receipts/_Create", new InventoryReceipt ());
		}

		[HttpPost]
		public ActionResult NewReceipt (InventoryReceipt item)
		{
			if (!ModelState.IsValid)
				return PartialView ("Receipts/_Create", item);

			item.CreationTime = DateTime.Now;
			item.ModificationTime = item.CreationTime;
			item.Creator = CurrentUser.Employee;
			item.Updater = item.Creator;
			item.Warehouse = Warehouse.Find (item.WarehouseId);
			item.Store = item.Warehouse.Store;

			using (var scope = new TransactionScope ()) {
				item.CreateAndFlush ();
			}

			return PartialView ("Receipts/_CreateSuccesful", new InventoryReceipt { Id = item.Id });
		}

		public ActionResult EditReceipt (int id)
		{
			var item = InventoryReceipt.Find (id);

			if (item.IsCompleted || item.IsCancelled) {
				return RedirectToAction ("Receipt", new {
					id = item.Id
				});
			}

			if (Request.IsAjaxRequest ())
				return PartialView ("Receipts/_MasterEditView", item);
			else
				return View ("Receipts/Edit", item);
		}

		public ActionResult DiscardReceiptChanges (int id)
		{
			return PartialView ("Receipts/_MasterView", InventoryReceipt.TryFind (id));
		}

		[HttpPost]
		public ActionResult EditReceipt (InventoryReceipt item)
		{
			var movement = InventoryReceipt.Find (item.Id);

			movement.Warehouse = Warehouse.Find (item.WarehouseId);
			movement.Store = movement.Warehouse.Store;
			movement.Updater = CurrentUser.Employee;
			movement.ModificationTime = DateTime.Now;
			movement.Comment = item.Comment;

			using (var scope = new TransactionScope ()) {
				movement.UpdateAndFlush ();
			}

			return PartialView ("Receipts/_MasterView", movement);
		}

		[HttpPost]
		public JsonResult AddReceiptDetail (int movement, int product)
		{
			var p = Product.Find (product);

			var item = new InventoryReceiptDetail {
				Receipt = InventoryReceipt.Find (movement),
				Product = p,
				ProductCode = p.Code,
				ProductName = p.Name,
				Quantity = 1,
				QuantityOrdered = 0
			};

			using (var scope = new TransactionScope ()) {
				item.CreateAndFlush ();
			}

			return Json (new {
				id = item.Id
			});
		}

		[HttpPost]
		public JsonResult EditReceiptDetailQuantity (int id, decimal value)
		{
			var detail = InventoryReceiptDetail.Find (id);

			if (value >= 0) {
				detail.Quantity = value;

				using (var scope = new TransactionScope ()) {
					detail.UpdateAndFlush ();
				}
			}

			return Json (new {
				id = id,
				value = detail.Quantity
			});
		}

		public ActionResult GetReceiptItem (int id)
		{
			return PartialView ("Receipts/_DetailEditView", InventoryReceiptDetail.Find (id));
		}

        public ActionResult GetTotalQuantityReceipt(int id) {

            return PartialView("Receipts/_TotalQuantity", InventoryReceipt.Find(id));
        }

		[HttpPost]
		public JsonResult RemoveReceiptDetail (int id)
		{
			var item = InventoryReceiptDetail.Find (id);

			using (var scope = new TransactionScope ()) {
				item.DeleteAndFlush ();
			}

			return Json (new {
				id = id,
				result = true
			});
		}

		[HttpPost]
		public ActionResult ConfirmReceipt (int id)
		{
			var item = InventoryReceipt.TryFind (id);

			if (item == null || item.IsCompleted || item.IsCancelled)
				return RedirectToAction ("Receipts");

			item.Store = item.Warehouse.Store;

			try {
				item.Serial = (from x in InventoryReceipt.Queryable
					       where x.Store.Id == item.Store.Id
					       select x.Serial).Max () + 1;
			} catch {
				item.Serial = 1;
			}

			item.IsCompleted = true;
			item.ModificationTime = DateTime.Now;

			using (var scope = new TransactionScope ()) {
				item.UpdateAndFlush ();

				foreach (var x in item.Details) {
					InventoryHelpers.ChangeNotification (TransactionType.InventoryReceipt, item.Id,
									     item.ModificationTime, item.Warehouse, null, x.Product, x.Quantity);
				}
			}

			return RedirectToAction ("Receipts");
		}

		[HttpPost]
		public ActionResult CancelReceipt (int id)
		{
			var item = InventoryReceipt.Find (id);

			item.IsCancelled = true;

			using (var scope = new TransactionScope ()) {
				item.UpdateAndFlush ();
			}

			return RedirectToAction ("Receipts");
		}

		#endregion

		#region Issues

		public ActionResult Issues ()
		{
			var search = SearchIssues (new Search<InventoryIssue> {
				Limit = WebConfig.PageSize
			});

			return View ("Issues/Index", search);
		}

		[HttpPost]
		public ActionResult Issues (Search<InventoryIssue> search)
		{
			if (ModelState.IsValid) {
				search = SearchIssues (search);
			}

			if (Request.IsAjaxRequest ()) {
				return PartialView ("Issues/_Index", search);
			}

			return View ("Issues/Index", search);
		}

		Search<InventoryIssue> SearchIssues (Search<InventoryIssue> search)
		{
			IQueryable<InventoryIssue> qry;

			if (search.Pattern == null) {
				qry = from x in InventoryIssue.Queryable
				      orderby x.Id descending
				      select x;
			} else {
				qry = from x in InventoryIssue.Queryable
				      where x.Warehouse.Name.Contains (search.Pattern)
				      orderby x.Id descending
				      select x;
			}

			search.Total = qry.Count ();
			search.Results = qry.Skip (search.Offset).Take (search.Limit).ToList ();

			return search;
		}

		public ViewResult Issue (int id)
		{
			var item = InventoryIssue.Find (id);
			return View ("Issues/View", item);
		}

		public ViewResult PrintIssue (int id)
		{
			var item = InventoryIssue.Find (id);
			return View ("Issues/Print", item);
		}

		public ActionResult NewIssue ()
		{
			return PartialView ("Issues/_Create", new InventoryIssue ());
		}

		[HttpPost]
		public ActionResult NewIssue (InventoryIssue item)
		{
			if (!ModelState.IsValid)
				return PartialView ("Issues/_Create", item);

			item.CreationTime = DateTime.Now;
			item.ModificationTime = item.CreationTime;
			item.Creator = CurrentUser.Employee;
			item.Updater = item.Creator;
			item.Warehouse = Warehouse.Find (item.WarehouseId);
			item.Store = item.Warehouse.Store;

			using (var scope = new TransactionScope ()) {
				item.CreateAndFlush ();
			}

			return PartialView ("Issues/_CreateSuccesful", new InventoryIssue { Id = item.Id });
		}

		public ActionResult EditIssue (int id)
		{
			var item = InventoryIssue.Find (id);

			if (item.IsCompleted || item.IsCancelled) {
				return RedirectToAction ("Issue", new {
					id = item.Id
				});
			}

			if (Request.IsAjaxRequest ())
				return PartialView ("Issues/_MasterEditView", item);
			else
				return View ("Issues/Edit", item);
		}

        public ActionResult GetTotalQuantityIssue(int id)
        {

            return PartialView("Issues/_TotalQuantity", InventoryIssue.Find(id));
        }

        public ActionResult DiscardIssueChanges (int id)
		{
			return PartialView ("Issues/_MasterView", InventoryIssue.TryFind (id));
		}

		[HttpPost]
		public ActionResult EditIssue (InventoryIssue item)
		{
			var movement = InventoryIssue.Find (item.Id);

			movement.Warehouse = Warehouse.Find (item.WarehouseId);
			movement.Store = movement.Warehouse.Store;
			movement.Updater = CurrentUser.Employee;
			movement.ModificationTime = DateTime.Now;
			movement.Comment = item.Comment;

			using (var scope = new TransactionScope ()) {
				movement.UpdateAndFlush ();
			}

			return PartialView ("Issues/_MasterView", movement);
		}

		[HttpPost]
		public JsonResult AddIssueDetail (int movement, int product)
		{
			var p = Product.Find (product);

			var item = new InventoryIssueDetail {
				Issue = InventoryIssue.Find (movement),
				Product = p,
				ProductCode = p.Code,
				ProductName = p.Name,
				Quantity = 1
			};

			using (var scope = new TransactionScope ()) {
				item.CreateAndFlush ();
			}

			return Json (new {
				id = item.Id
			});
		}

		[HttpPost]
		public JsonResult EditIssueDetailQuantity (int id, decimal value)
		{
			var detail = InventoryIssueDetail.Find (id);

			if (value >= 0) {
				detail.Quantity = value;

				using (var scope = new TransactionScope ()) {
					detail.UpdateAndFlush ();
				}
			}

			return Json (new {
				id = id,
				value = detail.Quantity
			});
		}

		public ActionResult GetIssueItem (int id)
		{
			return PartialView ("Issues/_DetailEditView", InventoryIssueDetail.Find (id));
		}

		[HttpPost]
		public JsonResult RemoveIssueDetail (int id)
		{
			var item = InventoryIssueDetail.Find (id);

			using (var scope = new TransactionScope ()) {
				item.DeleteAndFlush ();
			}

			return Json (new {
				id = id,
				result = true
			});
		}

		[HttpPost]
		public ActionResult ConfirmIssue (int id)
		{
			var item = InventoryIssue.TryFind (id);

			if (item == null || item.IsCompleted || item.IsCancelled)
				return RedirectToAction ("Issues");

			item.Store = item.Warehouse.Store;

			try {
				item.Serial = (from x in InventoryIssue.Queryable
					       where x.Store.Id == item.Store.Id
					       select x.Serial).Max () + 1;
			} catch {
				item.Serial = 1;
			}

			item.IsCompleted = true;
			item.ModificationTime = DateTime.Now;

			using (var scope = new TransactionScope ()) {
				item.UpdateAndFlush ();

				foreach (var x in item.Details) {
					InventoryHelpers.ChangeNotification (TransactionType.InventoryIssue, item.Id,
									     item.ModificationTime, item.Warehouse, null, x.Product, -x.Quantity);
				}
			}

			return RedirectToAction ("Issues");
		}

		[HttpPost]
		public ActionResult CancelIssue (int id)
		{
			var item = InventoryIssue.Find (id);

			item.IsCancelled = true;

			using (var scope = new TransactionScope ()) {
				item.UpdateAndFlush ();
			}

			return RedirectToAction ("Issues");
		}

		#endregion

		#region Transfers

		public ActionResult Transfers ()
		{
			var search = SearchTransfers (new Search<InventoryTransfer> {
				Limit = WebConfig.PageSize
			});

			return View ("Transfers/Index", search);
		}

		[HttpPost]
		public ActionResult Transfers (Search<InventoryTransfer> search)
		{
			if (ModelState.IsValid) {
				search = SearchTransfers (search);
			}

			if (Request.IsAjaxRequest ()) {
				return PartialView ("Transfers/_Index", search);
			}

			return View ("Transfers/Index", search);
		}

		Search<InventoryTransfer> SearchTransfers (Search<InventoryTransfer> search)
		{
			IQueryable<InventoryTransfer> qry;

			if (search.Pattern == null) {
				qry = from x in InventoryTransfer.Queryable
				      orderby x.Id descending
				      select x;
			} else {
				qry = from x in InventoryTransfer.Queryable
				      where x.To.Name.Contains (search.Pattern) ||
						    x.From.Name.Contains (search.Pattern)
				      orderby x.Id descending
				      select x;
			}

			search.Total = qry.Count ();
			search.Results = qry.Skip (search.Offset).Take (search.Limit).ToList ();

			return search;
		}

		public ViewResult Transfer (int id)
		{
			var item = InventoryTransfer.Find (id);
			return View ("Transfers/View", item);
		}

		public ViewResult PrintTransfer (int id)
		{
			var item = InventoryTransfer.Find (id);
			return View ("Transfers/Print", item);
		}

		public ActionResult NewTransfer ()
		{
			return PartialView ("Transfers/_Create", new InventoryTransfer ());
		}

		[HttpPost]
		public ActionResult NewTransfer (InventoryTransfer item)
		{
			if (!ModelState.IsValid) {
				return PartialView ("Transfers/_Create", item);
			}

			item.From = Warehouse.TryFind (item.FromId);
			item.To = Warehouse.TryFind (item.ToId);
			item.Store = item.From.Store;
			item.CreationTime = DateTime.Now;
			item.ModificationTime = item.CreationTime;
			item.Creator = CurrentUser.Employee;
			item.Updater = item.Creator;

			using (var scope = new TransactionScope ()) {
				item.CreateAndFlush ();
			}

			return PartialView ("Transfers/_CreateSuccesful", new InventoryTransfer { Id = item.Id });
		}

		public ActionResult EditTransfer (int id)
		{
			var item = InventoryTransfer.Find (id);

			if (item.IsCompleted || item.IsCancelled) {
				return RedirectToAction ("Transfer", new {
					id = item.Id
				});
			}

			if (Request.IsAjaxRequest ())
				return PartialView ("Transfers/_MasterEditView", item);
			else
				return View ("Transfers/Edit", item);
		}

        public ActionResult GetTotalQuantityTransfer(int id)
        {

            return PartialView("Transfers/_TotalQuantity", InventoryTransfer.Find(id));
        }

        public ActionResult DiscardTransferChanges (int id)
		{
			return PartialView ("Transfers/_MasterView", InventoryTransfer.TryFind (id));
		}

		[HttpPost]
		public ActionResult EditTransfer (InventoryTransfer item)
		{
			item.From = Warehouse.TryFind (item.FromId);
			item.To = Warehouse.TryFind (item.ToId);

			if (!ModelState.IsValid) {
				if (Request.IsAjaxRequest ()) {
					return PartialView ("Transfers/_MasterEditView", item);
				}

				return View (item);
			}

			var movement = InventoryTransfer.Find (item.Id);

			movement.From = item.From;
			movement.To = item.To;
			movement.Store = movement.From.Store;
			movement.Updater = CurrentUser.Employee;
			movement.ModificationTime = DateTime.Now;
			movement.Comment = item.Comment;

			using (var scope = new TransactionScope ()) {
				movement.UpdateAndFlush ();
			}

			return PartialView ("Transfers/_MasterView", movement);
		}

		[HttpPost]
		public JsonResult AddTransferDetail (int movement, int product)
		{
			var p = Product.Find (product);

			var item = new InventoryTransferDetail {
				Transfer = InventoryTransfer.Find (movement),
				Product = p,
				ProductCode = p.Code,
				ProductName = p.Name,
				Quantity = 1
			};

			using (var scope = new TransactionScope ()) {
				item.CreateAndFlush ();
			}

			return Json (new {
				id = item.Id
			});
		}

		[HttpPost]
		public JsonResult EditTransferDetailQuantity (int id, decimal value)
		{
			var detail = InventoryTransferDetail.Find (id);

			if (value >= 0) {
				detail.Quantity = value;

				using (var scope = new TransactionScope ()) {
					detail.UpdateAndFlush ();
				}
			}

			return Json (new {
				id = id,
				value = detail.Quantity
			});
		}

		public ActionResult GetTransferItem (int id)
		{
			return PartialView ("Transfers/_DetailEditView", InventoryTransferDetail.Find (id));
		}

		[HttpPost]
		public JsonResult RemoveTransferDetail (int id)
		{
			var item = InventoryTransferDetail.Find (id);

			using (var scope = new TransactionScope ()) {
				item.DeleteAndFlush ();
			}

			return Json (new {
				id = id,
				result = true
			});
		}

		[HttpPost]
		public ActionResult ConfirmTransfer (int id)
		{
			var item = InventoryTransfer.TryFind (id);

			if (item == null || item.IsCompleted || item.IsCancelled)
				return RedirectToAction ("Transfers");

			item.Store = item.From.Store;

			try {
				item.Serial = (from x in InventoryTransfer.Queryable
					       where x.Store.Id == item.Store.Id
					       select x.Serial).Max () + 1;
			} catch {
				item.Serial = 1;
			}

			item.IsCompleted = true;
			item.ModificationTime = DateTime.Now;

			using (var scope = new TransactionScope ()) {
				item.UpdateAndFlush ();

				foreach (var x in item.Details) {
					InventoryHelpers.ChangeNotification (TransactionType.InventoryTransfer, item.Id,
						item.ModificationTime, item.From, item.To, x.Product, -x.Quantity);
				}
			}

			return RedirectToAction ("Transfers");
		}

		[HttpPost]
		public ActionResult CancelTransfer (int id)
		{
			var item = InventoryTransfer.Find (id);

			item.IsCancelled = true;

			using (var scope = new TransactionScope ()) {
				item.UpdateAndFlush ();
			}

			return RedirectToAction ("Transfers");
		}

		[HttpPost]
		public ActionResult AddTransferItems (int id, string value)
		{
			var entity = InventoryTransfer.Find (id);
			SalesOrder sales_order = null;
			int sales_order_id = 0;
			int count = 0;

			if (entity.IsCompleted || entity.IsCancelled) {
				Response.StatusCode = 400;
				return Content (Resources.ItemAlreadyCompletedOrCancelled);
			}

			if (int.TryParse (value, out sales_order_id)) {
				sales_order = SalesOrder.TryFind (sales_order_id);
			}

			if (sales_order == null) {
				Response.StatusCode = 400;
				return Content (Resources.SalesOrderNotFound);
			}

			using (var scope = new TransactionScope ()) {
				foreach (var x in sales_order.Details) {
					if (!x.Product.IsStockable)
						continue;

					var item = new InventoryTransferDetail {
						Transfer = entity,
						Product = x.Product,
						ProductCode = x.ProductCode,
						ProductName = x.ProductName,
						Quantity = x.Quantity,
					};

					item.Create ();
					count++;
				}
			}

			return Json (new {
				id = id,
				value = string.Empty,
				itemsChanged = count
			});
		}

		#endregion

		#region Lot & Serial Numbers

		public ActionResult LotSerialNumbers ()
		{
			var search = SearchLotSerialNumbers (new Search<LotSerialRequirement> {
				Limit = WebConfig.PageSize
			});

			return View (search);
		}

		[HttpPost]
		public ActionResult LotSerialNumbers (Search<LotSerialRequirement> search)
		{
			if (ModelState.IsValid) {
				search = SearchLotSerialNumbers (search);
			}

			if (Request.IsAjaxRequest ()) {
				return PartialView ("_LotSerialNumbers", search);
			}

			return View (search);
		}

		Search<LotSerialRequirement> SearchLotSerialNumbers (Search<LotSerialRequirement> search)
		{
			var query = from x in LotSerialRequirement.Queryable
				    select new {
					    Source = x.Source,
					    Reference = x.Reference,
					    Warehouse = new Warehouse { Id = x.Warehouse.Id, Name = x.Warehouse.Name },
					    Quantity = x.Quantity
				    };
			var items = from x in query.ToList ()
				    group x by new {
					    x.Source,
					    x.Reference
				    } into g
				    select new LotSerialRequirement {
					    Source = g.Key.Source,
					    Reference = g.Key.Reference,
					    Warehouse = g.Select (y => y.Warehouse).First (),
					    Quantity = g.Sum (y => y.Quantity)
				    };

			search.Total = items.Count ();
			search.Results = items.Skip (search.Offset).Take (search.Limit).ToList ();

			return search;
		}

		public JsonResult DiscardLotSerialNumbers (int id)
		{
			var rqmt = LotSerialRequirement.Find (id);
			var qry = from x in LotSerialTracking.Queryable
				  where x.Source == rqmt.Source &&
						x.Reference == rqmt.Reference &&
						x.Warehouse.Id == rqmt.Warehouse.Id &&
						x.Product == rqmt.Product
				  select x;

			using (var scope = new TransactionScope ()) {
				var entity = new LotSerialTracking {
					Source = rqmt.Source,
					Reference = rqmt.Reference,
					Date = DateTime.Now,
					Warehouse = rqmt.Warehouse,
					Product = rqmt.Product,
					Quantity = rqmt.Quantity
				};

				foreach (var item in qry) {
					item.Delete ();
				}

				entity.Create ();
				rqmt.DeleteAndFlush ();
			}

			return Json (new {
				id = id,
				result = true
			}, JsonRequestBehavior.AllowGet);
		}

		public ActionResult AssignLotSerialNumbers (TransactionType source, int reference)
		{
			var rqmts = from x in LotSerialRequirement.Queryable
				    where x.Source == source &&
					    x.Reference == reference
				    select x;
			var items = new List<MasterDetails<LotSerialRequirement, LotSerialTracking>> ();

			foreach (var rqmt in rqmts) {
				var query = from x in LotSerialTracking.Queryable
					    where x.Source == source &&
						    x.Reference == reference &&
						    x.Warehouse.Id == rqmt.Warehouse.Id &&
						    x.Product.Id == rqmt.Product.Id
					    select x;

				items.Add (new MasterDetails<LotSerialRequirement, LotSerialTracking> {
					Master = rqmt,
					Details = query.ToList ()
				});
			}

			/*
			switch (source) {
			case TransactionType.SalesOrder:
			case TransactionType.InventoryIssue:
			case TransactionType.InventoryTransfer:
			case TransactionType.CustomerRefund:
				return View ("AssignExistingLotSerialNumbers", items);
			default:
				return View (items);
			}
			*/

			return View (items);
		}

		[HttpPost]
		public ActionResult ConfirmLotSerialNumbers (int id)
		{
			var entity = LotSerialRequirement.Find (id);
			var qry = from x in LotSerialTracking.Queryable
				  where x.Source == entity.Source &&
						x.Reference == entity.Reference &&
						x.Warehouse.Id == entity.Warehouse.Id &&
						x.Product.Id == entity.Product.Id
				  select x;
			decimal sum = qry.Count () > 0 ? qry.Sum (x => x.Quantity) : 0;

			if (entity.Quantity != sum) {
				Response.StatusCode = 400;
				return Content (Resources.ValidationFailed);
			}

			using (var scope = new TransactionScope ()) {
				if (entity.Source == TransactionType.InventoryTransfer) {
					var transfer = InventoryTransfer.Find (entity.Reference);

					foreach (var serial in qry) {
						var item = new LotSerialTracking {
							Source = serial.Source,
							Reference = serial.Reference,
							Date = serial.Date,
							Warehouse = transfer.To,
							Product = serial.Product,
							Quantity = -serial.Quantity,
							LotNumber = serial.LotNumber,
							ExpirationDate = serial.ExpirationDate,
							SerialNumber = serial.SerialNumber
						};

						item.Create ();
					}
				}

				entity.DeleteAndFlush ();
			}

			return Json (new {
				id = id,
				result = true
			});
		}

		public ActionResult GetLotSerialNumber (int id)
		{
			var item = LotSerialTracking.Find (id);

			return PartialView ("_LotSerialNumber", item);
		}

		public JsonResult GetLotSerialNumberCount (int id)
		{
			var item = LotSerialRequirement.Find (id);
			var qry = from x in LotSerialTracking.Queryable
				  where x.Source == item.Source &&
						x.Reference == item.Reference &&
						x.Warehouse.Id == item.Warehouse.Id &&
						x.Product.Id == item.Product.Id
				  select x.Quantity;
			decimal sum = qry.Count () > 0 ? qry.Sum () : 0;

			return Json (new {
				id = item.Id,
				count = sum,
				total = item.Quantity
			}, JsonRequestBehavior.AllowGet);
		}

		LotSerialTracking GetLastLotSerial (int product, string lot, string serial)
		{
			IQueryable<LotSerialTracking> query;

			if (!string.IsNullOrWhiteSpace (serial)) {
				query = from x in LotSerialTracking.Queryable
					where x.Product.Id == product &&
					    x.SerialNumber == serial
					orderby x.Date descending
					select x;
			} else {
				query = from x in LotSerialTracking.Queryable
					where x.Product.Id == product &&
						x.LotNumber == lot
					orderby x.Date descending
					select x;
			}

			return query.FirstOrDefault ();
		}

		[HttpPost]
		public JsonResult AddLotSerialNumber (int id, decimal qty, string lot, DateTime? expiration, string serial)
		{
			var rqmt = LotSerialRequirement.Find (id);
			var item = GetLastLotSerial (rqmt.Product.Id, lot, serial);

			if (item == null) {
				item = new LotSerialTracking {
					LotNumber = lot,
					ExpirationDate = expiration,
					SerialNumber = string.IsNullOrWhiteSpace (serial) ? null : serial
				};
			} else {
				item = new LotSerialTracking {
					LotNumber = item.LotNumber,
					ExpirationDate = item.ExpirationDate,
					SerialNumber = string.IsNullOrWhiteSpace (serial) ? null : item.SerialNumber
				};
			}

			item.Source = rqmt.Source;
			item.Reference = rqmt.Reference;
			item.Date = DateTime.Now;
			item.Warehouse = rqmt.Warehouse;
			item.Product = rqmt.Product;
			item.Quantity = (rqmt.Quantity > 0 ? qty : -qty);

			using (var scope = new TransactionScope ()) {
				item.CreateAndFlush ();
			}

			return Json (new {
				id = item.Id,
				result = true
			});
		}

		[HttpPost]
		public JsonResult RemoveLotSerialNumber (int id)
		{
			var item = LotSerialTracking.Find (id);

			using (var scope = new TransactionScope ()) {
				item.DeleteAndFlush ();
			}

			return Json (new {
				id = id,
				result = true
			});
		}

		[HttpPost]
		public JsonResult SetWarehouse (TransactionType source, int reference, int value)
		{
			var warehouse = Warehouse.Find (value);
			var rqmts = from x in LotSerialRequirement.Queryable
				    where x.Source == source &&
					    x.Reference == reference
				    select x;
			var serials = from x in LotSerialTracking.Queryable
				      where x.Source == source &&
					      x.Reference == reference
				      select x;

			using (var scope = new TransactionScope ()) {
				foreach (var item in rqmts) {
					item.Warehouse = warehouse;
					item.Update ();
				}
				foreach (var item in serials) {
					item.Warehouse = warehouse;
					item.Update ();
				}
			}

			return Json (new {
				id = warehouse.Id,
				value = warehouse.Name
			});
		}


		#endregion

		#region Inventory

		public ActionResult PhysicalCountAdjustment (int id)
		{
			var entity = InventoryReceipt.Find (id);
			string sql = @"SELECT d.product_code Code, d.product_name Name, s.lot_number LotNumber, s.expiration_date ExpirationDate, s.serial_number SerialNumber, SUM(s.quantity) Quantity
                            FROM lot_serial_tracking s
                            INNER JOIN inventory_receipt_detail d ON s.product = d.product
                            WHERE d.receipt = :id AND s.warehouse = :warehouse AND s.date < :date
                            GROUP BY s.product, s.lot_number, s.expiration_date, s.serial_number
                            HAVING SUM(s.quantity) <> 0";

			var items = (IList<dynamic>) ActiveRecordMediator<Product>.Execute (delegate (ISession session, object instance) {
				var query = session.CreateSQLQuery (sql);

				query.AddScalar ("Code", NHibernateUtil.String);
				query.AddScalar ("Name", NHibernateUtil.String);
				query.AddScalar ("LotNumber", NHibernateUtil.String);
				query.AddScalar ("ExpirationDate", NHibernateUtil.Date);
				query.AddScalar ("SerialNumber", NHibernateUtil.String);
				query.AddScalar ("Quantity", NHibernateUtil.Decimal);

				query.SetInt32 ("id", entity.Id);
				query.SetInt32 ("warehouse", entity.Warehouse.Id);
				query.SetDateTime ("date", entity.ModificationTime);

				return query.DynamicList ();
			}, null);

			return View ("PhysicalCountAdjustment", items);
		}

		[HttpPost, ActionName ("PhysicalCountAdjustment")]
		public ActionResult PhysicalCountAdjustmentConfirmed (int id)
		{
			var entity = InventoryReceipt.Find (id);
			string sql = @"SELECT s.product ProductId, s.lot_number LotNumber, s.expiration_date ExpirationDate, s.serial_number SerialNumber, SUM(s.quantity) Quantity
                            FROM lot_serial_tracking s
                            INNER JOIN inventory_receipt_detail d ON s.product = d.product
                            WHERE d.receipt = :id AND s.warehouse = :warehouse AND s.date < :date
                            GROUP BY s.product, s.lot_number, s.expiration_date, s.serial_number
                            HAVING SUM(s.quantity) <> 0";

			var items = (IList<dynamic>) ActiveRecordMediator<Product>.Execute (delegate (ISession session, object instance) {
				var query = session.CreateSQLQuery (sql);

				query.AddScalar ("ProductId", NHibernateUtil.Int32);
				query.AddScalar ("LotNumber", NHibernateUtil.String);
				query.AddScalar ("ExpirationDate", NHibernateUtil.Date);
				query.AddScalar ("SerialNumber", NHibernateUtil.String);
				query.AddScalar ("Quantity", NHibernateUtil.Decimal);

				query.SetInt32 ("id", entity.Id);
				query.SetInt32 ("warehouse", entity.Warehouse.Id);
				query.SetDateTime ("date", entity.ModificationTime);

				return query.DynamicList ();
			}, null);

			using (var scope = new TransactionScope ()) {
				var dt = entity.ModificationTime.AddMilliseconds (-1);

				foreach (var x in items) {
					var item = new LotSerialTracking {
						Source = TransactionType.InventoryAdjustment,
						Reference = entity.Id,
						Date = dt,
						Warehouse = entity.Warehouse,
						Product = Product.Find (x.ProductId),
						Quantity = -x.Quantity,
						LotNumber = x.LotNumber,
						ExpirationDate = x.ExpirationDate,
						SerialNumber = x.SerialNumber
					};

					item.Create ();
				}

				scope.Flush ();
			}

			return RedirectToAction ("Receipts");
		}

		public ActionResult ZeroOut (int id)
		{
			var entity = InventoryReceipt.Find (id);
			string sql = @"SELECT p.code Code, p.name Name, s.lot_number LotNumber, s.expiration_date ExpirationDate, s.serial_number SerialNumber, SUM(s.quantity) Quantity
                            FROM lot_serial_tracking s
                            INNER JOIN product p ON s.product = p.product_id
                            WHERE s.warehouse = :warehouse AND s.date < :date
                            GROUP BY s.product, s.lot_number, s.expiration_date, s.serial_number
                            HAVING SUM(s.quantity) <> 0";

			var items = (IList<dynamic>)ActiveRecordMediator<Product>.Execute (delegate (ISession session, object instance) {
				var query = session.CreateSQLQuery (sql);

				query.AddScalar ("Code", NHibernateUtil.String);
				query.AddScalar ("Name", NHibernateUtil.String);
				query.AddScalar ("LotNumber", NHibernateUtil.String);
				query.AddScalar ("ExpirationDate", NHibernateUtil.Date);
				query.AddScalar ("SerialNumber", NHibernateUtil.String);
				query.AddScalar ("Quantity", NHibernateUtil.Decimal);

				query.SetInt32 ("warehouse", entity.Warehouse.Id);
				query.SetDateTime ("date", entity.ModificationTime);

				return query.DynamicList ();
			}, null);

			return View ("PhysicalCountAdjustment", items);
		}

		[HttpPost, ActionName ("ZeroOut")]
		public ActionResult ZeroOutConfirmed (int id)
		{
			var entity = InventoryReceipt.Find (id);
			string sql = @"SELECT s.product ProductId, s.lot_number LotNumber, s.expiration_date ExpirationDate, s.serial_number SerialNumber, SUM(s.quantity) Quantity
                            FROM lot_serial_tracking s
                            WHERE s.warehouse = :warehouse AND s.date < :date
                            GROUP BY s.product, s.lot_number, s.expiration_date, s.serial_number
                            HAVING SUM(s.quantity) <> 0";

			var items = (IList<dynamic>)ActiveRecordMediator<Product>.Execute (delegate (ISession session, object instance) {
				var query = session.CreateSQLQuery (sql);

				query.AddScalar ("ProductId", NHibernateUtil.Int32);
				query.AddScalar ("LotNumber", NHibernateUtil.String);
				query.AddScalar ("ExpirationDate", NHibernateUtil.Date);
				query.AddScalar ("SerialNumber", NHibernateUtil.String);
				query.AddScalar ("Quantity", NHibernateUtil.Decimal);

				query.SetInt32 ("warehouse", entity.Warehouse.Id);
				query.SetDateTime ("date", entity.ModificationTime);

				return query.DynamicList ();
			}, null);

			using (var scope = new TransactionScope ()) {
				var dt = entity.ModificationTime.AddMilliseconds (-1);

				foreach (var x in items) {
					var item = new LotSerialTracking {
						Source = TransactionType.InventoryAdjustment,
						Reference = entity.Id,
						Date = dt,
						Warehouse = entity.Warehouse,
						Product = Product.Find (x.ProductId),
						Quantity = -x.Quantity,
						LotNumber = x.LotNumber,
						ExpirationDate = x.ExpirationDate,
						SerialNumber = x.SerialNumber
					};

					item.Create ();
				}

				scope.Flush ();
			}

			return RedirectToAction ("Receipts");
		}

		#endregion


	}
}
