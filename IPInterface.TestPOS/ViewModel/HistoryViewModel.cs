using System;
using System.Text;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel
{
    public class HistoryViewModel
    {
        public HistoryViewModel(string eventText)
        {
            _eventText = eventText;
        }

        public HistoryViewModel(EFTRequest request) : this(request, null, null) { }

        public HistoryViewModel(EFTRequest request, HistoryViewModel baseRequest, HistoryViewModel prevRequest)
        {
            _request = request;
            _baseRequest = baseRequest;
            _prevRequest = prevRequest;
        }

        public HistoryViewModel(EFTResponse response, HistoryViewModel baseRequest, HistoryViewModel prevRequest)
        {
            _response = response;
            _baseRequest = baseRequest;
            _prevRequest = prevRequest;
        }

        private readonly string _eventText;
        private readonly EFTRequest _request;
        private readonly EFTResponse _response;
        private readonly HistoryViewModel _baseRequest;
        private readonly HistoryViewModel _prevRequest;

        private string GetDiff(HistoryViewModel comp) => comp == null ? string.Empty : (TimeStamp - comp.TimeStamp).TotalSeconds.ToString("N3");

        public Type MessageType => _request?.GetType() ?? _response.GetType();

        public DateTime TimeStamp { get; } = DateTime.Now;
        public string Command
        {
            get
            {
                if (_eventText != null)
                    return $"! {_eventText}";

                StringBuilder sb = new StringBuilder();
                sb.Append(_request != null ? "→ " : "← ");

                if (_baseRequest != null)
                    sb.Append(" ");

                string type = MessageType.ToString();
                type = type.Substring(type.LastIndexOf('.') + 1);
                sb.Append(type);

                if (_response is EFTDisplayResponse display)
                    sb.Append($" ({display.DisplayText[0].Trim()})");
                else if (_request is EFTSendKeyRequest key)
                    sb.Append($" ({key.Key}{(!string.IsNullOrEmpty(key.Data) ? $" '{key.Data}'" : "")})");

                return sb.ToString();
            }
        }
        public string Time => TimeStamp.ToString("HH:mm:ss.fff");
        public string Elapsed => GetDiff(_baseRequest);
        public string Diff => _baseRequest == null ? string.Empty : GetDiff(_prevRequest);
    }
}
