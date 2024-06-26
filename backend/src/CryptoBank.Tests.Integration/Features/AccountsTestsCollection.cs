﻿using CryptoBank.Tests.Integration.Fixtures;

namespace CryptoBank.Tests.Integration.Features;

[CollectionDefinition(Name)]
public class AccountsTestsCollection : ICollectionFixture<TestFixture>
{
    public const string Name = nameof(AccountsTestsCollection);

    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
