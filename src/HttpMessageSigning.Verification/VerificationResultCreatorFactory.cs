﻿using System;

namespace Dalion.HttpMessageSigning.Verification {
    internal class VerificationResultCreatorFactory : IVerificationResultCreatorFactory {
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;
        
        public VerificationResultCreatorFactory(IClaimsPrincipalFactory claimsPrincipalFactory) {
            _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
        }

        public IVerificationResultCreator Create(Client client, HttpRequestForVerification requestForVerification) {
            return new VerificationResultCreator(client, requestForVerification, _claimsPrincipalFactory);
        }
    }
}