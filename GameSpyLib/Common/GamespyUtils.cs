﻿using GameSpyLib.Extensions;
using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GameSpyLib.Common
{
    public static class GameSpyUtils
    {
        /// <summary>
        /// Encodes a password to Gamespy format
        /// </summary>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static string EncodePassword(string Password)
        {
            // Get password string as UTF8 String, Convert to Base64
            byte[] PasswordBytes = Encoding.UTF8.GetBytes(Password);
            string Pass = Convert.ToBase64String(GsPassEncode(PasswordBytes));

            // Convert Standard Base64 to Gamespy Base 64
            StringBuilder builder = new StringBuilder(Pass);
            builder.Replace('=', '_');
            builder.Replace('+', '[');
            builder.Replace('/', ']');
            return builder.ToString();
        }

        /// <summary>
        /// Decodes a Gamespy encoded password
        /// </summary>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static string DecodePassword(string password)
        {
            // Convert Gamespy Base64 to Standard Base 64
            StringBuilder builder = new StringBuilder(password);
            builder.Replace('_', '=');
            builder.Replace('[', '+');
            builder.Replace(']', '/');

            // Decode passsword
            byte[] passwordBytes = Convert.FromBase64String(builder.ToString());
            return Encoding.UTF8.GetString(GsPassEncode(passwordBytes));
        }

        /// <summary>
        /// Gamespy's XOR method to encrypt and decrypt a password
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        public static byte[] GsPassEncode(byte[] pass)
        {
            int a = 0;
            int num = 0x79707367; // gspy
            for (int i = 0; i < pass.Length; ++i)
            {
                num = Gslame(num);
                a = num % 0xFF;
                pass[i] ^= (byte)a;
            }

            return pass;
        }

        /// <summary>
        /// Not exactly sure what this does, but i know its used to 
        /// reverse the encryption and decryption of a string
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private static int Gslame(int num)
        {
            int c = (num >> 16) & 0xffff;
            int a = num & 0xffff;

            c *= 0x41a7;
            a *= 0x41a7;
            a += ((c & 0x7fff) << 16);

            if (a < 0)
            {
                a &= 0x7fffffff;
                a++;
            }

            a += (c >> 15);

            if (a < 0)
            {
                a &= 0x7fffffff;
                a++;
            }

            return a;
        }


        /// </summary>
        /// Converts a trimmed presence message from the client string to a keyValue pair dictionary
        /// </summary>
        /// <param name="parts">The array of data from the client</param>
        /// <returns>A converted dictionary</returns>
        public static Dictionary<string, string> ConvertRequestToKeyValue(string[] parts)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            try
            {
                for (int i = 0; i < parts.Length; i += 2)
                {
                    if (!data.ContainsKey(parts[i]))
                        data.Add(parts[i].ToLower(), parts[i + 1]);//Some game send uppercase key to us, so we have to deal with it
                }
            }
            catch (IndexOutOfRangeException) { }
            ProcessPasswordInRetroSpyWay(data);
            return data;
        }
        /// <summary>
        /// Encode password in our way, so we do not need to care about the encoding method of password
        /// </summary>
        /// <param name="dict"></param>
        public static void ProcessPasswordInRetroSpyWay(Dictionary<string, string> dict)
        {
            string password;
            if (dict.ContainsKey("passwordenc"))
            {
                //we do nothing with encoded password                
                password = DecodePassword(dict["passwordenc"]);
                dict.Add("passenc", StringExtensions.GetMD5Hash(password));
            }
            if (dict.ContainsKey("passenc"))
            {
                //we do nothing with encoded password                
                password = DecodePassword(dict["passenc"]);
                dict["passenc"] = StringExtensions.GetMD5Hash(password);
            }
            if (dict.ContainsKey("pass"))
            {
                password = StringExtensions.GetMD5Hash(dict["pass"]);
                dict.Add("passenc", password);
            }
            if (dict.ContainsKey("password"))
            {
                password = StringExtensions.GetMD5Hash(dict["password"]);
                dict.Add("passenc", password);
            }
        }

        public static void PrintReceivedGPDictToLogger(Dictionary<string, string> recv)
        {
            ServerManager.LogWriter.Log.Error("Received request {0} with content: {1}", recv.Keys.First(), string.Join(";", recv.Select(x => x.Key + "=" + x.Value).ToArray()));
        }

        /// <summary>
        /// Send a presence error
        /// </summary>
        /// <param name="stream">The stream that will receive the error</param>
        /// <param name="code">The error code</param>
        /// <param name="error">A string containing the error</param>
        public static void SendGPError(TcpSession session, object errorCode, string error)
        {
            string sendingBuffer = string.Format(@"\error\\err\{0}\fatal\\errmsg\{1}\id\1\final\", (uint)errorCode, error);
            session.SendAsync(sendingBuffer);
        }

        /// <summary>
        /// Check the correctness of the email account format.
        /// </summary>
        /// <param name="email">email account</param>
        /// <returns></returns>
        public static bool IsEmailFormatCorrect(string email)
        {

            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                ServerManager.LogWriter.Log.Error(e.ToString());
                return false;
            }
            catch (ArgumentException e)
            {
                ServerManager.LogWriter.Log.Error(e.ToString());
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            //return email.Any(ch => (!char.IsLetterOrDigit(ch) && ch != '@' && ch != '.'));
        }
        /// <summary>
        /// Check is the format of nick or uniquenick are correct
        /// </summary>
        /// <param name="nick"></param>
        /// <returns></returns>
        public static bool IsNickOrUniquenickFormatCorrect(string nick)
        {
            string pattern = @"^\w+$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(nick);
        }

        /// <summary>
        /// Check if a date is correct
        /// </summary>
        /// <param name="day"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <returns>True if the date is valid, otherwise false</returns>
        public static bool IsValidDate(int day, int month, int year)
        {
            // Check for a blank.
            /////////////////////
            if ((day == 0) && (month == 0) && (year == 0))
                return false;

            // Validate the day of the month.
            /////////////////////////////////
            switch (month)
            {
                // No month.
                ////////////
                case 0:
                    // Can't specify a day without a month.
                    ///////////////////////////////////////
                    if (day != 0)
                        return false;
                    break;

                // 31-day month.
                ////////////////
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    if (day > 31)
                        return false;
                    break;

                // 30-day month.
                ////////////////
                case 4:
                case 6:
                case 9:
                case 11:
                    if (day > 30)
                        return false;
                    break;

                // 28/29-day month.
                ///////////////////
                case 2:
                    // Leap year?
                    /////////////
                    if ((((year % 4) == 0) && ((year % 100) != 0)) || ((year % 400) == 0))
                    {
                        if (day > 29)
                            return false;
                    }
                    else
                    {
                        if (day > 28)
                            return false;
                    }
                    break;

                // Invalid month.
                /////////////////
                default:
                    return false;
            }

            // Check that the date is in the valid range.
            /////////////////////////////////////////////
            if (year < 1900)
                return false;
            if (year > 2079)
                return false;
            if (year == 2079)
            {
                if (month > 6)
                    return false;
                if ((month == 6) && (day > 6))
                    return false;
            }

            return true;
        }
        public static bool IsNumber(string strNumber)
        {
            Regex objNotNumberPattern = new Regex("[^0-9.-]");
            Regex objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
            Regex objTwoMinusPattern = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
            string strValidRealPattern = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
            string strValidIntegerPattern = "^([-]|[0-9])[0-9]*$";
            Regex objNumberPattern = new Regex("(" + strValidRealPattern + ")|(" + strValidIntegerPattern + ")");

            return !objNotNumberPattern.IsMatch(strNumber) &&
                   !objTwoDotPattern.IsMatch(strNumber) &&
                   !objTwoMinusPattern.IsMatch(strNumber) &&
                   objNumberPattern.IsMatch(strNumber);
        }
    }
}
