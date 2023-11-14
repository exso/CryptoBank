﻿using CryptoBank.Common.Services;
using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Deposits.Domain;
using CryptoBank.Features.Deposits.Options;
using CryptoBank.Pipeline;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NBitcoin;

using static CryptoBank.Features.Deposits.Errors.Codes.DepositsLogicConflictErrors;

namespace CryptoBank.Features.Deposits.Requests;

public static class GetDepositAddress
{
    [HttpGet("/getDepositAddress")]
    [Authorize]
    public class Endpoint : EndpointWithoutRequest<Response>
    {
        private readonly Dispatcher _dispatcher;
        private readonly UserIdentifierService _userIdentifierService;
        private readonly DepositsOptions _depositsOptions;

        public Endpoint(
            Dispatcher dispatcher, 
            UserIdentifierService userIdentifierService,
            IOptions<DepositsOptions> depositsOptions)
        {
            _dispatcher = dispatcher;
            _userIdentifierService = userIdentifierService;
            _depositsOptions = depositsOptions.Value;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancellationToken)
        {
            var userId = _userIdentifierService.GetUserIdentifier();

            return await _dispatcher.Dispatch(new Request(userId, _depositsOptions.Currency!.Code), cancellationToken);
        }
    }

    public record Request(int UserId, string CurrencyCode) : IRequest<Response>;

    public record Response(string CryptoAddress);

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly Context _context;
        private readonly DepositsOptions _depositsOptions;

        public RequestHandler(Context context, IOptions<DepositsOptions> depositsOptions)
        {
            _context = context;
            _depositsOptions = depositsOptions.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var currency = await _context.Currencies
                .SingleOrDefaultAsync(x => x.Code == request.CurrencyCode, cancellationToken)
                    ?? throw new LogicConflictException("Currency not exist", CurrencyNotExist);

            var existingAddress = await _context.DepositAddresses
                .SingleOrDefaultAsync(x => x.UserId == request.UserId && x.CurrencyId == currency.Id, cancellationToken);

            if (existingAddress is not null)
            {
                return new Response(existingAddress.CryptoAddress);
            }

            var xpub = await _context.Xpubs
                .SingleOrDefaultAsync(x => x.CurrencyId == currency.Id, cancellationToken)
                    ?? throw new LogicConflictException("Xpub not exist", XpubNotExist);

            var lastDerivationIndex = await _context.Variables
                .SingleOrDefaultAsync(x => x.Key == _depositsOptions.DerivationIndex, cancellationToken)
                    ?? throw new LogicConflictException("Derivation index not exist", DerivationIndexNotExist);

            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

            await _context.Variables
                .Where(x => x.Key == _depositsOptions.DerivationIndex)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Value, lastDerivationIndex.Value + 1), cancellationToken);

            var cryptoAddress = await CreateCryptoAddress(currency.Id, request.UserId, xpub, lastDerivationIndex.Value, cancellationToken);

            await tx.CommitAsync(cancellationToken);

            return new Response(cryptoAddress);
        }

        private async Task<string> CreateCryptoAddress(
            int currencyId, 
            int userId, 
            Xpub xpub, 
            int derivationIndex,
            CancellationToken cancellationToken)
        {
            var cryptoAddress = GenerateCryptoAddress(xpub, derivationIndex);

            var depositAddress = new DepositAddress(currencyId, userId, xpub.Id, derivationIndex, cryptoAddress);

            await _context.DepositAddresses.AddAsync(depositAddress, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return cryptoAddress;
        }

        private string GenerateCryptoAddress(Xpub xpub, int derivationIndex)
        {
            var network = GetNetwork();

            var extPubKey = ExtPubKey.Parse(xpub.Value, network).Derive(0, false);
            var derivedPubKey = extPubKey.Derive(derivationIndex, false).PubKey;

            var cryptoAddress = derivedPubKey.GetAddress(ScriptPubKeyType.Segwit, network).ToString();

            return cryptoAddress;
        }

        [Obsolete("TODO дубль, переместить в сервисы")]
        private Network GetNetwork()
        {
            return _depositsOptions.BitcoinNetwork switch
            {
                BitcoinNetwork.MainNet => Network.Main,
                BitcoinNetwork.TestNet => Network.TestNet,
                BitcoinNetwork.RegTest => Network.RegTest,
                _ => throw new ArgumentOutOfRangeException(nameof(_depositsOptions.BitcoinNetwork)),
            };
        }
    }
}
