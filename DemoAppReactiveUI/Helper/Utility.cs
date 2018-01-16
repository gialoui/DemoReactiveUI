using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DemoAppReactiveUI.Helper
{
    internal static class UtilityNativeMethods
    {
        [DllImport("User32.dll")]
        internal static extern bool
        GetLastInputInfo(ref Utility.LASTINPUTINFO plii);
    }

    public static class Utility
    {
        // event tracker Log
        public static readonly ILog logger_eventracker = LogManager.GetLogger("[TrackerEvent]");

        // log
        public static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string CACHE_FOLDER = "cache";
        public static string IMAGE_CACHE_FOLDER = "images";
        public static string PRODUCT_CACHE_FOLDER = "product";
        public static string PRINTER_TEMPLATES_FOLDER = "printer_templates";
        public static string SETTINGS_FOLDER = "settings";
        public static string RESOURCES_FOLDER = "Resources";

        internal struct LASTINPUTINFO
        {
            public uint cbSize;

            public uint dwTime;
        }

        #region UTIL METHODS

        public static string HandleChineseNameForPrinting(this string str)
        {
            if (String.IsNullOrWhiteSpace(str) || !str.Any(c => c > 255)) return str;
            var diff = str.Where(c => c > 255).Count(); // Count unicode character in string
            return str.Remove(str.Length - diff);
        }

        public static string ConvertToServerTimeString(DateTime convertingTime)
        {
            return convertingTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        }

        public static void SortDataGrid(DataGrid dataGrid, int columnIndex = 0, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            var column = dataGrid.Columns[columnIndex];

            // Clear current sort descriptions
            dataGrid.Items.SortDescriptions.Clear();

            // Add the new sort description
            dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, sortDirection));

            // Apply sort
            foreach (var col in dataGrid.Columns)
            {
                col.SortDirection = null;
            }
            column.SortDirection = sortDirection;

            // Refresh items to display sort
            dataGrid.Items.Refresh();
        }

        /*
        Reference: http://stackoverflow.com/questions/10788982/is-there-any-async-equivalent-of-process-start
        */

        public static async Task<int> BackupDatabaseAsync(string saveFilePath)
        {
            // generate postgres logical backup
            var backupScriptPath = Path.Combine(Utility.GetResourcesPath(), "backup/postgresqlBackup.bat");
            using (var process = new Process
            {
                StartInfo =
                {
                    FileName = backupScriptPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = String.Format("\"{0}\"", saveFilePath)
                },
                EnableRaisingEvents = true
            })
            {
                return await RunProcessAsync(process).ConfigureAwait(false);
            }
        }

        /*
        Reference: http://stackoverflow.com/questions/10788982/is-there-any-async-equivalent-of-process-start
        */

        private static Task<int> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.Exited += (s, ea) => tcs.SetResult(process.ExitCode);
            //process.OutputDataReceived += (s, ea) => Console.WriteLine(ea.Data);
            //process.ErrorDataReceived += (s, ea) => Console.WriteLine("ERR: " + ea.Data);
            try
            {
                bool started = process.Start();
                if (!started)
                {
                    //you may allow for the process to be re-used (started = false)
                    //but I'm not sure about the guarantees of the Exited event in such a case
                    throw new InvalidOperationException("Could not start process: " + process);
                }
            }
            catch (Exception ex)
            {
                logger.Error("RunProcessAsync ERROR " + ex.Message);
            }
            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();

            return tcs.Task;
        }

        public static bool IsMasterTerminalByCheckingDatabaseHost()
        {
            return Properties.Settings.Default.databaseHost.ToLower() == "localhost";
        }

        public static bool IsSlaveTerminalByCheckingDatabaseHost()
        {
            return Properties.Settings.Default.databaseHost.ToLower() != "localhost";
        }

        public static bool ValidateURIAddress(string address)
        {
            if (address == null) return false;

            Uri myURI;
            if (Uri.TryCreate(address, UriKind.Absolute, out myURI) == false ||
                (myURI.Scheme != "http" && myURI.Scheme != "https") ||
                myURI.Host.IndexOf('.') < 0)
            {
                return false;
            }

            return true;
        }

        public static bool VerifyCheckSum(string barcode)
        {
            int checkSum = 0;
            var barchars = barcode.ToList();

            for (int position = 1; position < barchars.Count - 1; position += 2)
            {
                checkSum += barchars[position];
            }

            checkSum *= 3;

            for (int position = 0; position < barchars.Count - 1; position += 2)
            {
                checkSum += barchars[position];
            }

            checkSum %= 10;

            checkSum = 10 - checkSum;

            if (checkSum == 10) checkSum = 0;
            if (checkSum == barchars[12]) return true;
            return false;
        }

        #endregion UTIL METHODS

        #region IMAGE CACHING

        public static List<byte[]> GenerateCacheListFromImageFilePaths(string[] filePaths)
        {
            var imageByteList = new List<byte[]>();
            if (filePaths == null || filePaths.Length <= 0) return imageByteList;

            foreach (string file in filePaths)
            {
                byte[] a = FileToByteArray(file);
                imageByteList.Add(a);
            }

            return imageByteList;
        }

        public static byte[] FileToByteArray(string fileName)
        {
            byte[] fileData = null;

            using (FileStream fs = File.OpenRead(fileName))
            {
                var binaryReader = new BinaryReader(fs);
                fileData = binaryReader.ReadBytes((int)fs.Length);
            }
            return fileData;
        }

        #endregion IMAGE CACHING

        #region PATH

        public static JArray createDomainJArray(string name, string operatorStr, string value)
        {
            var array = new JArray();
            array.Add(name); array.Add(operatorStr); array.Add(value);
            return array;
        }

        public static string getCachePath()
        {
            var directory = System.AppDomain.CurrentDomain.BaseDirectory;
            //Console.WriteLine("App directory: " + directory);
            var cachePath = Path.Combine(directory, CACHE_FOLDER);
            return cachePath;
        }

        public static string getProductImageCachePath()
        {
            var directory = System.AppDomain.CurrentDomain.BaseDirectory;
            string productImageCachePath = Path.Combine(directory, CACHE_FOLDER + "/" + IMAGE_CACHE_FOLDER + "/" + PRODUCT_CACHE_FOLDER);
            return productImageCachePath;
        }

        public static string GetResourcesPath()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            string resourcesPath = Path.Combine(directory, RESOURCES_FOLDER);
            return resourcesPath;
        }

        public static string getSettingsPath()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            string settingPath = Path.Combine(directory, SETTINGS_FOLDER);
            return settingPath;
        }

        public static string getProductImageCacheFilePath(string imageURI)
        {
            var directory = getProductImageCachePath();
            var fileName = getHashCode(imageURI);
            string filePath = Path.Combine(directory, fileName);

            return filePath;
        }

        public static string getPrinterTemplatesPath()
        {
            var templatesPath = @"Resources/printer_templates";
            return templatesPath;
        }

        public static bool doesProductCacheImageExist(string imageURI)
        {
            var imageFilePath = getProductImageCacheFilePath(imageURI);
            if (File.Exists(imageFilePath))
                return true;
            else return false;
        }

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        public static void ShowErrorMessageWithSound(string title, string message)
        {
            // play error sound
            SoundPlayer simpleSound = new SoundPlayer(@"Resources/sounds/error_sound.wav");
            simpleSound.Play();

            MessageBox.Show(message, title, MessageBoxButton.OK);
        }

        #endregion PATH

        #region JSON_Serialize

        /// <summary>
        /// Writes the given object instance to a Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            // Ensure settings path existed before save to json
            string settingsPath = Utility.getSettingsPath();
            if (Directory.Exists(settingsPath) == false)
            {
                Directory.CreateDirectory(settingsPath);
            }

            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite,
                    Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver() });
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the Json file.</returns>
        public static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
                var fileContents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(fileContents,
                    new JsonSerializerSettings { ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver() });
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        /// <summary>
        /// use this custom DefaultContractResolver to Ignore [JsonIgnore] Attribute on Serialization / Deserialization
        /// </summary>
        public class JsonIgnoreAttributeIgnorerContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                property.Ignored = false; // Here is the magic
                return property;
            }
        }

        #endregion JSON_Serialize

        #region HELPER

        public static string ConvertPropertyNameToDBName(string propertyName)
        {
            if (propertyName == "barcodeEx1") return "barcode_extra1";
            if (propertyName == "barcodeEx2") return "barcode_extra2";
            if (propertyName == "barcodeEx") return "barcode_extra";
            if (propertyName == "barcodeEx3") return "barcode_extra3";
            if (propertyName == "price") return "unit_price";
            return Regex.Replace(Regex.Replace(propertyName, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1_$2"), @"(\p{Ll})(\P{Ll})", "$1_$2").ToLower();
        }

        public static App GetApplication()
        {
            var myApp = ((App)Application.Current);
            return myApp;
        }

        public static string getHashCode(string sourceString)
        {
            string hashCode = String.Format("{0:X}", sourceString.GetHashCode());
            return hashCode;
        }

        public static TChildItem FindVisualChild<TChildItem>(DependencyObject obj) where TChildItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is TChildItem)
                    return (TChildItem)child;

                var childOfChild = FindVisualChild<TChildItem>(child);

                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }

        /*
        When data return from odoo server. There will be optional field
        which will return "false" if the field is empty
            */

        public static string GetStringValueForKey(object obj, string key)
        {
            try
            {
                object value = null;
                DataRow dataRow = obj as DataRow;
                if (dataRow != null) value = dataRow[key];
                JObject jObject = obj as JObject;
                if (jObject != null) value = jObject[key];

                if (value == null) return "";

                var stringValue = value.ToString();
                var boolValue = false;
                if (bool.TryParse(stringValue, out boolValue) == false)
                {
                    return stringValue;
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                //Console.WriteLine("GetStringValueForKey ERROR" + ex.Message);
            }

            return "";
        }

        public static Guid? GetUUIDValueForKey(object obj, string key)
        {
            try
            {
                object value = null;
                DataRow dataRow = obj as DataRow;
                if (dataRow != null) value = dataRow[key];
                JObject jObject = obj as JObject;
                if (jObject != null) value = jObject[key];

                if (value == null) return null;

                var stringValue = value.ToString();
                Guid boolValue;
                if (Guid.TryParse(stringValue, out boolValue))
                {
                    if (boolValue.ToString() == "00000000-0000-0000-0000-000000000000")
                        return null;
                    return boolValue;
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                //Console.WriteLine("GetStringValueForKey ERROR" + ex.Message);
            }

            return null;
        }

        public static int GetIntValueForKey(object obj, string key)
        {
            try
            {
                object value = null;
                DataRow dataRow = obj as DataRow;
                if (dataRow != null) value = dataRow[key];
                JObject jObject = obj as JObject;
                if (jObject != null) value = jObject[key];

                if (value == null) return -1;

                var stringValue = value.ToString();
                var boolValue = false;
                if (bool.TryParse(stringValue, out boolValue) == false)
                {
                    return int.Parse(stringValue);
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                //Console.WriteLine("GetIntValueForKey: " + key + " ERROR: " + ex.Message);
            }
            return -1;
        }

        public static Int64 GetInt64ValueForKey(object obj, string key)
        {
            try
            {
                object value = null;
                DataRow dataRow = obj as DataRow;
                if (dataRow != null) value = dataRow[key];
                JObject jObject = obj as JObject;
                if (jObject != null) value = jObject[key];

                if (value == null) return -1;

                var stringValue = value.ToString();
                var boolValue = false;
                if (bool.TryParse(stringValue, out boolValue) == false)
                {
                    return Int64.Parse(stringValue);
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                //Console.WriteLine("GetIntValueForKey: " + key + " ERROR: " + ex.Message);
            }
            return -1;
        }

        public static bool GetBoolValueForKey(object obj, string key)
        {
            try
            {
                object value = null;
                DataRow dataRow = obj as DataRow;
                if (dataRow != null) value = dataRow[key];
                JObject jObject = obj as JObject;
                if (jObject != null) value = jObject[key];

                if (value == null) return false;

                var stringValue = value.ToString();
                return bool.Parse(stringValue);
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                //Console.WriteLine("GetBoolValueForKey: " + key + " ERROR: " + ex.Message);
            }
            return false;
        }

        public static double GetDoubleValueForKey(object obj, string key)
        {
            try
            {
                object value = null;
                DataRow dataRow = obj as DataRow;
                if (dataRow != null) value = dataRow[key];
                JObject jObject = obj as JObject;
                if (jObject != null) value = jObject[key];

                if (value == null) return 0;

                var stringValue = value.ToString();
                var boolValue = false;
                if (bool.TryParse(stringValue, out boolValue) == false)
                {
                    return double.Parse(stringValue);
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                //Console.WriteLine("GetDoubleValueForKey: " + key + " ERROR: " + ex.Message);
            }

            return 0;
        }

        public static decimal GetDecimalValueForKey(object obj, string key)
        {
            try
            {
                object value = null;
                if (obj is DataRow dataRow) value = dataRow[key];
                if (obj is JObject jObject) value = jObject[key];

                if (value == null) return 0;

                var stringValue = value.ToString();
                if (bool.TryParse(stringValue, out bool boolValue) == false)
                {
                    return Convert.ToDecimal(stringValue);
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                //Console.WriteLine("GetDoubleValueForKey: " + key + " ERROR: " + ex.Message);
            }

            return 0;
        }

        public static JArray GetJArrayFromJObjectWithKey(JObject jObj, string key)
        {
            var property = jObj.Property(key);
            //check if property exists
            if (property != null)
            {
                var array = property.Value as JArray;
                return array;
            }
            else
            {
                //there is no "msg" property, compensate somehow.
                return null;
            }
        }

        public static UserControl GetParentForUserControl(UserControl control)
        {
            DependencyObject ucParent = control.Parent;

            while (ucParent != null && !(ucParent is UserControl))
            {
                ucParent = LogicalTreeHelper.GetParent(ucParent);
            }

            return (UserControl)ucParent;
        }

        public static object ConvertStringToDate(string dateTimeString)
        {
            try
            {
                DateTime myDate = Convert.ToDateTime(dateTimeString);
                return myDate;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning disable 0168
            {
                //Console.WriteLine("ConvertStringToDate: " + dateTimeString + " format: " + format + " ERROR: " + ex.Message);
            }

            return null;
        }

        public static object ConvertGMTDateStringToLocalDate(string dateTimeString)
        {
            try
            {
                DateTime convertedDate = DateTime.SpecifyKind(
                    DateTime.Parse(dateTimeString),
                    DateTimeKind.Utc
                );
                return convertedDate.ToLocalTime();
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning disable 0168
            {
                //Console.WriteLine("ConvertStringToDate: " + dateTimeString + " format: " + format + " ERROR: " + ex.Message);
            }

            return null;
        }

        public static string GetStringFromDate(DateTime dt)
        {
            return String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
        }

        public static string GetStringFromDate(DateTime dt, string formatString)
        {
            return String.Format(formatString, dt);
        }

        public static string EscapeQueryString(string query)
        {
            return query.Replace("'", "''");
        }

        public static double ParseDouble(string text)
        {
            var validText = double.TryParse(text, out double value);
            return validText ? value : 0.0;
        }

        public static int ParseInt(string text)
        {
            try
            {
                return int.Parse(text);
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                return 0;
            }
        }

        public static string TruncateString(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars);
        }

        public static string FormatNumberForReport(double number, bool withDecimal = true)
        {
            if (number == 0) return "-";
            else if (number < 0)
            {
                if (withDecimal) return "(" + Math.Abs(number).ToString("0.00") + ")";
                else return "(" + Math.Abs(number).ToString("0") + ")";
            }
            else
            {
                if (withDecimal) return number.ToString("0.00");
                else return number.ToString("0");
            }
        }

        public static uint GetIdleTime()
        {
            uint idleTime = 0;
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);
            lastInPut.dwTime = 0;

            uint envTicks = (uint)Environment.TickCount;
            if (UtilityNativeMethods.GetLastInputInfo(ref lastInPut))
            {
                uint lastInputTick = lastInPut.dwTime;

                idleTime = envTicks - lastInputTick;
            }

            return ((idleTime > 0) ? (idleTime / 1000) : 0);
        }

        public static bool IsKeyADigit(Key key)
        {
            return (key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9);
        }

        public static string FixedLengthString(string str, int fixedLength)
        {
            string formatString = TruncateString(str, fixedLength);
            return formatString.PadRight(fixedLength);
        }

        public static string FixedLengthStringNumber(string str, int fixedLength)
        {
            string formatString = TruncateString(str, fixedLength);
            return formatString.PadLeft(fixedLength, '0');
        }

        public static string GetDictionaryValueByKey(int key, Dictionary<int, string> myDict)
        {
            bool hasValue = myDict.TryGetValue(key, out string value);
            if (hasValue) return value;
            return "N/A";
        }

        public static string FormatNumberForMallReport(string number)
        {
            // for numeric(10,2)
            var maxValue = 9999999999;
            var newNumber = Convert.ToDouble(number);
            if (Math.Abs(newNumber) > maxValue)
            {
                MessageBox.Show("Some Value is greater than maximum allowed!!! Value will be set to 0");
                return "0";
            }
            return string.Format("{0:0.00}", newNumber);
        }

        public static T ConvertFromDBVal<T>(object obj)
        {
            // if value is null returns the default value for the type
            if (obj == null || obj == DBNull.Value)
            {
                return default(T);
            }
            else
            {
                return (T)obj;
            }
        }

        /// <summary>
        /// Convert Boolean to String value
        /// </summary>
        /// <param name="value">"1" => True, "0" => False</param>
        /// <returns></returns>
        public static string ConvertBoolToString(bool value)
        {
            if (value) return "1";
            return "0";
        }

        /// <summary>
        /// Convert Boolean to String value
        /// </summary>
        /// <param name="value">True => "1", False => "0"</param>
        /// <returns></returns>
        public static bool ConverStringToBool(string value)
        {
            if (value == "1") return true;
            return false;
        }

        public static List<string> GetPropertiesDescription<T>()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            return properties.Select(p => Attribute.IsDefined(p, typeof(DescriptionAttribute)) ?
                (Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute)) as DescriptionAttribute).Description : p.Name)
                .ToList();
        }

        public static List<string> GetPropertiesName<T>()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            return properties.Select(p => p.Name).ToList();
        }

        public static Dictionary<string, dynamic> GetPropertiesDict<T>(T obj)
        {
            var dict = new Dictionary<string, dynamic>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(inherit: false).Count() == 0) continue;
                var propertyDescription = GetPropertyDescription(property);
                dict.Add(propertyDescription, property.GetValue(obj, null));
            }
            return dict;
        }

        private static string GetPropertyDescription(PropertyInfo property)
        {
            var attribute = property.GetCustomAttributes(typeof(DescriptionAttribute), true)?[0];
            var description = (DescriptionAttribute)attribute;
            return description.Description;
        }

        public static string ConvertPaymentTypeForMarinaReport(string journal_id)
        {
            switch (journal_id)
            {
                case "1": return "1";
                case "5": return "5";
                case "2": return "8";
                case "3": return "8";
                case "6": return "7";
                case "4": return "6";
                default: return "0";
            }
        }

        public static string ConvertPaymentTypeForCustomReport(string journal_id)
        {
            switch (journal_id)
            {
                case "0": return "Other";
                case "10": return "Cash";
                case "11": return "Cashcard";
                case "12": return "CreditCard";
                case "13": return "EZLink";
                case "14": return "Nets";
                case "15": return "Voucher";
                default: return "8";
            }
        }

        public static string CalculateGST(string amount, string tax_applied)
        {
            var price = Convert.ToDouble(amount);
            var taxApplied = Convert.ToDouble(tax_applied);
            var gst = price - price / (1 + taxApplied / 100);
            return string.Format("{0:0.00}", gst);
        }

        public static double CalculateTaxAmount(double price, double taxValue)
        {
            var priceBeforeTax = (price / (1 + taxValue));
            return price - priceBeforeTax;
        }

        public static string FindIpFromString(string sourceString)
        {
            var match = Regex.Match(sourceString, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
            if (match.Success) return match.Captures[0].Value;
            else return null;
        }

        #endregion HELPER

        #region CALCULATION

        public static double RoundTotalPriceToNearestValue(double floatNumber, double roundToNearestValue)
        {
            // if round to nearestvalue setup wrong. We just return the input value
            if (roundToNearestValue <= 0.0) return floatNumber;

            var shouldRoundUp = Properties.Settings.Default.shouldRoundUp;
            /*
             * When dealing with negative floatNumber, we have to revert ShouldRoundUp
             * so that the rounding amount will be accurate
             * To support Return product and rounding: http://pm.floatingcube.com/T1763
             */
            if (floatNumber < 0) shouldRoundUp = !shouldRoundUp;
            roundToNearestValue = Math.Round(roundToNearestValue, 2);
            var roundValue = 1 / roundToNearestValue;
            //logger.Debug("roundValue: " + roundValue);

            // final number
            var finalNumber = 0.0;
            if (shouldRoundUp) finalNumber = Math.Ceiling(floatNumber * roundValue) / roundValue;
            else finalNumber = Math.Floor(floatNumber * roundValue) / roundValue;
            //logger.Debug("input number: " + floatNumber + " shouldRoundUp:" + shouldRoundUp + " ---- finalNumber: " + finalNumber);
            return finalNumber;
        }

        public static double RoundDoubleValue(double value, int decimalNumber)
        {
            return Math.Round(value, decimalNumber, MidpointRounding.AwayFromZero);
        }

        public static string RoundDoubleValueWithFixedTrailingNumber(double value, int decimalNumber)
        {
            return Math.Round(value, decimalNumber, MidpointRounding.AwayFromZero).ToString("F" + decimalNumber);
        }

        public static string FormatDoubleWithOptionalDecimalNumbers(double value)
        {
            var result = "";
            var roundValue = (int)value;
            // if the original value has decimal number --> round it down to 2 number only
            if (Math.Abs(value - roundValue) > 0.01) result = value.ToString("0.00");
            else result = roundValue.ToString(); // else just return as integer

            return result;
        }

        public static double CalculateSellPriceWithoutGST(double productSellPrice)
        {
            var result = productSellPrice;
            if (Properties.Settings.Default.taxEnable == true)
            {
                // remember to divide to 100 to get the real GST value
                var TaxValue = Properties.Settings.Default.taxValue / 100.0;
                // we have to calculate the price w/o GST
                result /= 1 + TaxValue;
            }

            return result;
        }

        public static double CalculateRealCostPrice(double cost, bool taxApplied)
        {
            var realCost = cost;
            /*
            For store with GST enable, the realCost price will always be the same as inputed cost.
            */
            if (taxApplied == false || Properties.Settings.Default.taxEnable == true)
            {
                return realCost;
            }
            else if (taxApplied)
            {
                /*
                For store without GST, the realcost with tax applied will be
                = inputed cost + GST value
                */
                // remember to divide to 100 to get the real Tax value
                var TaxValue = Properties.Settings.Default.taxValue / 100.0;
                // calculate real cost with with Tax applied
                realCost = cost * (1 + TaxValue);
            }

            return realCost;
        }

        #endregion CALCULATION

        #region ENUM

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static List<string> GetEnumDescriptions<T>()
        {
            var enumList = Enum.GetValues(typeof(T)).Cast<T>()?.ToList();
            var list = new List<string>();
            foreach (var item in enumList)
            {
                list.Add(Utility.GetEnumDescription(item as Enum));
            }
            return list;
        }

        public static T GetEnumFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            return default(T);
        }

        #endregion ENUM
    }
}