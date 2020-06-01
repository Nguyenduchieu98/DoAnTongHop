using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tmt.Models;
using PagedList;

namespace tmt.Controllers
{
    public class QLKSController : Controller
    {
        QLKSDataContext db = new QLKSDataContext();
        // GET: QLKS

        List<KhuVuc> takekv(int count)
        {
            return db.KhuVucs.Take(count).ToList();
        }

        List<KhachSan> ksnoibat(int count)
        {
            return db.KhachSans.Take(count).OrderByDescending(x => x.point).ToList();
        }
        private bool CoPhong(List<p> list, int sopdon, int sopdoi)
        {
            foreach (p p in list)
            {
                if (sopdon >= p.P2 && sopdoi >= p.P4)
                {
                    return true;
                }
            }
            return false;
        }

        public ActionResult Index()
        {
            List<KhuVuc> kv = db.KhuVucs.ToList();

            ViewBag.kv = kv;
            var t = takekv(4);

            return View(t);
        }

        //public ActionResult _JSStar()
        //{
        //    List<string> starid = new List<string>() { "star1", "star2", "star3", "star4", "star5" };
        //    return PartialView(starid);
        //}

        public ActionResult _JSStar()
        {
            List<string> filter = new List<string>() { "star1", "star2", "star3", "star4", "star5",
                                                        "service1", "service2", "service3", "service4", "service5","service6","service7",
                                                         "gia","xh","ssstar"};
            return PartialView(filter);
        }
        
        public ActionResult Search(int? page,string destination, DateTime? CheckIn, DateTime? CheckOut, int? people, int? children, int? room, bool? star1, bool? star2, bool? star3, bool? star4, bool? star5)
        {
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            ViewBag.destination = destination;
            ViewBag.CheckIn = CheckIn;
            ViewBag.CheckOut = CheckOut;
            ViewBag.people = people;

            List<KhachSan> ks = new List<KhachSan>();

            if (people == null)
            {
                people = 1;
            }

            KhuVuc kv = db.KhuVucs.FirstOrDefault(x => x.TenKV.Equals(destination));
            if (kv != null )
            {
                ks = db.KhachSans.Where(x => x.MaKV == kv.MaKV).ToList();
                List<KhachSan> r = new List<KhachSan>();
                List<p> list = new List<p>();

                int SoNguoi = (int)people;
                if(children>people)
                {
                    SoNguoi = SoNguoi + (int)(children + people);
                }
                int PDoi = SoNguoi % 4 == 0 ? SoNguoi / 4 : SoNguoi / 4 + 1;

                for (int i = PDoi * 4; i >= 0; i -= 4)
                {
                    p t = new p();
                    t.P4 = i / 4;
                    t.P2 = 0;
                    for (int j = SoNguoi - i; j > 0; j -= 2)
                    {
                        t.P2++;
                    }
                    list.Add(t);
                }
                foreach (KhachSan k in ks)
                {
                    int SLPDon = 0;
                    int SLPDoi = 0;
                    if (db.Phongs.Where(x => x.MaKS == k.MaKS && x.MaLoai == 1).Count() != 0)
                    {
                        SLPDon = (int)db.Phongs.Where(x => x.MaKS == k.MaKS && x.MaLoai == 1).Sum(x => x.SL);
                    }

                    if (db.Phongs.Where(x => x.MaKS == k.MaKS && x.MaLoai == 2).Count() != 0)
                    {
                        SLPDoi = (int)db.Phongs.Where(x => x.MaKS == k.MaKS && x.MaLoai == 2).Sum(x => x.SL);
                    }

                    
                    if (CoPhong(list, SLPDon, SLPDoi))
                    {
                        r.Add(k);
                    }
                }             
                return View(r.ToPagedList(pageNumber, pageSize));
            }
            if (kv == null)
            {
                ks = db.KhachSans.Where(x => x.DiaChi.Contains(destination)).ToList();
            }
            return View(ks.ToPagedList(pageNumber, pageSize));
        }



        public ActionResult _Phong(int maks)
        {
            List<Phong> p = db.Phongs.Where(x => x.MaKS == maks).ToList();
            return PartialView(p);
        }


