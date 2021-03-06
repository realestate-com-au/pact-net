﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using PactNet.Mocks.MockHttpService.Comparers;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Models;
using PactNet.Reporters;

namespace PactNet.Mocks.MockHttpService.Validators
{
    public class ProviderServiceValidator : IProviderServiceValidator
    {
        private readonly IProviderServiceResponseComparer _providerServiceResponseComparer;
        private readonly HttpClient _httpClient;
        private readonly IHttpRequestMessageMapper _httpRequestMessageMapper;
        private readonly IProviderServiceResponseMapper _providerServiceResponseMapper;
        private readonly IReporter _reporter;
        
        [Obsolete("For testing only.")]
        public ProviderServiceValidator(
            IProviderServiceResponseComparer providerServiceResponseComparer, 
            HttpClient httpClient, 
            IHttpRequestMessageMapper httpRequestMessageMapper,
            IProviderServiceResponseMapper providerServiceResponseMapper,
            IReporter reporter)
        {
            _providerServiceResponseComparer = providerServiceResponseComparer;
            _httpClient = httpClient;
            _httpRequestMessageMapper = httpRequestMessageMapper;
            _providerServiceResponseMapper = providerServiceResponseMapper;
            _reporter = reporter;
        }

        public ProviderServiceValidator(HttpClient httpClient, IReporter reporter) : this(
            new ProviderServiceResponseComparer(reporter),
            httpClient,
            new HttpRequestMessageMapper(),
            new ProviderServiceResponseMapper(),
            reporter)
        {
        }

        public void Validate(ProviderServicePactFile pactFile, ProviderStates providerStates)
        {
            if (pactFile == null)
            {
                throw new ArgumentException("Please supply a non null pactFile");
            }

            if (pactFile.Consumer == null || String.IsNullOrEmpty(pactFile.Consumer.Name))
            {
                throw new ArgumentException("Please supply a non null or empty Consumer name in the pactFile");
            }

            if (pactFile.Provider == null || String.IsNullOrEmpty(pactFile.Provider.Name))
            {
                throw new ArgumentException("Please supply a non null or empty Provider name in the pactFile");
            }

            if (pactFile.Interactions != null && pactFile.Interactions.Any())
            {
                InvokePactSetUpIfApplicable(providerStates);

                var interationNumber = 1;
                try //TODO: Clean this up once the validators/comparers no longer throw
                {
                    foreach (var interaction in pactFile.Interactions)
                    {
                        ProviderState providerStateItem = null;

                        if (interaction.ProviderState != null)
                        {
                            try
                            {
                                providerStateItem = providerStates.Find(interaction.ProviderState);
                            }
                            catch (Exception)
                            {
                                providerStateItem = null;
                            }

                            if (providerStateItem == null)
                            {
                                throw new InvalidOperationException(String.Format("providerState \"{0}\" was defined by a consumer, however could not be found. Please supply this provider state.", interaction.ProviderState));
                            }
                        }

                        InvokeInteractionSetUpIfApplicable(providerStateItem);

                        _reporter.ReportInfo(String.Format("{0}) Verifying a Pact between {1} and {2} - {3}.", interationNumber, pactFile.Consumer.Name, pactFile.Provider.Name, interaction.Description));

                        try
                        {
                            ValidateInteraction(interaction);
                        }
                        finally
                        {
                            InvokeInteractionIfApplicable(providerStateItem);
                        }
                        
                        interationNumber++;
                    }

                    _reporter.ThrowIfAnyErrors();
                }
                finally 
                {
                    InvokeTearDownIfApplicable(providerStates);
                }
            }
        }

        private void ValidateInteraction(ProviderServiceInteraction interaction)
        {
            var request = _httpRequestMessageMapper.Convert(interaction.Request);

            var response = _httpClient.SendAsync(request, CancellationToken.None).Result;

            var expectedResponse = interaction.Response;
            var actualResponse = _providerServiceResponseMapper.Convert(response);

            _providerServiceResponseComparer.Compare(expectedResponse, actualResponse);
        }

        private void InvokePactSetUpIfApplicable(ProviderStates providerStates)
        {
            if (providerStates != null && providerStates.SetUp != null)
            {
                providerStates.SetUp();
            }
        }

        private void InvokeTearDownIfApplicable(ProviderStates providerStates)
        {
            if (providerStates != null && providerStates.TearDown != null)
            {
                providerStates.TearDown();
            }
        }

        private void InvokeInteractionSetUpIfApplicable(ProviderState providerState)
        {
            if (providerState != null && providerState.SetUp != null)
            {
                providerState.SetUp();
            }
        }

        private void InvokeInteractionIfApplicable(ProviderState providerState)
        {
            if (providerState != null && providerState.TearDown != null)
            {
                providerState.TearDown();
            }
        }
    }
}
