using System;
using System.Configuration;
using System.Data.SqlClient;

public static class Logger
{
    private static string connectionString = ConfigurationManager.ConnectionStrings["Haier_DB"].ConnectionString;


    public static void Log(string action, int? userId = null) // int? : Bu bir nullable int yani int olabilir veya null olabilir.
    {
        // Konsola da yazmak için
        Console.WriteLine($"{DateTime.Now}: {action}");

        // Veritabanına log yaz
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "INSERT INTO Logs (UserId, Action, Timestamp) VALUES (@UserId, @Action, @Timestamp)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId.HasValue ? (object)userId.Value : DBNull.Value); // userId varsa → değerini alır. Yoksa(null) → DBNull.Value gönderir.
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            //DB çökerse
            Console.WriteLine($"Logging failed: {ex.Message}");
        }
    }
}
