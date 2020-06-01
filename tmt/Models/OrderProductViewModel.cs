using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace tmt.Models
{
    public class OrderProductViewModel
    {
        [DisplayName("Mã Hóa Đơn")]
        public int order_id { set; get; }
        [DisplayName("Ngày Đặt")]
        public DateTime? order_day { set; get; }
        [DisplayName("Tên Khách Sạn")]
        public string ten_ks { set; get; }
        public int? order_userid { set; get; }
        public decimal? order_total { set; get; }
        [DisplayName("Tên Khách Hàng")]
        public string order_username { set; get; }
    }
}