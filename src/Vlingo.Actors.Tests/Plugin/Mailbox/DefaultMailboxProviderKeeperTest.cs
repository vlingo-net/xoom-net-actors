// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using NSubstitute;
using System;
using Vlingo.Actors.Plugin.Mailbox;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Mailbox
{
    public class DefaultMailboxProviderKeeperTest
    {
        private readonly static string MAILBOX_NAME = "Mailbox-" + Guid.NewGuid().ToString();
        private readonly static int SOME_HASHCODE = Guid.NewGuid().GetHashCode();

        private readonly IMailboxProvider mailboxProvider;
        private readonly IMailboxProviderKeeper keeper;

        public DefaultMailboxProviderKeeperTest()
        {
            mailboxProvider = Substitute.For<IMailboxProvider>();
            keeper = new DefaultMailboxProviderKeeper();
            keeper.Keep(MAILBOX_NAME, false, mailboxProvider);
        }

        [Fact]
        public void TestThatAssignsAMailboxFromTheSpecifiedProvider()
        {
            keeper.AssignMailbox(MAILBOX_NAME, SOME_HASHCODE);
            mailboxProvider.Received(1)
                .ProvideMailboxFor(Arg.Is<int>(x => x == SOME_HASHCODE));
        }

        [Fact]
        public void TestThatKeepingADefaultMailboxOverridesTheCurrentDefault()
        {
            var newDefault = Substitute.For<IMailboxProvider>();
            var name = Guid.NewGuid().ToString();

            keeper.Keep(name, true, newDefault);

            Assert.Equal(name, keeper.FindDefault());
        }

        [Fact]
        public void TestThatClosesAllProviders()
        {
            keeper.Close();

            mailboxProvider.Received(1)
                .Close();
        }

        [Fact]
        public void TestThatIsValidMailboxNameChecksForMailboxExistance()
        {
            Assert.True(keeper.IsValidMailboxName(MAILBOX_NAME));
            Assert.False(keeper.IsValidMailboxName(MAILBOX_NAME + "_does_not_exist"));
        }

        [Fact]
        public void TestThatAssigningAnUnknownMailboxFailsGracefully()
        {
            Assert.Throws<InvalidOperationException>(() => keeper.AssignMailbox(MAILBOX_NAME + "_does_not_exist", SOME_HASHCODE));
        }

        [Fact]
        public void TestThatNoDefaultProviderWillFailGracefully()
        {
            Assert.Throws<InvalidOperationException>(() => new DefaultMailboxProviderKeeper().FindDefault());
        }
    }
}
