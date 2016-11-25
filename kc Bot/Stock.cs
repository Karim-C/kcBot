using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace kcBot
{
    public class Sotck
    {
        public static async Task<string> getStockAsync(string tickerSymbol)
        {
            string returnString = "";

            if (string.IsNullOrWhiteSpace(tickerSymbol))
            {
                return "Please enter a ticker symbol e.g. MSFT for Microsoft Corporation";
            }

            string urlPrice = $"http://finance.yahoo.com/d/quotes.csv?s={tickerSymbol}&f=sl1";
            string urlFullName = $"http://finance.yahoo.com/d/quotes.csv?s={tickerSymbol}&f=n";

            string csvPrice, csvName;
            using (WebClient client = new WebClient())
            {
                csvPrice = await client.DownloadStringTaskAsync(urlPrice).ConfigureAwait(false);
                csvName = await client.DownloadStringTaskAsync(urlFullName).ConfigureAwait(false);
            }
            string linePrice = csvPrice.Split('\n')[0];
            string fullName = csvName.Split('\n')[0];
            string price = linePrice.Split(',')[1];
            //string fullName = lineName.Split(',')[1];

            if (price == "N/A")
            {
                returnString = "The ticker symbol " + tickerSymbol.ToUpper() + " is not valid";
            }
            else
            {
                returnString = "Stock: " + fullName + " (" + tickerSymbol.ToUpper() + ") Price: " + price;
            }

            return returnString;
        }
    }
    
}