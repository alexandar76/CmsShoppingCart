using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //Initial the cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Check if cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty";
                return View();
            }

            //Calculate total and save in Viewbag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            //Return view with list
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //Initial CartVM
            CartVM model = new CartVM();

            //Initial quantity
            int qty = 0;

            //INitial price
            decimal price = 0m;

            //Check for cart session
            if (Session["cart"] != null)
            {
                //Get total quantity and price
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                //Or set quantity and price to 0
                model.Quantity = 0;
                model.Price = 0m;
            }

            //Return partial vview with model
            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //Initial CartVM list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Initial CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                //Get the product
                ProductDTO product = db.Products.Find(id);

                //Check if product is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //If not add new
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    //If it is, increment
                    productInCart.Quantity++;
                }
            }

            //Get total quantity and price and add to the model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //Save cart back to session
            Session["cart"] = cart;

            //Return partial view with model
            return PartialView(model);
        }

        // GET: /Cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            //Initial cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Increment quantity
                model.Quantity++;

                //Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                //Return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Cart/DecrementProduct
        public JsonResult DecrementProduct(int productId)
        {
            //Initial cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get model from the list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Decrement quantity
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                //Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                //Return json
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Cart/RemoveProduct
        public void RemoveProduct(int productId)
        {
            //Initial cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get model from the list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Remove model from list
                cart.Remove(model);

            }
        }

        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }

        //POST  /Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //Get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //Get username
            string username = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                //Initial OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                //Get user id
                var q = db.Users.FirstOrDefault(x => x.Username == username);
                int userId = q.Id;

                //Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                //Get inserted id
                orderId = orderDTO.OrderId;

                //Initial OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                //Add to OrderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }
            }

            // Email admin
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("18e611371a200c", "d8755ae60604c7"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "New Order", "You have a new order. Order number " + orderId);

            //Reset session
            Session["cart"] = null;
        }
    }
}