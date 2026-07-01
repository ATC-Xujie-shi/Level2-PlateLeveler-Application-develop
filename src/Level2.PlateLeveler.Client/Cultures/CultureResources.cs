using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Forms;

namespace Level2.PlateLeveler.Client.Cultures {
    /// <summary>
    /// Wraps up XAML access to instance of WPFLocalize.Properties.Resources, list of available cultures, and method to change culture
    /// </summary>
    public class CultureResources {
        //only fetch installed cultures once
        private static bool bFoundInstalledCultures;

        /// <summary>
        /// List of available cultures, enumerated at startup
        /// </summary>
        public static List<CultureInfo> SupportedCultures { get; } = [];

        public CultureResources() {
            if (!bFoundInstalledCultures) {
                //determine which cultures are available to this application
                Debug.WriteLine("Get Installed cultures:");
                _ = new CultureInfo("");
                foreach (var dir in Directory.GetDirectories(Application.StartupPath)) {
                    try {
                        //see if this directory corresponds to a valid culture name
                        var dirinfo = new DirectoryInfo(dir);
                        var tCulture = CultureInfo.GetCultureInfo(dirinfo.Name);

                        //determine if a resources dll exists in this directory that matches the executable name
                        if (dirinfo.GetFiles(Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".resources.dll").Length > 0) {
                            SupportedCultures.Add(tCulture);
                            Debug.WriteLine(string.Format(" Found Culture: {0} [{1}]", tCulture.DisplayName, tCulture.Name));
                        }
                    } catch (ArgumentException) //ignore exceptions generated for any unrelated directories in the bin folder
                      {
                    }
                }
                bFoundInstalledCultures = true;
            }
        }

        /// <summary>
        /// The Resources ObjectDataProvider uses this method to get an instance of the WPFLocalize.Properties.Resources class
        /// </summary>
        /// <returns></returns>
        public Resources GetResourceInstance() => new();

        public static ObjectDataProvider ResourceProvider {
            get {
                field ??= (ObjectDataProvider)System.Windows.Application.Current.FindResource("Resources");

                return field;
            }
        }

        /// <summary>
        /// Change the current culture used in the application.
        /// If the desired culture is available all localized elements are updated.
        /// </summary>
        /// <param name="culture">Culture to change to</param>
        public static void ChangeCulture(CultureInfo culture) {
            //remain on the current culture if the desired culture cannot be found
            // - otherwise it would revert to the default resources set, which may or may not be desired.
            if (SupportedCultures.Contains(culture)) {
                Resources.Culture = culture;
                ResourceProvider.Refresh();
            } else {
                Debug.WriteLine(string.Format("Culture [{0}] not available", culture));
            }
        }
    }
}
