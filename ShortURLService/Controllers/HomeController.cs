using ShortURLService.DAL;
using ShortURLService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ShortURLService.Controllers
{
    public class HomeController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        UrlContext db = new UrlContext();        
        public ActionResult Index()
        {
            IEnumerable<URL> model = new List<URL>();
            db.Configuration.ProxyCreationEnabled = false;
            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.GetUserId();
                model = db.Urls.Where(u => u.UserId == userId).OrderByDescending(u => u.GeneratedDate).AsEnumerable();
            }
            return View(model);
        }

        public ActionResult RedirectToLong(string shortURL)
        {
            if (string.IsNullOrEmpty(shortURL))
                return RedirectToAction("NotFound", "Home");
            else
            {
                int location = URL.Decode(shortURL);
                URL url = db.Urls.Where(u => u.UrlId == location).FirstOrDefault();


                if (url == null)
                    return RedirectToAction("NotFound", "Home");
                else
                {
                    #region Statistics collected for this URL
                    UrlStat stats = new UrlStat(Request);
                    stats.UrlId = url.UrlId; // relation

                    try
                    {
                        db.UrlStats.Add(stats);
                        db.SaveChanges();
                    }
                    catch (Exception exc)
                    {
                        log.Error(exc);
                    }
                    #endregion

                    url.Hits++; // increment visits
                    db.SaveChanges();
                    Response.StatusCode = 302;
                    return Redirect(url.LongUrl); // redirects to the long URL
                }
            }
        }

        public ActionResult NotFound()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ShorterURL(string longUrl)
        {
            db.Configuration.ProxyCreationEnabled = false;

            if (string.IsNullOrEmpty(longUrl))
                return Json(new { status = false, message = "Please provide URL" }, JsonRequestBehavior.AllowGet);
            else
            {
                if (!new URL().HasHTTPProtocol(longUrl))
                    longUrl = "http://" + longUrl;

                // Check if long URL already exists in the database
                URL existingURL = db.Urls.Where(u => u.LongUrl.ToLower() == longUrl.ToLower()).FirstOrDefault();

                // if the long url doesnt yet exist
                if (existingURL == null)
                {
                    URL shortUrl = new URL()
                    {
                        LongUrl = longUrl,
                        GeneratedDate = DateTime.UtcNow,
                        Hits = 0
                    };
                    string userId = User.Identity.GetUserId();

                    if (shortUrl.CheckLongUrlExists())  // goes to the site to check its valid
                    {
                        // Main work happens here
                        var prevItem = db.Urls.OrderByDescending(s => s.UrlId).FirstOrDefault();
                        int nextItemId = prevItem != null ? prevItem.UrlId + 1 : 1;
                        string result = URL.Encode(nextItemId);   
                        shortUrl.AssignShortUrl(result);

                        //assigning userId
                        if (!string.IsNullOrEmpty(userId))
                            shortUrl.UserId = userId;
                        
                        try
                        {
                            //adding it to the database
                            db.Urls.Add(shortUrl);
                            db.SaveChanges();

                            shortUrl.ShortUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/" + shortUrl.ShortUrl;

                            return Json(new { status = true, url = shortUrl }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception exc)
                        {
                            log.Error(exc);
                            return Json(new { status = false, message = exc.Message }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                        return Json(new { status = false, message = "Not valid URL provided" }, JsonRequestBehavior.AllowGet);
                } // if the long url already exists
                else
                {
                    existingURL.ShortUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/" + existingURL.ShortUrl;
                    return Json(new { status = true, url = existingURL }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult LongerURL(string shortUrl)
        {
            db.Configuration.ProxyCreationEnabled = false;

            if (string.IsNullOrEmpty(shortUrl))
                return Json(new { status = false, message = "Please provide URL" }, JsonRequestBehavior.AllowGet);
            else
            {
                //remove the service beginning part
                string begining = Request.Url.Scheme + "://" + Request.Url.Authority + "/";
                if(shortUrl.Contains(begining))
                {
                    shortUrl = shortUrl.Replace(begining, string.Empty);
                }

                //getting the long URL by location in the DB
                int location = URL.Decode(shortUrl);                            
                var existingURL = db.Urls.Where(u => u.UrlId == location).FirstOrDefault();

                if (existingURL != null)
                {
                    return Json(new { status = true, url = existingURL }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, message = "No corresponding long URL exists" }, JsonRequestBehavior.AllowGet);
                }

            }
        }
    }
}