        public ActionResult _SearchAJAX(int? page, string destination, DateTime? CheckIn, DateTime? CheckOut, int? people, int? children, int? room,
            bool? star1, bool? star2, bool? star3, bool? star4, bool? star5,
            bool? service1, bool? service2, bool? service3, bool? service4, bool? service5, bool? service6, bool? service7,
            bool? gia,bool? xh,bool? ssstar)
        {
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            ViewBag.destination = destination;
            ViewBag.CheckIn = CheckIn;
            ViewBag.CheckOut = CheckOut;
            ViewBag.people = people;
            ViewBag.star1 = star1;
            ViewBag.star2 = star2;
            ViewBag.star3 = star3;
            ViewBag.star4 = star4;
            ViewBag.star5 = star5;

            int s1 = star1 == null ? -1 : 1;
            int s2 = star2 == null ? -2 : 2;
            int s3 = star3 == null ? -3 : 3;
            int s4 = star4 == null ? -4 : 4;
            int s5 = star5 == null ? -5 : 5;

            bool se1 = service1 == null ? false : true;
            bool se2 = service2 == null ? false : true;
            bool se3 = service3 == null ? false : true;
            bool se4 = service4 == null ? false : true;
            bool se5 = service5 == null ? false : true;
            bool se6 = service6 == null ? false : true;
            bool se7 = service7 == null ? false : true;
            bool giaa = gia == null ? false : true;

            //bool ss = ssstar == null ? false : true;
            //bool rice = gia == null ? false : true;
            //bool rank = xh == null ? false : true;

            List<KhachSan> ks = new List<KhachSan>();

            if (people == null)
            {
                people = 1;
            }

            KhuVuc kv = db.KhuVucs.FirstOrDefault(x => x.TenKV.Equals(destination));

            if (kv != null)
            {
                ks = db.KhachSans.Where(x => x.MaKV == kv.MaKV).ToList();
                //Nếu 1 trong 5 checkbox được check
                if (s1 > 0 || s2 > 0 || s3 > 0 || s4 > 0 || s5 > 0)
                {
                    ks = db.KhachSans.Where(x => x.MaKV == kv.MaKV && (x.star == s1 || x.star == s2 || x.star == s3 || x.star == s4 || x.star == s5)).ToList();

                }
                if (se1 == true)
                {
                    ks = ks.Where(x => x.TienNghi.LeTan24h == se1).ToList();
                }
                if (se2 == true)
                {
                    ks = ks.Where(x => x.TienNghi.ChoThueXe == se2).ToList();
                }
                if (se3 == true)
                {
                    ks = ks.Where(x => x.TienNghi.CachAm == se3).ToList();
                }
                if (se4 == true)
                {
                    ks = ks.Where(x => x.TienNghi.MayGiat == se4).ToList();
                }
                if (se5 == true)
                {
                    ks = ks.Where(x => x.TienNghi.BonTam == se5).ToList();
                }
                if (se6 == true)
                {
                    ks = ks.Where(x => x.TienNghi.Tivi == se5).ToList();
                }
                if (se5 == true)
                {
                    ks = ks.Where(x => x.TienNghi.TuLanh == se5).ToList();
                }

                if (ssstar == true)
                {
                    ks = ks.OrderByDescending(x => x.star).ToList();
                }

                if (xh == true)
                {
                    ks = ks.OrderByDescending(x => x.point).ToList();
                }
                if (giaa == true)
                {
                    ks = ks.OrderBy(x => x.Phongs.Min(y => y.Gia25)).ToList();
                }
                List<KhachSan> r = new List<KhachSan>();
                List<p> list = new List<p>();
                int song = (int)people;
                int sop4 = song % 4 == 0 ? song / 4 : song / 4 + 1; ;
                int song4 = sop4 * 4;
                for (int i = song4; i >= 0; i -= 4)
                {
                    p t = new p();
                    t.P4 = i / 4;
                    t.P2 = 0;
                    for (int j = song - i; j > 0; j -= 2)
                    {
                        t.P2 += 1;
                    }
                    list.Add(t);
                }
                foreach (KhachSan k in ks)
                {
                    int SLPDon = 0;
                    int SLPDoi = 0;
                    if (db.Phongs.Where(x => x.MaKS == k.MaKS && x.MaLoai == 1).Count() != 0)
                    {
                        SLPDon = (int)db.Phongs.Where(x => x.MaKS == k.MaKS && x.MaLoai == 1).Sum(x => x.SL);
                    }

                    if (db.Phongs.Where(x => x.MaKS == k.MaKS && x.MaLoai == 2).Count() != 0)
                    {
                        SLPDoi = (int)db.Phongs.Where(x => x.MaKS == k.MaKS && x.MaLoai == 2).Sum(x => x.SL);
                    }
                    if (CoPhong(list, SLPDon, SLPDoi))
                    {
                        r.Add(k);
                    }
                }

                return PartialView(r.ToPagedList(pageNumber, pageSize));
            }
            if (kv == null)
            {
                ks = db.KhachSans.Where(x => x.DiaChi.Contains(destination)).ToList();
            }
            return PartialView(ks.ToPagedList(pageNumber, pageSize));
        }
        
        public ActionResult QC()
        {
            Random rnd = new Random();
            List<KhachSan> ks = db.KhachSans.Take(8).OrderByDescending(x => x.QC == true).ToList();
            return PartialView(ks);
        }
        public ActionResult KSNoiBat()
        {
            List<KhachSan> ks = ksnoibat(5);
            return PartialView(ks);
        }
    }
}