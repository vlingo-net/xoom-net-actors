// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using NSubstitute;
using Vlingo.Xoom.Actors.Plugin.Mailbox;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests.Plugin.Mailbox
{
    public class DefaultMailboxProviderKeeperTest
    {
        private readonly static string MailboxName = "Mailbox-" + Guid.NewGuid().ToString();
        private readonly static int SomeHashcode = Guid.NewGuid().GetHashCode();

        private readonly IMailboxProvider _mailboxProvider;
        private readonly IMailboxProviderKeeper _keeper;

        public DefaultMailboxProviderKeeperTest()
        {
            _mailboxProvider = Substitute.For<IMailboxProvider>();
            _keeper = new DefaultMailboxProviderKeeper();
            _keeper.Keep(MailboxName, false, _mailboxProvider);
        }

        [Fact]
        public void TestThatAssignsAMailboxFromTheSpecifiedProvider()
        {
            _keeper.AssignMailbox(MailboxName, SomeHashcode);
            _mailboxProvider.Received(1)
                .ProvideMailboxFor(Arg.Is<int>(x => x == SomeHashcode));
        }

        [Fact]
        public void TestThatKeepingADefaultMailboxOverridesTheCurrentDefault()
        {
            var newDefault = Substitute.For<IMailboxProvider>();
            var name = Guid.NewGuid().ToString();

            _keeper.Keep(name, true, newDefault);

            Assert.Equal(name, _keeper.FindDefault());
        }

        [Fact]
        public void TestThatClosesAllProviders()
        {
            _keeper.Close();

            _mailboxProvider.Received(1)
                .Close();
        }

        [Fact]
        public void TestThatIsValidMailboxNameChecksForMailboxExistance()
        {
            Assert.True(_keeper.IsValidMailboxName(MailboxName));
            Assert.False(_keeper.IsValidMailboxName(MailboxName + "_does_not_exist"));
        }

        [Fact]
        public void TestThatAssigningAnUnknownMailboxFailsGracefully()
        {
            Assert.Throws<InvalidOperationException>(() => _keeper.AssignMailbox(MailboxName + "_does_not_exist", SomeHashcode));
        }

        [Fact]
        public void TestThatNoDefaultProviderWillFailGracefully()
        {
            Assert.Throws<InvalidOperationException>(() => new DefaultMailboxProviderKeeper().FindDefault());
        }
    }
}
