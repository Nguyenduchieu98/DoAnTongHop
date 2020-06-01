using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tmt.Models;

using PayPal.Api;
using System.Globalization;

namespace tmt.Controllers
{
    public class InfoHotelController : Controller
    {
        QLKSDataContext db = new QLKSDataContext();
        // GET: InfoHotel
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Info1(int id)
        {
            KhachSan ks;
            ks = db.KhachSans.FirstOrDefault(x => x.MaKS == id);
            return View(ks);
        }

        public ActionResult img(int id)
        {
            List<HinhAnh> images = db.HinhAnhs.Where(x => x.MaKS == id).ToList();
            return PartialView(images);
        }

        public ActionResult TienNghi(int id)
        {
            TienNghi tn = db.TienNghis.FirstOrDefault(x => x.MaKS == id);
            return PartialView(tn);
        }
        public ActionResult Gia(int id)
        {
            List<Phong> p = db.Phongs.Where(x => x.MaKS == id).ToList();
            return PartialView(p);
        }

        public ActionResult KSLienQUan(int id)
        {
            KhachSan ks = db.KhachSans.FirstOrDefault(x => x.MaKS == id);

            List<KhachSan> ks1 = db.KhachSans.Where(x => x.MaKV == ks.MaKV || (x.MaKV == ks.MaKV && x.star == ks.star)).ToList();
            return PartialView(ks1);
        }

