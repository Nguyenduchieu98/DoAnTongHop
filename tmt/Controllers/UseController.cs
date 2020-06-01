using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tmt.Models;
namespace tmt.Controllers
{
    public class UseController : Controller
    {
        QLKSDataContext db = new QLKSDataContext();
        // GET: Use
        [HttpGet]
        public ActionResult DN(string url, string msg)
        {
            ViewBag.url = url;
            ViewBag.ThongBao = msg;
            return View();
        }
        [HttpPost]
        public ActionResult DN(FormCollection form, string url)
        {
            var id = form["TenDN"];
            var pw = form["PWDN"];
         
            KhacHang kh = db.KhacHangs.SingleOrDefault(x => x.UserName == id && x.PassWord == pw);
            if (kh != null)
            {
                Session["TaiKhoan"] = kh;
                if(url ==null)
                {
                                 
                    return RedirectToAction("Index", "QLKS");
                }
                else
                    return Redirect(url);

            }
            else
                ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
            return RedirectToAction("DN", "Use", new { msg = ViewBag.ThongBao });
        }

        public ActionResult LogOut()
        {            
            return PartialView();
        }

        public ActionResult SLogOut()
        {           
            Session.Clear();
            return Redirect(Request.UrlReferrer.ToString());
        }
        [HttpGet]
        public ActionResult Register1()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult Register1(FormCollection form, KhacHang kh)
        {
            var id = form["ID"];
            var PW = form["PW"];
            var rePW = form["rePW"];
            var Ten = form["Ten"];
            var DiaChi = form["DiaChi"];
            var Phone = form["phone"];
            var email = form["email"];
            DateTime NgaySinh = DateTime.Parse(form["NgaySinh"]);

            kh.UserName = id;
            kh.PassWord = PW;
            kh.HoTen = Ten;
            kh.DiaChi = DiaChi;
            kh.SDT = Phone;
            kh.Email = email;
            kh.NgaySinh = NgaySinh;
            if (PW == rePW)
            {
                db.KhacHangs.InsertOnSubmit(kh);
                db.SubmitChanges();
                return RedirectToAction("DN", "Use");
            }
            else
                ViewBag.ThongBao = "Nhập lại mật khẩu không đúng!";
            return RedirectToAction("DN","Use");
        }
    }
}