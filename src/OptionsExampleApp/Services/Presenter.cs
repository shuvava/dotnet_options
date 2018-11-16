using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;


namespace OptionsExampleApp.Services
{
    public class Presenter: IPresenter
    {
        private readonly JsonSerializerSettings _settings;


        public Presenter()
        {
            _settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Formatting = Formatting.Indented,
            };

            _settings.Converters.Add(new StringEnumConverter {CamelCaseText = true});
        }
        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }
    }
}
