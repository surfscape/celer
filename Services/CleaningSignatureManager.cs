using Celer.Properties;
using System.IO;
using System.Net.Http;

namespace Celer.Services
{
    public static class CleaningSignatureManager
    {
        private static readonly string LocalDbPath = "signatures.json";
        private static readonly string DownloadUrl = Signatures.Default.CleaningEngineMainSource;

        public static bool HasLocalDatabase() => File.Exists(LocalDbPath);

        public static async Task<bool> TryDownloadCleaningSignaturesAsync()
        {
            try
            {
                using HttpClient client = new();
                var response = await client.GetAsync(DownloadUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    await File.WriteAllTextAsync(LocalDbPath, json);
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
