using System;
using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class CompositeHeaderAppender : IHeaderAppender {
        private readonly IHeaderAppender _defaultHeaderAppender;
        private readonly IHeaderAppender _requestTargetHeaderAppender;
        private readonly IHeaderAppender _createdHeaderAppender;
        private readonly IHeaderAppender _expiresHeaderAppender;
        private readonly IHeaderAppender _dateHeaderAppender;

        public CompositeHeaderAppender(
            IHeaderAppender defaultHeaderAppender,
            IHeaderAppender requestTargetHeaderAppender,
            IHeaderAppender createdHeaderAppender,
            IHeaderAppender expiresHeaderAppender,
            IHeaderAppender dateHeaderAppender) {
            _defaultHeaderAppender = defaultHeaderAppender ?? throw new ArgumentNullException(nameof(defaultHeaderAppender));
            _requestTargetHeaderAppender = requestTargetHeaderAppender ?? throw new ArgumentNullException(nameof(requestTargetHeaderAppender));
            _createdHeaderAppender = createdHeaderAppender ?? throw new ArgumentNullException(nameof(createdHeaderAppender));
            _expiresHeaderAppender = expiresHeaderAppender ?? throw new ArgumentNullException(nameof(expiresHeaderAppender));
            _dateHeaderAppender = dateHeaderAppender ?? throw new ArgumentNullException(nameof(dateHeaderAppender));
        }

        public void Append(HeaderName header, StringBuilder sb) {
            if (header == HeaderName.Empty) throw new ValidationException("An empty header name was specified.");
            
            switch (header) {
                case string str when str == HeaderName.PredefinedHeaderNames.RequestTarget:
                    _requestTargetHeaderAppender.Append(header, sb);
                    break;
                case string str when str == HeaderName.PredefinedHeaderNames.Created:
                    _createdHeaderAppender.Append(header, sb);
                    break;
                case string str when str == HeaderName.PredefinedHeaderNames.Expires:
                    _expiresHeaderAppender.Append(header, sb);
                    break;
                case string str when str == HeaderName.PredefinedHeaderNames.Date:
                    _dateHeaderAppender.Append(header, sb);
                    break;
                default:
                    _defaultHeaderAppender.Append(header, sb);
                    break;
            }
        }
    }
}