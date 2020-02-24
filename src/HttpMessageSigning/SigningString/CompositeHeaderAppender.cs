using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class CompositeHeaderAppender : IHeaderAppender {
        private readonly IHeaderAppender _defaultHeaderAppender;
        private readonly IHeaderAppender _requestTargetHeaderAppender;
        private readonly IHeaderAppender _createdHeaderAppender;
        private readonly IHeaderAppender _expiresHeaderAppender;
        private readonly IHeaderAppender _dateHeaderAppender;
        private readonly IHeaderAppender _digestHeaderAppender;

        public CompositeHeaderAppender(
            IHeaderAppender defaultHeaderAppender,
            IHeaderAppender requestTargetHeaderAppender,
            IHeaderAppender createdHeaderAppender,
            IHeaderAppender expiresHeaderAppender,
            IHeaderAppender dateHeaderAppender,
            IHeaderAppender digestHeaderAppender) {
            _defaultHeaderAppender = defaultHeaderAppender ?? throw new ArgumentNullException(nameof(defaultHeaderAppender));
            _requestTargetHeaderAppender = requestTargetHeaderAppender ?? throw new ArgumentNullException(nameof(requestTargetHeaderAppender));
            _createdHeaderAppender = createdHeaderAppender ?? throw new ArgumentNullException(nameof(createdHeaderAppender));
            _expiresHeaderAppender = expiresHeaderAppender ?? throw new ArgumentNullException(nameof(expiresHeaderAppender));
            _dateHeaderAppender = dateHeaderAppender ?? throw new ArgumentNullException(nameof(dateHeaderAppender));
            _digestHeaderAppender = digestHeaderAppender ?? throw new ArgumentNullException(nameof(digestHeaderAppender));
        }

        public string BuildStringToAppend(HeaderName header) {
            if (header == HeaderName.Empty) throw new HttpMessageSigningValidationException("An empty header name was specified.");
            
            switch (header) {
                case string str when str == HeaderName.PredefinedHeaderNames.RequestTarget:
                    return _requestTargetHeaderAppender.BuildStringToAppend(header);
                case string str when str == HeaderName.PredefinedHeaderNames.Created:
                    return _createdHeaderAppender.BuildStringToAppend(header);
                case string str when str == HeaderName.PredefinedHeaderNames.Expires:
                    return _expiresHeaderAppender.BuildStringToAppend(header);
                case string str when str == HeaderName.PredefinedHeaderNames.Date:
                    return _dateHeaderAppender.BuildStringToAppend(header);
                case string str when str == HeaderName.PredefinedHeaderNames.Digest:
                    return _digestHeaderAppender.BuildStringToAppend(header);
                default:
                    return _defaultHeaderAppender.BuildStringToAppend(header);
            }
        }
    }
}