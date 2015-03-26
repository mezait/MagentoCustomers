using System;
using System.Configuration;
using System.Web.Mvc;
using MagentoCustomers.local.magento.v2;
using PagedList;

namespace MagentoCustomers.Controllers
{
    public class CustomerController : Controller
    {
        /// <summary>
        /// Get a Magento session ID
        /// </summary>
        /// <param name="client">Magento WS-I client</param>
        /// <returns>Magento session ID</returns>
        private static string GetSessionID(Mage_Api_Model_Server_Wsi_HandlerPortTypeClient client)
        {
            // Check for credential  authentication
            if (client.ClientCredentials != null &&
                !String.IsNullOrEmpty(ConfigurationManager.AppSettings["domain:Username"]) &&
                !String.IsNullOrEmpty(ConfigurationManager.AppSettings["domain:Password"]))
            {
                client.ClientCredentials.UserName.UserName = ConfigurationManager.AppSettings["domain:Username"];
                client.ClientCredentials.UserName.Password = ConfigurationManager.AppSettings["domain:Password"];
            }

            return client.login(
                ConfigurationManager.AppSettings["service:Username"],
                ConfigurationManager.AppSettings["service:Password"]);
        }

        //
        // GET: /Customer/

        public ActionResult Index(int? page, string surname)
        {
            var pageNum = (page ?? 1);
            var pageSize = Int32.Parse(ConfigurationManager.AppSettings["pageSize"]);

            var filters = !String.IsNullOrEmpty(surname)
                ? new[]
                {
                    new[]
                    {
                        new complexFilter
                        {
                            key = "lastname",
                            value = new associativeEntity {key = "eq", value = surname}
                        }
                    }
                }
                : null;

            using (var client = new Mage_Api_Model_Server_Wsi_HandlerPortTypeClient())
            {
                var sessionId = GetSessionID(client);

                var list = client.mezaitCustomerAddressList(sessionId, filters, pageNum, pageSize);
                var count = client.mezaitCustomerAddressCount(sessionId, filters);

                client.endSession(sessionId);

                var customers =
                    new StaticPagedList<mezaitCustomerAddress>(
                        list,
                        pageNum,
                        pageSize,
                        count);

                ViewBag.Count = count;
                ViewBag.Surname = surname;

                return View(customers);
            }
        }
    }
}