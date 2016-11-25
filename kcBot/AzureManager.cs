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

        // AzureManager is a singleton class
        public static AzureManager AzureManagerInstance
        {
            get
            {
                // Checks whether the class has been instantiated
                if (instance == null) 
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }


        // Adds a new person to the bank database
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

        // Deletes an account from the bank database
        public async Task DeleteBankRecord(string Name)
        {
            moodTable bankRecord = (await bankRecordTable.Where(p => p.Name == Name).ToEnumerableAsync()).Single();
            await this.bankRecordTable.DeleteAsync(bankRecord);
        }

        // Updates the bank balance of a specific person in the database by adding or subtracting
        public async Task updateBalance(string Name, double amount)
        {
              
            // Finds the row in the database table where the name matches the given name
            moodTable bankRecord = (await bankRecordTable.Where(p => p.Name == Name).ToEnumerableAsync()).Single();

            if (bankRecord != null)
            {
                bankRecord.Balance += amount;
                await bankRecordTable.UpdateAsync(bankRecord);
            }
        }

        // Retrieves the bank balance of a specified person
        public async Task<double> getBalance(string Name)
        {
            moodTable bankRecord = (await bankRecordTable.Where(p => p.Name == Name).ToEnumerableAsync()).Single();

            if (bankRecord != null)
            {
                return bankRecord.Balance;
               
            }

            return 0;
        }

        //  Gets the password stored in the database
        public async Task<string> getPassword(string Name)
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
