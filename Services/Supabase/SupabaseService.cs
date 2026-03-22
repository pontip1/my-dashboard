using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDashboard.Services.Backend
{
    public static class SupabaseService
    {
        public static Supabase.Client Client { get; private set; }

        public static async Task Initialize()
        {
            Client = new Supabase.Client(
                SupabaseSettings.Url,
                SupabaseSettings.Key
            );

            await Client.InitializeAsync();
        }
    }
}
