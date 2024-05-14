using PayPal.Api;
using ShoppingOnline.Models.Business;
using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace ShoppingOnline.Controllers
{
    public class CartController : Controller
    {

        private const string CartSession = "CartSession";
        // GET: Cart
        public ActionResult Index()
        {
            //hiển thị giỏ        
            var cart = Session[CartSession];
            var list = new List<CartDTO>();

            if (cart != null)
            {
                list = (List<CartDTO>)cart;
            }
            return View(list);
        }


        public JsonResult AddCart(long product_ID, int quantity)
        {
            var product = new ProductBusiness().findID(product_ID);
            var cart = Session[CartSession];
            if (cart != null)//Nếu giỏ đã chứa sản phẩm
            {
                var list = (List<CartDTO>)cart;
                if (list.Exists(x => x.Product.ID == product_ID))//nếu giỏ đã chứa sản phẩm có ID = BookID
                {
                    foreach (var item in list)
                    {
                        if (item.Product.ID == product_ID)
                        {
                            item.Quantity += quantity;
                        }
                    }
                }
                else
                {
                    //Tạo đối tượng mới
                    var item = new CartDTO();
                    item.Product = product;
                    item.Quantity = quantity;
                    item.actual_number = (int)product.Quantity;
                    item.Seller_ID = product.User_ID;
                    list.Add(item);
                }
            }
            else//nếu giỏ hàng trống
            {
                var item = new CartDTO();
                item.Product = product;
                item.Quantity = quantity;
                item.actual_number = (int)product.Quantity;
                item.Seller_ID = product.User_ID;
                var list = new List<CartDTO>();
                list.Add(item);

                Session[CartSession] = list;
            }
            return Json(new
            {
                status = true
            }, JsonRequestBehavior.AllowGet);
        }

        //Xóa từng sản phẩm
        public JsonResult Delete(long id)
        {
            var cartSec = (List<CartDTO>)Session[CartSession];
            cartSec.RemoveAll(x => x.Product.ID == id);
            Session[CartSession] = cartSec;
            return Json(new
            {
                status = true
            });
        }

        //Sửa số lượng sp trong giỏ hàng
        public JsonResult Edit(long product_ID, int quantity)
        {
            var productSec = (List<CartDTO>)Session[CartSession];

            foreach (var item in productSec)
            {
                if (item.Product.ID == product_ID)
                {
                    item.Quantity = quantity;
                }

            }

            Session[CartSession] = productSec;
            return Json(new
            {
                status = true
            });
        }


        public ActionResult Payment()
        {
            if(Session["user"] == null)
            {
                TempData["error"] = "Bạn vui lòng đăng nhập để đặt hàng.";
                Session["check_payment"] = true;
                return Redirect("/user/login");
            }
            Session["check_payment"] = null;
            return View();
        }

        //Thanh toán hóa đơn
        [HttpPost]
        public ActionResult OrderPayment(ShoppingOnline.Models.Entities.Order order)
        {
            var listSeller = new List<long>();
            var cart = (List<CartDTO>)Session[CartSession];
            foreach(var item in cart)
            {
                listSeller.Add((long)item.Seller_ID);
            }
            foreach (var jtem in listSeller.Distinct())
            {
                var res = new OrderBusiness().addOrder(order, jtem);
                decimal? TotalMoney = 0;
                int Quantity = 0;
                if (res)
                {
                    foreach (var item in cart)
                    {
                        if(item.Product.User_ID == jtem)
                        {
                            var od = new Order_Detail();
                            od.Product_ID = item.Product.ID;
                            od.Quantity = item.Quantity;
                            od.Order_ID = new OrderBusiness().findMaxID();
                            if (item.Product.Price != null)
                            {
                                od.Price = item.Product.Price;
                                od.Amount = (int)item.Product.Price * item.Quantity;
                                TotalMoney += item.Product.Price * item.Quantity;
                            }
                            else
                            {
                                od.Price = item.Product.Promotion_Price;
                                od.Amount = (int)item.Product.Promotion_Price * item.Quantity;
                                TotalMoney += item.Product.Promotion_Price * item.Quantity;
                            }
                            Quantity += item.Quantity;
                            new OrderBusiness().addOrder_Detail(od);

                            //update tổng tiền và số lượng 
                            new OrderBusiness().Update_Order((long)od.Order_ID, TotalMoney, Quantity);
                        }
                        
                    }
                    
                }
                else
                {
                    return RedirectToAction("Payment");
                }
            }
            Session[CartSession] = null;
            return RedirectToAction("Order_success");
        }


        //Thanh toán paypal
        //account: payment_paypal@eshopper.com
        //password: 12345678
        //account: sb-4zzpq3257876@personal.example.com
        //password: sb-4zzpq3257876
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            //getting the apiContext  
            APIContext apiContext = PayPalModel.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Cart/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                return View("FailureView");
            }
            //on successful payment, show success page to user.
            Session[CartSession] = null;
            return View("Order_success");
        }
        private PayPal.Api.Payment payment;


        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }


        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            var Listcart = (List<CartDTO>)Session[CartSession];
            decimal tong = 0;
            decimal? totalmoney = 0;
            int quantity = 0;
            foreach (var item in Listcart)
            {
                var vnd = Convert.ToDecimal((item.Quantity * (item.Product.Price!= null? item.Product.Price : item.Product.Promotion_Price)) / 23060);
                var money = Math.Round(Convert.ToDecimal((item.Product.Price != null ? item.Product.Price : item.Product.Promotion_Price) / 23060), 2);
                itemList.items.Add(new Item()
                {
                    name = Str_ProductName(item.Product.Product_Name),
                    currency = "USD",
                    price = money.ToString(),
                    quantity = item.Quantity.ToString(),
                    sku = "sku"
                });

                tong += vnd;

                totalmoney += item.Quantity * (item.Product.Price != null ? item.Product.Price : item.Product.Promotion_Price);
                quantity += item.Quantity;
            }
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "1",
                shipping = "1",
                subtotal = Math.Round(tong, 2).ToString()
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = (Convert.ToDouble(details.tax) + Convert.ToDouble(details.shipping) + Convert.ToDouble(details.subtotal)).ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Convert.ToString((new Random()).Next(100000)), //Generate an Invoice No  
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext

            //Lưu đơn hàng vào csdl
            Save_Order();

            return this.payment.Create(apiContext);
        }

        public ActionResult Order_success()
        {
            return View();
        }

        public ActionResult FailureView()
        {
            return View();
        }

        Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        public void Save_Order()
        {
            var listSeller = new List<long>();
            var cart = (List<CartDTO>)Session[CartSession];
            var user = Session["user"] as User;
            
            foreach (var item in cart)
            {
                listSeller.Add((long)item.Seller_ID);
            }
            foreach (var jtem in listSeller.Distinct())
            {
                var order = new ShoppingOnline.Models.Entities.Order();

                //Lưu thông tin đơn hàng
                var maxid = db.Orders.Max(x => x.ID);
                order.Order_Code = "ORDER0" + (maxid + 1).ToString() + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year;
                order.CustomerName = user.Fullname;
                order.CustomerAddress = user.Address;
                order.CustomerPhone = user.Phone;
                order.Email = user.Email;
                order.CreatedDate = DateTime.Now;
                order.Seller_ID = jtem;
                order.User_ID = user.ID;
                order.Status = 1;//Chưa xác nhận đơn hàng
                order.Payment = 1; //Hình thức thanh toán online
                order.PaidDate = DateTime.Now;

                db.Orders.Add(order);
                db.SaveChanges();

                decimal? TotalMoney = 0;
                int Quantity = 0;
                //Lưu chi tiết hóa đơn
                foreach (var item in cart)
                {
                    if (item.Product.User_ID == jtem)
                    {
                        var od = new Order_Detail();
                        od.Product_ID = item.Product.ID;
                        od.Quantity = item.Quantity;
                        od.Order_ID = new OrderBusiness().findMaxID();
                        if (item.Product.Price != null)
                        {
                            od.Price = item.Product.Price;
                            od.Amount = (int)item.Product.Price * item.Quantity;
                            TotalMoney += item.Product.Price * item.Quantity;
                        }
                        else
                        {
                            od.Price = item.Product.Promotion_Price;
                            od.Amount = (int)item.Product.Promotion_Price * item.Quantity;
                            TotalMoney += item.Product.Promotion_Price * item.Quantity;

                        }

                        Quantity += item.Quantity;
                        new OrderBusiness().addOrder_Detail(od);

                        //update tổng tiền và số lượng 
                        new OrderBusiness().Update_Order((long)od.Order_ID, TotalMoney, Quantity);
                    }

                }


            }
        }

        public string Str_ProductName(string str)
        {
            string[] VietNamChar = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ:"
            };
            //Thay thế và lọc dấu từng char      
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                    str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
            }
            string str1 = str.ToLower();
            string[] name = str1.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string meta = null;
            //Thêm dấu '-'
            foreach (var item in name)
            {
                meta = meta + item + " ";
            }
            return meta;
        }

        public void SendEmail(string address, Models.Entities.Order order)
        {
            string email = "";
            string password = "";

            var loginInfo = new NetworkCredential(email, password);
            var msg = new MailMessage();
            var smtpClient = new SmtpClient("smtp.gmail.com", 587);
            msg.IsBodyHtml = true;
            msg.From = new MailAddress(email);
            msg.To.Add(new MailAddress(address));
            msg.Subject = "E-Shopper - Thông báo đơn hàng";
            msg.Body = "<div style='width: 600px; height: 100 %; margin: auto'>";
            msg.Body += "<div style ='margin:0 250px'>";
            //msg.Body += "<a style='color:#871fff;text-decoration:none' target='blank'" +
            //                    "style='vertical-align:middle;width:45%;height:auto;max-width:100%;border-width:0'" +
            //                    "class='CToWUd'>" +
            //                        "<img src='https://lh3.googleusercontent.com/1LIw-IQwGPyWHez1tuBM7QwgHKAhvGfnpKjTsT1wf0Onk4ADg20r2PMwCFOXb9-getwcPhUS0zrwRQJwBX4fn1uGkNM-Rf7vTS1ONms0SLGL6Vi6wd8gnu3bBQViLa6Nxp-K_k3f7n_5NeI1j57MaT-HURsxPo70zQfQm36axC62bQUlvNUqj03KRi_lmJVAIPVvXIx7ZtfJaVgG4evv8GexGHL5ngmVuR_OG7OZdzpr8HuzJ9weMsoR4Y7EghQ9odxkgOtyShkSLUnEWCkvXsIrDbdGYsI-mPMox-fVpDJXo80RDdtiBy9lH_9bkQ-O5fCWGUvc1RFjVrn50IxGaBwdRuFD1Yzj3B6Z_ejZDhILGX_ptYOPjeAUGFngSV5JfT4TN22v8GisY7v8dLOhG6uW8bg8BgGynua5PA1gxoNQRGyjmGDREbV2f6loMv4xDLMrlf4ND2gf04Iapxh8_AePp9EHBzORmCVIB2ZSGMYdARYAl61mQnXBjBesdMdAGVOgIzL4LoADQtjQWRwOC5re_R3puuYJLLAHaF1rUrDhKgaxir0_ilGhY22o1mHmlEphXGyDuAdKV3plJgDp57J7wzorQhK8A69VR1yjxe9xY8rJin3miMMlYRItmaGoDkzB-y8ZmKdTlMCzyUF3YYiAwi5Z9Tejv_UwNNmjrS1qUsGcISZKFkZuSyA=w235-h66-no'" +
            //                        "style='vertical-align:middle;border-width:0'" +
            //                        "class='CToWUd'>" +
            //                "</a>";
            msg.Body += "<div>" +
                            "<div style='font-weight: normal;line-height: 48px'>" +
                                "<h1>Thông báo đơn hàng</h1>" +
                            "</div>" +
                        "</div>";
            msg.Body += "<div>" +
                            "<p style='font-size:16px;margin-bottom:16px;line-height:24px'> " +
                                "Họ và tên: <span style='color: blue'>" + order.CustomerName + "</span>.</p>" +
                             "<p style='font-size:16px;margin-bottom:16px;line-height:24px'> " +
                                "Địa chỉ: <span style='color: blue'>" + order.CustomerAddress + "</span>.</p>" +
                             "<p style='font-size:16px;margin-bottom:16px;line-height:24px'> " +
                                "Số điện thoại: <span style='color: blue'>" + order.CustomerPhone + "</span>.</p>" +
                             "<p style='font-size:16px;margin-bottom:16px;line-height:24px'> " +
                                "Tổng tiền đơn hàng: <span style='color: blue'>" + order.TotalMoney.Value.ToString("N0") + "</span>.</p>" +
                            "<p style='font-size:16px;margin-bottom:16px;line-height:24px'> Cảm ơn bạn đã đặt hàng tại E-Shopper </p>" +
                        "</div>";
            msg.Body += "<p style='font-family: 'Roboto', sans-serif;'>" +
                            "<span style='line-height:24px;font-size:16px'> Trang mua sắm online E-Shopper,</span><br>" +
                            "<span style='line-height:24px;font-size:16px'> Dành cho bạn những sản phẩm với chất lượng tốt nhất</span >" +
                        "</p>" +

                        "<hr style='margin:40px 0 20px 0;display:block;height:1px;border:0;border-top:1px solid #c4cdd5;padding:0'>" +
                        "<footer style='font-family: 'Roboto', sans-serif; margin-bottom:40px'>" +
                            "<span style='color:#919eab;line-height:28px;font-size:12px'> E-SHOPPER</span><br>" +
                            "<span style='color:#919eab;line-height:28px;font-size:12px'> Mail thông báo đơn hàng. Do not reply </span ><br >";
            msg.Body += "</div>";
            msg.Body += "</div>";
            msg.BodyEncoding = UTF8Encoding.UTF8;
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = true;
            smtpClient.Credentials = loginInfo;
            smtpClient.Send(msg);

        }

    }
}