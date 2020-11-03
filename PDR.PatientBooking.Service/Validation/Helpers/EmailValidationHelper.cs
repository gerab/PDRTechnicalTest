using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PDR.PatientBooking.Service.Validation.Helpers
{
    public static class EmailValidationHelper
    {
        /// <summary>
        /// A helper method to test an email is valid.
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format"/>>
        /// <param name="email">An email as a unit of check.</param>
        /// <param name="result">A validation result.</param>
        /// <returns></returns>
        public static void CheckEmailIsValid(string email, ref PdrValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                EnrichFailedResult(ref result);
                return;
            }

            try
            {
                // Normalize the domain
                email = Regex.Replace(email!, @"(@)(.+)$", DomainMapper,
                    RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                EnrichFailedResult(ref result);
            }
            catch (ArgumentException)
            {
                EnrichFailedResult(ref result);
            }

            try
            {
                if (!Regex.IsMatch(email!,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                {
                    EnrichFailedResult(ref result);
                }
            }
            catch (RegexMatchTimeoutException)
            {
                EnrichFailedResult(ref result);
            }
        }

        private static void EnrichFailedResult(ref PdrValidationResult result)
        {
            result.PassedValidation = false;
            result.Errors.Add("Email must be a valid email address");
        }
    }
}