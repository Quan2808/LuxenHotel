using Microsoft.AspNetCore.Mvc;

namespace LuxenHotel.Areas.Customer.Controllers;

[Area("Customer")]
public class BookingController : Controller
{
    // Action hiển thị danh sách chỗ ở hoặc giao diện đặt phòng
    [HttpGet]
    public ActionResult Accommodations()
    {
        return View();
    }

    // Action hiển thị thông tin chỗ ở
    [HttpGet]
    public ActionResult AccommodationDetails()
    {
        return View();
    }

    // Action xử lý đặt chỗ ở
    [HttpPost]
    public ActionResult BookAccommodations()
    {
        return View();
    }

    // Action hiển thị danh sách dịch vụ hoặc giao diện đặt dịch vụ
    [HttpGet]
    public ActionResult Services()
    {
        return View();
    }

    // Action xử lý đặt dịch vụ
    [HttpPost]
    public ActionResult BookServices()
    {
        return View();
    }
}