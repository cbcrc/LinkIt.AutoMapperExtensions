using System;
using System.Collections.Generic;
using Tavis.UriTemplates;

namespace LinkIt.AutoMapperExtensions.UrlTemplates
{
    public class UrlTemplate
    {
        private static Uri _baseUrl;
        private static readonly IDictionary<string, string> _urlTemplateOverrides = new Dictionary<string, string>();

        public static Uri BaseUrl
        {
            get { return _baseUrl; }
            set
            {
                if (value == null) { throw new ArgumentNullException("value", "Base URL cannot be null."); }

                _baseUrl = value;
            }
        }

        public static IDictionary<string, string> UrlTemplateOverrides
        {
            get { return _urlTemplateOverrides; }
        }

        private readonly string _webApiRouteTemplate;
        private readonly IDictionary<string, object> _urlParameters;

        public UrlTemplate(string webApiRouteTemplate, object id)
        {
            _webApiRouteTemplate = webApiRouteTemplate;
            _urlParameters = new Dictionary<string, object> { { "id", id } };
        }

        public UrlTemplate(string webApiRouteTemplate, IDictionary<string, object> urlParameters)
        {
            _webApiRouteTemplate = webApiRouteTemplate;
            _urlParameters = urlParameters;
        }

        public UrlTemplate DeepCopy()
        {
            return new UrlTemplate(_webApiRouteTemplate, new Dictionary<string, object>(_urlParameters));
        }

        public UrlTemplate SetParameter(string key, object value)
        {
            _urlParameters[key] = value;

            return this;
        }

        public Uri Resolve()
        {
            if (ContainsNullId()) { return null; }

            var urlTemplate = _urlTemplateOverrides.ContainsKey(_webApiRouteTemplate)?
                _urlTemplateOverrides[_webApiRouteTemplate]
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
            if (id != null && id.Value == default(int))
            {
                return true;
            }

            return false;
        }
    }
}