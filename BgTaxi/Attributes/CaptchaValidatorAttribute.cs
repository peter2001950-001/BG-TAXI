using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace BgTaxi.Attributes
{
    public class CaptchaValidatorAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        private const string CHALLENGE_FIELD_KEY = "recaptcha_challenge_field";
        private const string RESPONSE_FIELD_KEY = "recaptcha_response_field";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var captchaChallengeValue = filterContext.HttpContext.Request.Form["g-recaptcha-response"];

            var client = new WebClient();
            var reply =
                client.DownloadString(
                    string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", "6LcjhQ4UAAAAAPU7QwCCq0TQlpHW2EBiamtFfRbt", captchaChallengeValue));

            var captchaResponse = JsonConvert.DeserializeObject<CaptchaResponse>(reply);
            filterContext.ActionParameters["captchaValid"] = captchaResponse.Success;

            base.OnActionExecuting(filterContext);
        }
        public class CaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }
    }
}