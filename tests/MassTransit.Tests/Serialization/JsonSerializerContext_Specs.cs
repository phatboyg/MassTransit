#if NET8_0_OR_GREATER
namespace MassTransit.Tests.Serialization
{
    using System.Threading.Tasks;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TestJsonMessageSerialization;


    [TestFixture]
    public class JsonSerializerContext_Specs
    {
        [Test]
        public async Task Should_not_use_reflection()
        {
            await using var provider = new ServiceCollection()
                .AddMassTransitTestHarness(x =>
                {
                    x.AddConsumer<CommandConsumer>();
                    x.AddConsumer<FaultConsumer>();

                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureJsonSerializerOptions(options =>
                        {
                            options.TypeInfoResolverChain.Insert(0, TestJsonMessageSerializerContext.Default);
                            return options;
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                })
                .BuildServiceProvider(true);

            var harness = await provider.StartTestHarness();

            await harness.Bus.Publish(new FirstCommand("Name", "Value"));

            Assert.That(await harness.Consumed.Any<FirstCommand>(x => x.Exception == null));

            await harness.Bus.Publish(new SecondCommand("Name", "Value"));

            Assert.That(await harness.Consumed.Any<SecondCommand>(x => x.Exception != null));

            Assert.That(await harness.Consumed.Any<Fault<SecondCommand>>(x => x.Exception == null));
        }
    }


    namespace TestJsonMessageSerialization
    {
        using System.Text.Json.Serialization;
        using Events;
        using TestFramework;


        public record FirstCommand(string Name, string Value);


        public record SecondCommand(string Name, string Value);


        public class CommandConsumer :
            IConsumer<FirstCommand>,
            IConsumer<SecondCommand>
        {
            public Task Consume(ConsumeContext<FirstCommand> context)
            {
                return Task.CompletedTask;
            }

            public Task Consume(ConsumeContext<SecondCommand> context)
            {
                throw new IntentionalTestException("Failing to test fault");
            }
        }

        public class FaultConsumer :
            IConsumer<Fault<SecondCommand>>
        {
            public Task Consume(ConsumeContext<Fault<SecondCommand>> context)
            {
                return Task.CompletedTask;
            }
        }


        [JsonSerializable(typeof(FirstCommand))]
        [JsonSerializable(typeof(Fault<FirstCommand>))]
        [JsonSerializable(typeof(FaultEvent<FirstCommand>))]
        [JsonSerializable(typeof(SecondCommand))]
        [JsonSerializable(typeof(Fault<SecondCommand>))]
        [JsonSerializable(typeof(FaultEvent<SecondCommand>))]
        partial class TestJsonMessageSerializerContext :
            JsonSerializerContext
        {
        }
    }
}
#endif
