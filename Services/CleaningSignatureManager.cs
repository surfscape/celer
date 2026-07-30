using Celer.Properties;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace Celer.Services
{
    public static class CleaningSignatureManager
    {
        private static readonly string InternalDbPath = "pack://application:,,,/Resources/Signatures/cleaning.json";
        private static readonly string LocalDbPath = ".\\Signatures\\cleaning.json";
        private static readonly string SignaturesUrl = Signatures.Default.CleaningEngineMainSource;

        public static bool HasLocalDatabase() => File.Exists(LocalDbPath);
        public static bool HasInternalDatabase() => Application.GetResourceStream(new Uri(InternalDbPath)) is not null;

        public static string GetSignatures()
        {
            if (!MainConfiguration.Default.CLEANENGINE_PreferInternalSignatures && File.Exists(LocalDbPath))
            {
                return File.ReadAllText(LocalDbPath);

            }
            else {
                Uri embeddedResource = new(InternalDbPath);
                var streamInfo = Application.GetResourceStream(embeddedResource);

                if (streamInfo != null)
                {
                    using StreamReader reader = new(streamInfo.Stream);
                    return reader.ReadToEnd();
                } else
                {
                    return string.Empty;
                }
            }
        }

        public static async Task<bool> TryDownloadCleaningSignaturesAsync()
        {
            try
            {
                using HttpClient client = new();
                var response = await client.GetAsync(SignaturesUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    if (!Directory.Exists(".\\Signatures")) { 
                        DirectoryInfo directoryInfo = Directory.CreateDirectory(".\\Signatures");
                    }
                    await File.WriteAllTextAsync(LocalDbPath, json);
                    return true;
                }
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine(e.Message);
            }
            return false;
        }
    }
}
