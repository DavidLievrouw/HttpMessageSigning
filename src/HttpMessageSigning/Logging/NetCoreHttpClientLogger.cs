using System;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Logging {
    internal class NetCoreHttpClientLogger<TContext> : IHttpClientLogger<TContext> {
        private readonly ILogger<TContext> _logger;

        public NetCoreHttpClientLogger(ILogger<TContext> logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Verbose(string messageTemplate) {
            _logger.LogTrace(messageTemplate);
        }

        public void Verbose<T1>(string messageTemplate, T1 propertyValue) {
            _logger.LogTrace(messageTemplate, propertyValue);
        }

        public void Verbose<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogTrace(messageTemplate, propertyValue0, propertyValue1);
        }

        public void Verbose<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogTrace(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Verbose(string messageTemplate, params object[] propertyValues) {
            _logger.LogTrace(messageTemplate, propertyValues);
        }

        public void Verbose(Exception exception, string messageTemplate) {
            _logger.LogTrace(exception, messageTemplate);
        }

        public void Verbose<T1>(Exception exception, string messageTemplate, T1 propertyValue) {
            _logger.LogTrace(exception, messageTemplate, propertyValue);
        }

        public void Verbose<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogTrace(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        public void Verbose<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogTrace(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues) {
            _logger.LogTrace(exception, messageTemplate, propertyValues);
        }

        public void Debug(string messageTemplate) {
            _logger.LogDebug(messageTemplate);
        }

        public void Debug<T1>(string messageTemplate, T1 propertyValue) {
            _logger.LogDebug(messageTemplate, propertyValue);
        }

        public void Debug<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogDebug(messageTemplate, propertyValue0, propertyValue1);
        }

        public void Debug<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogDebug(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Debug(string messageTemplate, params object[] propertyValues) {
            _logger.LogDebug(messageTemplate, propertyValues);
        }

        public void Debug(Exception exception, string messageTemplate) {
            _logger.LogDebug(exception, messageTemplate);
        }

        public void Debug<T1>(Exception exception, string messageTemplate, T1 propertyValue) {
            _logger.LogDebug(exception, messageTemplate, propertyValue);
        }

        public void Debug<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogDebug(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        public void Debug<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogDebug(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues) {
            _logger.LogDebug(exception, messageTemplate, propertyValues);
        }

        public void Information(string messageTemplate) {
            _logger.LogInformation(messageTemplate);
        }

        public void Information<T1>(string messageTemplate, T1 propertyValue) {
            _logger.LogInformation(messageTemplate, propertyValue);
        }

        public void Information<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogInformation(messageTemplate, propertyValue0, propertyValue1);
        }

        public void Information<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogInformation(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Information(string messageTemplate, params object[] propertyValues) {
            _logger.LogInformation(messageTemplate, propertyValues);
        }

        public void Information(Exception exception, string messageTemplate) {
            _logger.LogInformation(exception, messageTemplate);
        }

        public void Information<T1>(Exception exception, string messageTemplate, T1 propertyValue) {
            _logger.LogInformation(exception, messageTemplate, propertyValue);
        }

        public void Information<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogInformation(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        public void Information<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogInformation(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Information(Exception exception, string messageTemplate, params object[] propertyValues) {
            _logger.LogInformation(exception, messageTemplate, propertyValues);
        }

        public void Warning(string messageTemplate) {
            _logger.LogWarning(messageTemplate);
        }

        public void Warning<T1>(string messageTemplate, T1 propertyValue) {
            _logger.LogWarning(messageTemplate, propertyValue);
        }

        public void Warning<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogWarning(messageTemplate, propertyValue0, propertyValue1);
        }

        public void Warning<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogWarning(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Warning(string messageTemplate, params object[] propertyValues) {
            _logger.LogWarning(messageTemplate, propertyValues);
        }

        public void Warning(Exception exception, string messageTemplate) {
            _logger.LogWarning(exception, messageTemplate);
        }

        public void Warning<T1>(Exception exception, string messageTemplate, T1 propertyValue) {
            _logger.LogWarning(exception, messageTemplate, propertyValue);
        }

        public void Warning<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogWarning(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        public void Warning<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogWarning(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues) {
            _logger.LogWarning(exception, messageTemplate, propertyValues);
        }

        public void Error(string messageTemplate) {
            _logger.LogError(messageTemplate);
        }

        public void Error<T1>(string messageTemplate, T1 propertyValue) {
            _logger.LogError(messageTemplate, propertyValue);
        }

        public void Error<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogError(messageTemplate, propertyValue0, propertyValue1);
        }

        public void Error<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogError(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Error(string messageTemplate, params object[] propertyValues) {
            _logger.LogError(messageTemplate, propertyValues);
        }

        public void Error(Exception exception, string messageTemplate) {
            _logger.LogError(exception, messageTemplate);
        }

        public void Error<T1>(Exception exception, string messageTemplate, T1 propertyValue) {
            _logger.LogError(exception, messageTemplate, propertyValue);
        }

        public void Error<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogError(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        public void Error<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogError(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Error(Exception exception, string messageTemplate, params object[] propertyValues) {
            _logger.LogError(exception, messageTemplate, propertyValues);
        }

        public void Fatal(string messageTemplate) {
            _logger.LogCritical(messageTemplate);
        }

        public void Fatal<T1>(string messageTemplate, T1 propertyValue) {
            _logger.LogCritical(messageTemplate, propertyValue);
        }

        public void Fatal<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogCritical(messageTemplate, propertyValue0, propertyValue1);
        }

        public void Fatal<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogCritical(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Fatal(string messageTemplate, params object[] propertyValues) {
            _logger.LogCritical(messageTemplate, propertyValues);
        }

        public void Fatal(Exception exception, string messageTemplate) {
            _logger.LogCritical(exception, messageTemplate);
        }

        public void Fatal<T1>(Exception exception, string messageTemplate, T1 propertyValue) {
            _logger.LogCritical(exception, messageTemplate, propertyValue);
        }

        public void Fatal<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1) {
            _logger.LogCritical(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        public void Fatal<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) {
            _logger.LogCritical(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues) {
            _logger.LogCritical(exception, messageTemplate, propertyValues);
        }
    }
}