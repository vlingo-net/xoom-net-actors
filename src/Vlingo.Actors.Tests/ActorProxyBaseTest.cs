// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Newtonsoft.Json;
using Vlingo.Common.Serialization;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ActorProxyBaseTest
    {
        [Fact]
        public void TestWriteReadJson()
        {
            ActorProxyBase<IProto> proxy = new ActorProxyBaseImpl(
                Definition.Has<Actor>(Definition.NoParameters), new TestAddress(1));

            var serialized = JsonSerialization.Serialized(proxy);
            
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TestAddressConverter());
            var deserialized = JsonSerialization.Deserialized<ActorProxyBaseImpl>(serialized, settings);
            
            Assert.Equal(typeof(IProto), deserialized.Protocol);
            Assert.Equal(typeof(Actor), deserialized.Definition.Type);
            Assert.Equal(Definition.NoParameters, deserialized.Definition.Parameters);
            Assert.Equal(1, deserialized.Address.Id);
        }
    }
    
    interface IProto {}

    [Serializable]
    class ActorProxyBaseImpl : ActorProxyBase<IProto>, IProto
    {
        public ActorProxyBaseImpl(Definition definition, IAddress address)
            : base(Actors.Definition.SerializationProxy<IProto>.From(definition), address)
        {
            
        }
    }
    
    [Serializable]
    class TestAddress : IAddress
    {
        public TestAddress(long id) => Id = id;

        public int CompareTo(IAddress other) => 0;

        public long Id { get; }

        public long IdSequence => Id;

        public string IdSequenceString => IdString;
        
        public string IdString => Id.ToString();

        public T IdTyped<T>(Func<string, T> typeConverter) => default;

        public string Name => null;

        public bool IsDistributable => false;
    }
    
    class TestAddressConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) => serializer.Deserialize(reader, typeof(TestAddress));

        public override bool CanConvert(Type objectType) => objectType == typeof(IAddress);
    }
}