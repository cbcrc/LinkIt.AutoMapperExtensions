using System;
using System.Collections.Generic;
using Tavis.UriTemplates;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource.UrlTemplates
{
    public class UrlTemplate
    {
        private static Uri BaseUrl=null;
        private static Dictionary<string, string> UrlTemplateOverrides;

        //stle: think about a better solution: NeuroUrlTemplate.Init
        public static void Init(Uri baseUrl, Dictionary<string, string> urlTemplateOverrides)
        {
            //stle: need code contract + assertion lib
            if (baseUrl == null) { throw new ArgumentNullException("baseUrl"); }
            if (BaseUrl != null) { throw new InvalidOperationException("Init once and only once"); }

            BaseUrl = baseUrl;
            UrlTemplateOverrides = urlTemplateOverrides;
        }

        private readonly string _webApiRouteTemplate;
        private readonly IDictionary<string, object> _urlParameters;

        public UrlTemplate(string webApiRouteTemplate, int? id)
        {
            _webApiRouteTemplate = webApiRouteTemplate;
            _urlParameters = new Dictionary<string, object> { { "id", id } };
        }

        public UrlTemplate(string webApiRouteTemplate, IDictionary<string, object> urlParameters)
        {
            _webApiRouteTemplate = webApiRouteTemplate;
            _urlParameters = urlParameters;
        }

        public UrlTemplate SetParameter(string key, object value)
        {
            _urlParameters[key] = value;

            return this;
        }

        public Uri Resolve()
        {
            if (ContainsNullId()) { return null; }

            var urlTemplate = UrlTemplateOverrides.ContainsKey(_webApiRouteTemplate)?
                UrlTemplateOverrides[_webApiRouteTemplate]
                :_webApiRouteTemplate;

            var template = new UriTemplate(urlTemplate)
                .AddParameters(_urlParameters);

            var resolvedTemplate = template.Resolve();

            return new Uri(BaseUrl, resolvedTemplate);
        }

        private bool ContainsNullId()
        {
            if (!_urlParameters.ContainsKey("id"))
            {
                return false;
            }

            if (_urlParameters["id"] == null)
            {
                return true;
            }
            
            // Compromise for backward-compatibility
            var id = _urlParameters["id"] as int?;
            if (id.Value == default(int))
            {
                return true;
            }

            return false;
        }
    }
}