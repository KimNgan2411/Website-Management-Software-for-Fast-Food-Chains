using FastFoodBranch.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FastFoodBranch.Controllers
{
    public class DistanceController : Controller
    {
        private readonly HttpClient _client;

        public DistanceController()
        {
            _client = new HttpClient();
        }

        //Lấy khoảng cách đối với tài khoản đã đăng nhập
        [HttpGet]
        public async Task<JsonResult> CalculateDistance( string storeAddress)
        {
            string apiKey = "AIzaSyD9s4cfdiiNotBkyL-tJV2PvEXe-L-TByw";
            var user = (KhachHang)Session["userKH"];
            if(user != null)
            {
                // Lấy tọa độ từ địa chỉ người dùng và cửa hàng
                var userLocation = await GetCoordinates(user.DiaChi, apiKey);
                var storeLocation = await GetCoordinates(storeAddress, apiKey);

                // Tính toán khoảng cách từ người dùng đến cửa hàng
                var distance = await GetDistance(userLocation, storeLocation, apiKey);

                //return Content($"Khoảng cách từ {userAddress} đến {storeAddress}: {distance}");
                return Json(new
                {
                    success = true,
                    StoreAddress = storeAddress,
                    Distance = distance
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            
        }

        //Lấy vị trị của người dùng đang đứng sau đó tính khoảng cách
        [HttpGet]
        public async Task<JsonResult> CalculateDistance1(double userLatitude, double userLongitude, string storeAddress)
        {
            string apiKey = "AIzaSyD9s4cfdiiNotBkyL-tJV2PvEXe-L-TByw";

            var userLocation = $"{userLatitude},{userLongitude}";
            var storeLocation = await GetCoordinates(storeAddress, apiKey);

            var distance = await GetDistance(userLocation, storeLocation, apiKey);

            return Json(new
            {
                success = true,
                UserLatitude = userLatitude,
                UserLongitude = userLongitude,
                StoreAddress = storeAddress,
                Distance = distance
            }, JsonRequestBehavior.AllowGet);
        }

        private async Task<string> GetCoordinates(string address, string apiKey)
        {
            var encodedAddress = Uri.EscapeDataString(address);
            var apiUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={apiKey}";

            var response = await _client.GetAsync(apiUrl);
            var content = await response.Content.ReadAsStringAsync();

            dynamic result = JsonConvert.DeserializeObject(content);
            var location = result.results[0].geometry.location;

            return $"{location.lat},{location.lng}";
        }

        private async Task<string> GetDistance(string origin, string destination, string apiKey)
        {
            var apiUrl = $"https://maps.googleapis.com/maps/api/distancematrix/json?units=metric&origins={origin}&destinations={destination}&key={apiKey}";

            var response = await _client.GetAsync(apiUrl);
            var content = await response.Content.ReadAsStringAsync();

            dynamic result = JsonConvert.DeserializeObject(content);
            var distance = result.rows[0].elements[0].distance.text;

            return distance;
        }
    }
}