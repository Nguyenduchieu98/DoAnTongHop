using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tmt.Models;

namespace tmt.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        QLKSDataContext data = new QLKSDataContext();

        public ActionResult Index()
        {
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
                return RedirectToAction("Login");
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["Taikhoanadmin"] != null && Session["Taikhoanadmin"].ToString() != "")
                return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            QLKSDataContext data = new QLKSDataContext();

            var tendn = collection["username"];
            var matkhau = collection["password"];
            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Phải nhập tên đăng nhập";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Phải nhập mật khẩu";
            }
            else
            {
                TaiKhoan ad = data.TaiKhoans.SingleOrDefault(t => t.TenTK == tendn && t.MatKhau == matkhau);
                if (ad != null)
                {
                    Session["Taikhoanadmin"] = ad;
                    Session["HoTenAdmin"] = ad.TenTK;
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Remove("Taikhoanadmin");
            Session.Remove("HoTenAdmin");
            return RedirectToAction("Login");
        }

        //*** Hotel Control ***
        public ActionResult HotelControl(int? page)
        {
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 15;
            return View(data.KhachSans.ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult DetailHotel(int id)
        {
            KhachSan p = data.KhachSans.SingleOrDefault(n => n.MaKS == id);
            return View(p);
        }

        [HttpGet]
        public ActionResult EditHotel(int id)
        {
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            KhachSan p = data.KhachSans.SingleOrDefault(i => i.MaKS == id);
            ViewData["Image"] = p.hinh_1;
            if (p == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(p);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditHotel(KhachSan p, HttpPostedFileBase NewImageP, FormCollection col)
        {
            var image = col["ImageP"];
            KhachSan item = data.KhachSans.First(n => n.MaKS == p.MaKS);

            if (ModelState.IsValid)
            {
                if (NewImageP != null)
                {
                    if (Path.GetExtension(NewImageP.FileName).ToLower() == ".jpg"
                        || Path.GetExtension(NewImageP.FileName).ToLower() == ".png"
                        || Path.GetExtension(NewImageP.FileName).ToLower() == ".gif"
                        || Path.GetExtension(NewImageP.FileName).ToLower() == ".jpeg"
                        )
                    {
                        var filename = Path.GetFileName(NewImageP.FileName);
                        var path = Path.Combine(Server.MapPath("~/images"), filename);
                        if (System.IO.File.Exists(path))
                        {
                            ViewBag.error = "Tên ảnh đã tồn tại";
                        }
                        else
                        {
                            var newImageP = NewImageP.FileName;
                            NewImageP.SaveAs(path);
                            item.hinh_1 = newImageP;
                            UpdateModel(item);
                            data.SubmitChanges();
                            return RedirectToAction("HotelControl");
                        }
                    }
                    else
                    {
                        ViewBag.error = "Hãy chọn một ảnh";
                    }
                }
                else
                {
                    item.hinh_1 = image;
                    item.hinh_2 = image;
                    UpdateModel(item);
                    data.SubmitChanges();
                    return RedirectToAction("HotelControl");
                }
            }
            return this.EditHotel(p.MaKS);
        }

        [HttpGet]
        public ActionResult CreateHotel()
        {
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateHotel(KhachSan p, HttpPostedFileBase fileUpload, FormCollection collection)
        {
            var filename = Path.GetFileName(fileUpload.FileName);
            var path = Path.Combine(Server.MapPath("~/Images"), filename);
            if (System.IO.File.Exists(path))
            {
                ViewBag.Thongbao = "Hình ảnh đã tồn tại";
            }
            else
            {
                fileUpload.SaveAs(path);
            }

            var tenks = collection["TenKS"];
            var gioithieu = collection["GioiThieu"];
            var diachi = collection["DiaChi"];
            var sdt = collection["SDT"];

            p.TenKS = tenks;
            p.hinh_1 = filename;
            p.hinh_2 = filename;
            p.GioiThieu = gioithieu;
            p.DiaChi = diachi;
            p.SDT = sdt;
            p.MaKV = 1;
            p.star = 4;
            p.point = 7;
            p.QC = true;
            p.LuotDanhGia = 500;
            data.KhachSans.InsertOnSubmit(p);
            data.SubmitChanges();

            return RedirectToAction("HotelControl");
        }
        public ActionResult RemoveHotel(int id)
        {
            List<DonDatPhong> ls_ddp = data.DonDatPhongs.Where(t => t.MaKS == id).ToList();
            foreach (DonDatPhong ddp in ls_ddp)
            {
                data.DonDatPhongs.DeleteOnSubmit(ddp);
            }

            List<DiemDanhGia> ls_ddg = data.DiemDanhGias.Where(t => t.MaKS == id).ToList();
            foreach (DiemDanhGia ddg in ls_ddg)
            {
                data.DiemDanhGias.DeleteOnSubmit(ddg);
            }

            List<ChiTietDatPhong> ls_ctdp = data.ChiTietDatPhongs.Where(t => t.maks == id).ToList();
            foreach (ChiTietDatPhong ctdp in ls_ctdp)
            {
                data.ChiTietDatPhongs.DeleteOnSubmit(ctdp);
            }

            TienNghi tn = data.TienNghis.FirstOrDefault(t => t.MaKS == id);
            data.TienNghis.DeleteOnSubmit(tn);

            List<Phong> ls_p = data.Phongs.Where(t => t.MaKS == id).ToList();
            foreach (Phong p in ls_p)
            {
                data.Phongs.DeleteOnSubmit(p);
            }

            KhachSan ks = data.KhachSans.SingleOrDefault(i => i.MaKS == id);
            List<HinhAnh> ls_ha = data.HinhAnhs.Where(t => t.MaKS == ks.MaKS).ToList();
            foreach (HinhAnh ha in ls_ha)
            {
                data.HinhAnhs.DeleteOnSubmit(ha);
            }
            if (ks == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //var filePath = Server.MapPath("~/images/products" + p.ImageProduct);
            var filePath = Path.Combine(Server.MapPath("~/images"), ks.hinh_1);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            data.KhachSans.DeleteOnSubmit(ks);
            data.SubmitChanges();
            return RedirectToAction("HotelControl");
        }

        //*** Order Control ***
        public ActionResult OrderControl()
        {
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
                return RedirectToAction("Login");
            var lstOrder = from o in data.DonDatPhongs
                           join u in data.KhachSans on o.MaKS equals u.MaKS
                           join t in data.KhacHangs on o.MaKH equals t.MaKH
                           select new OrderProductViewModel
                           {
                               order_id = o.id_ddp,
                               order_day = o.ngay_dat,
                               ten_ks = u.TenKS,
                               order_username = t.HoTen,
                               order_total = o.thanh_tien
                           };
            return View(lstOrder);
        }

        //*** Order Control ***
        public ActionResult CustomerControl()
        {
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
                return RedirectToAction("Login");
            var lstCustomer = data.KhacHangs.ToList();
            return View(lstCustomer);
        }

        public ActionResult RemoveOrder(int id)
        {
            DonDatPhong ddp = data.DonDatPhongs.FirstOrDefault(t => t.id_ddp == id);
            data.DonDatPhongs.DeleteOnSubmit(ddp);
            data.SubmitChanges();
            return RedirectToAction("OrderControl");
        }

        public ActionResult RemoveCustomer(int id)
        {
            List<DonDatPhong> ls_p = data.DonDatPhongs.Where(t => t.MaKH == id).ToList();
            foreach (DonDatPhong p in ls_p)
            {
                data.DonDatPhongs.DeleteOnSubmit(p);
            }

            KhacHang kh = data.KhacHangs.FirstOrDefault(t => t.MaKH == id);
            data.KhacHangs.DeleteOnSubmit(kh);
            data.SubmitChanges();
            return RedirectToAction("CustomerControl");
        }
    }
}