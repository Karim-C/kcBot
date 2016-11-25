using Microsoft.WindowsAzure.MobileServices;
using kcBot.DataModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kcBot
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<moodTable> bankRecordTable;

        private AzureManager()
        {

            this.client = new MobileServiceClient("http://moodtime.azurewebsites.net");
            this.bankRecordTable = this.client.GetTable<moodTable>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task AddBankRecord(string userName, string password)
        {
            moodTable record = new moodTable()
            {
                Name = userName,
                Balance = 0,
                Password = password
            };

            await this.bankRecordTable.InsertAsync(record);
        }

        public async Task DeleteBankRecord(string Name)
        {
            moodTable bankRecord = (await bankRecordTable.Where(p => p.Name == Name).ToEnumerableAsync()).Single();
            await this.bankRecordTable.DeleteAsync(bankRecord);
        }

        public async Task updateBalance(string Name, double amount)
        {
              

            moodTable bankRecord = (await bankRecordTable.Where(p => p.Name == Name).ToEnumerableAsync()).Single();

            if (bankRecord != null)
            {
                bankRecord.Balance += amount;
                await bankRecordTable.UpdateAsync(bankRecord);
            }
        }

        public async Task<double> getBalance(string Name)
        {
            moodTable bankRecord = (await bankRecordTable.Where(p => p.Name == Name).ToEnumerableAsync()).Single();

            if (bankRecord != null)
            {
                return bankRecord.Balance;
               
            }

            return 0;
        }

        public async Task<string> getPassward(string Name)
        {
            moodTable bankRecord = (await bankRecordTable.Where(p => p.Name == Name).ToEnumerableAsync()).Single();

            if (bankRecord != null)
            {
                return bankRecord.Password;

            }

            return "";
        }

        public async Task<List<moodTable>> GetBankRecords()
        {
            return await this.bankRecordTable.ToListAsync();
        }
    }
}