        [HttpPost]
        public ActionResult Paypal(FormCollection form, string url)
        {
            if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
            {
                return RedirectToAction("DN", "Use", new { url = url });
            }
            else
            {
            short MaKH = Convert.ToInt16(((tmt.Models.KhacHang)Session["TaiKhoan"]).MaKH);
            short maks = Convert.ToInt16(form["maks"]);
            int sopdon = Convert.ToInt32(form["sopdon"]);
            int sopdoi = Convert.ToInt32(form["sopdoi"]);
                DateTime ngayden = DateTime.Parse(form["ngayden"]);
                DateTime ngaytra = DateTime.Parse(form["ngaytra"]);

                KhachSan ks = db.KhachSans.FirstOrDefault(x => x.MaKS == maks);
            Phong pdon = db.Phongs.FirstOrDefault(x => x.MaKS == maks && x.MaLoai == 1);
            Phong pdoi = db.Phongs.FirstOrDefault(x => x.MaKS == maks && x.MaLoai == 2);
            decimal gdon25 = Convert.ToDecimal(pdon.Gia25);
            decimal gdon68 = Convert.ToDecimal(pdon.Gia68);
            decimal gdoi25 = Convert.ToDecimal(pdoi.Gia25);
            decimal gdoi68 = Convert.ToDecimal(pdoi.Gia68);

            
            decimal tongpdon = 0;
            decimal tongpdoi = 0;
            decimal tongthanhtien = 0;
            for (DateTime date = ngayden; date < ngaytra; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Friday && date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    //gia25
                    tongpdon += gdon25 * sopdon;
                    tongpdoi += gdoi25 * sopdoi;
                }
                else
                {
                    //gia68
                    tongpdon += gdon68 * sopdon;
                    tongpdoi += gdoi68 * sopdoi;
                }
            }
            tongthanhtien += tongpdon + tongpdoi;

            string giapdon = tongpdon.ToString();
            string giapdoi = tongpdoi.ToString();
            string thanhtien = tongthanhtien.ToString();

            int? id = 0;
            db.P_GetIDDonDatPhong(ref id);
            string dondat_id = (id+1).ToString();

            DonDatPhong dondat = new DonDatPhong();
            dondat.MaKH = MaKH;
                dondat.MaKS = ks.MaKS;
            dondat.ngay_dat = DateTime.Now;
            dondat.thanh_tien = tongthanhtien;
            dondat.DanhGia = false;

            db.DonDatPhongs.InsertOnSubmit(dondat);

            try
            {
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            ChiTietDatPhong chitiet = new ChiTietDatPhong();
            chitiet.dondat_id = dondat.id_ddp;
            chitiet.maks = maks;
            chitiet.maloai = 1;
            chitiet.soluong = sopdon;
            chitiet.ngay_den = ngayden;
            chitiet.ngay_tra = ngaytra;

            db.ChiTietDatPhongs.InsertOnSubmit(chitiet);

            chitiet = new ChiTietDatPhong();
            chitiet.dondat_id = dondat.id_ddp;
            chitiet.maks = maks;
            chitiet.maloai = 2;
            chitiet.soluong = sopdoi;
            chitiet.ngay_den = ngayden;
            chitiet.ngay_tra = ngaytra;

            db.ChiTietDatPhongs.InsertOnSubmit(chitiet);

            try
            {
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Session["maks"] = maks;
            Session["sopdon"] = sopdon;
            Session["sopdoi"] = sopdoi;
            Phong phdon = db.Phongs.FirstOrDefault(x => x.MaKS == maks && x.MaLoai == 1);
            phdon.SL -= Convert.ToInt16(sopdon);
            Phong phdoi = db.Phongs.FirstOrDefault(x => x.MaKS == maks && x.MaLoai == 2);
            phdoi.SL -= Convert.ToInt16(sopdoi);
            try
            {
                db.SubmitChanges();
            }
            catch (Exception)
            {
            }

            var apiContext = Configuration.GetAPIContext();

            string payerId = Request.Params["PayerID"];
            if (string.IsNullOrEmpty(payerId))
            {
                var itemList = new ItemList()
                {
                    items = new List<Item>()
                                    {
                        new Item()
                        {
                            name = "p don",
                            currency = "USD",
                            price = giapdon,
                            quantity = "1",
                            sku = "sku"
                        },
                        new Item()
                        {
                            name = "p doi",
                            currency = "USD",
                            price = giapdoi,
                            quantity = "1",
                            sku = "sku"
                        }
                                    }
                };

                var payer = new Payer() { payment_method = "paypal" };

                var baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/InfoHotel/Paypal?";
                var guid = Convert.ToString((new Random()).Next(100000));
                var redirectUrl = baseURI + "guid=" + guid;
                var redirUrls = new RedirectUrls()
                {
                    cancel_url = redirectUrl + "&cancel=true",
                    return_url = redirectUrl
                };

                var details = new Details()
                {
                    tax = "0",
                    shipping = "0",
                    subtotal = thanhtien
                };

                var amount = new Amount()
                {
                    currency = "USD",
                    total = thanhtien, // Total must be equal to sum of shipping, tax and subtotal.
                    details = details
                };

                var transactionList = new List<Transaction>();

                transactionList.Add(new Transaction()
                {
                    description = ks.TenKS,
                    invoice_number = dondat_id,
                    amount = amount,
                    item_list = itemList
                });

                var payment = new Payment()
                {
                    intent = "sale",
                    payer = payer,
                    transactions = transactionList,
                    redirect_urls = redirUrls
                };
                    string paypalRedirectUrl = null;
                    try
                    {
                        var createdPayment = payment.Create(apiContext);
                        
                        var links = createdPayment.links.GetEnumerator();
                        while (links.MoveNext())
                        {
                            var link = links.Current;
                            if (link.rel.ToLower().Trim().Equals("approval_url"))
                            {
                                //
                                paypalRedirectUrl = link.href;
                            }
                        }
                        Session.Add(guid, createdPayment.id);
                    }
                    catch (Exception ee)
                    {
                        //fail
                        DonDatPhong zdondat = (db.DonDatPhongs.OrderByDescending(x => x.id_ddp)).FirstOrDefault();
                        db.DonDatPhongs.DeleteOnSubmit(zdondat);
                        Phong zphdon = db.Phongs.FirstOrDefault(x => x.MaKS == (short)Session["maks"] && x.MaLoai == 1);
                        zphdon.SL += Convert.ToInt16(Session["sopdon"]);
                        Phong zphdoi = db.Phongs.FirstOrDefault(x => x.MaKS == (short)Session["maks"] && x.MaLoai == 2);
                        zphdoi.SL += Convert.ToInt16(Session["sopdoi"]);
                        try
                        {
                            db.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        return View("Fail");
                    }
                
                return Redirect(paypalRedirectUrl);
            }
            else
            {
                var guid = Request.Params["guid"];

                var paymentId = Session[guid] as string;
                var paymentExecution = new PaymentExecution() { payer_id = payerId };
                var payment = new Payment() { id = paymentId };

                var executedPayment = payment.Execute(apiContext, paymentExecution);

                if (executedPayment.state.ToLower() != "approved")
                {
                    //fail
                    return View("Fail");
                }
            }
        }
            return View("Cám ơn bạn đã sử dụng dịch vụ của chúng tôi");
        }

        public ActionResult Paypal()
        {
            var apiContext = Configuration.GetAPIContext();

            string payerId = Request.Params["PayerID"];
            if (string.IsNullOrEmpty(payerId))
            {

            }
            else
            {
                var guid = Request.Params["guid"];

                var paymentId = Session[guid] as string;
                var paymentExecution = new PaymentExecution() { payer_id = payerId };
                var payment = new Payment() { id = paymentId };
                var executedPayment = new Payment();
                try
                {
                    executedPayment = payment.Execute(apiContext, paymentExecution);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        //fail
                        DonDatPhong dondat = (db.DonDatPhongs.OrderByDescending(x => x.id_ddp)).FirstOrDefault();
                        db.DonDatPhongs.DeleteOnSubmit(dondat);
                        Phong phdon = db.Phongs.FirstOrDefault(x => x.MaKS == (short)Session["maks"] && x.MaLoai == 1);
                        phdon.SL += Convert.ToInt16(Session["sopdon"]);
                        Phong phdoi = db.Phongs.FirstOrDefault(x => x.MaKS == (short)Session["maks"] && x.MaLoai == 2);
                        phdoi.SL += Convert.ToInt16(Session["sopdoi"]);
                        try
                        {
                            db.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        return View("Fail");
                    }
                }
                catch (Exception ee)
                {
                    DonDatPhong dondat = (db.DonDatPhongs.OrderByDescending(x => x.id_ddp)).FirstOrDefault();
                    db.DonDatPhongs.DeleteOnSubmit(dondat);
                    Phong phdon = db.Phongs.FirstOrDefault(x => x.MaKS == (short)Session["maks"] && x.MaLoai == 1);
                    phdon.SL += Convert.ToInt16(Session["sopdon"]);
                    Phong phdoi = db.Phongs.FirstOrDefault(x => x.MaKS == (short)Session["maks"] && x.MaLoai == 2);
                    phdoi.SL += Convert.ToInt16(Session["sopdoi"]);
                    try
                    {
                        db.SubmitChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    return View("Fail");
                }


            }
            return View("Success");
        }

        public ActionResult DanhGia(int maks)
        {
            DonDatPhong d = new DonDatPhong();
            if (Session["TaiKhoan"]!=null)
            {
                KhacHang t = (KhacHang)Session["TaiKhoan"];

                d = db.DonDatPhongs.FirstOrDefault(x => x.MaKH == t.MaKH);
               
            }

            ViewBag.maks = maks;
            return PartialView(d);
        }
        [HttpPost]
        public ActionResult DanhGia(FormCollection form)
        {
            short sachse = Convert.ToInt16(form["SachSe"]);
            short thoaimai = Convert.ToInt16(form["ThoaiMai"]);
            short tiennghi = Convert.ToInt16(form["TienNghi"]);
            short phucvu = Convert.ToInt16(form["PhucVu"]);
            short dangtien = Convert.ToInt16(form["DangTien"]);
            short wifi = Convert.ToInt16(form["Wifi"]);
            short diadiem = Convert.ToInt16(form["DiaDiem"]);
            short maks = Convert.ToInt16(form["maks"].ToString().Replace(",", ""));

            KhacHang k = (KhacHang)Session["TaiKhoan"];
            List<DiemDanhGia> t = db.DiemDanhGias.Where(x => x.MaKS == maks&& x.MaKH == k.MaKH).OrderByDescending(x=>x.Lan).ToList();

            DiemDanhGia d = new DiemDanhGia();
            d.MaKS = maks;
            d.MaKH = k.MaKH;
            d.SachSe = sachse;
            d.ThoaiMai = thoaimai;
            d.TienNghi = tiennghi;
            d.PhucVu = phucvu;
            d.DangTien = dangtien;
            d.Wifi = wifi;
            d.DiaDiem = diadiem;

            if (t.Count > 0)
            {                
                d.Lan = Convert.ToInt16((int)t[0].Lan + 1);
               
                
            }
            else
            {
                d.Lan = 1;
            }

            db.DiemDanhGias.InsertOnSubmit(d);

            List<DonDatPhong> dondats = db.DonDatPhongs.Where(x => x.MaKS == maks && x.MaKH == k.MaKH && x.DanhGia == false).OrderByDescending(x => x.id_ddp).ToList();
            if (dondats.Count > 0)
            {
                dondats[0].DanhGia = true;
            }

            try
            {
                db.SubmitChanges();
            }
            catch (Exception)
            {

                throw;
            }

            return RedirectToAction("Index", "QLKS");
           
        }
    }
}