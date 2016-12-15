using BatteryWebApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BatteryWebApp.Models;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace BatteryWebApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            MLResponse result = await HelperClasses.BatteryMLService.GetMLResponse();
            storeResponseOutput(result);
            List<ResponseOutput> chartData = getPlotPoints();
            HomeViewModel hvm = new HomeViewModel
            {
                Response = result,
                chartData = chartData
            };
            return View(hvm);
        }

        private List<ResponseOutput> getPlotPoints()
        {
            HttpHelpers.DataAccess dataAccess = new HttpHelpers.DataAccess();
            List<ResponseOutput> responseItems = dataAccess.getResponseItems();
            return responseItems;
        }

        private void storeResponseOutputs(List<MLResponse> results)
        {
            for(int i = 0; i < results.Count; i++)
            {
                storeResponseOutput(results[i]);
            }
        }

        private void storeResponseOutput(MLResponse result)
        {
            HttpHelpers.DataAccess dataAccess = new HttpHelpers.DataAccess();
            String[] resultsHolder = result.Response.Split(':');
            double eval = 0.0;
            double pred = 0.0;
            for (int i = 0; i < resultsHolder.Length; i++)
            {
                string temp = resultsHolder[i];
                string extract = Regex.Match(temp, @"-?\d+(.\d+)?").Value;

                if (!String.IsNullOrEmpty(extract))
                {
                    if (extract.Contains('-'))
                    {
                        try
                        {
                            pred = Double.Parse(extract);
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            eval = Double.Parse(extract);
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                    }
                    
                }
            }
            ResponseOutput responseOutput = new ResponseOutput {
                DateStamp = result.dateStamp,
                evaluation = eval,
                prediction = pred
            };

            dataAccess.storeNewRow(responseOutput);
        }

        

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}