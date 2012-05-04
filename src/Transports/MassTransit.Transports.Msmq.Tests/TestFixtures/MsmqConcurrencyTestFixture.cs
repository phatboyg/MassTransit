// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Transports.Msmq.Tests.TestFixtures
{
    using System.Data;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using MassTransit.Tests.Saga.StateMachine;
    using NHibernate;
    using NHibernate.Cache;
    using NHibernate.Cfg;
    using NHibernate.Tool.hbm2ddl;
    using NHibernateIntegration.Tests.Sagas;
    using NUnit.Framework;

    [TestFixture, Category("Integration")]
    public class MsmqConcurrentSagaTestFixtureBase :
        MsmqTransactionalEndpointTestFixture
    {
        protected ISessionFactory SessionFactory { get; private set; }

        protected override void EstablishContext()
        {
            SessionFactory = Fluently.Configure()
                .Database(configureForSqlLite)
                .ExposeConfiguration(configureCommonOptions)
                .Mappings(m =>
                    {
                        m.FluentMappings.Add<ConcurrentSagaMap>();
                        m.FluentMappings.Add<ConcurrentLegacySagaMap>();
                    })
                .ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
           
            base.EstablishContext();
        }

        static void configureCommonOptions(Configuration cfg)
        {
            var connectionString = "Data Source=:memory:";
            cfg.SetProperty(Environment.ConnectionString, connectionString)
                .SetProperty(Environment.UseSecondLevelCache, "true")
                .SetProperty(Environment.UseQueryCache, "true")
                .SetProperty(Environment.CacheProvider, typeof (HashtableCacheProvider).AssemblyQualifiedName)
                .AddAssembly(typeof(RegisterUserStateMachine).Assembly)
                .AddAssembly(typeof(SagaRepository_Specs).Assembly);
        }
        static IPersistenceConfigurer configureForSqlLite()
        {
            return SQLiteConfiguration
                .Standard
                .InMemory();
        }

        static IPersistenceConfigurer configureForSql2008()
        {
            return MsSqlConfiguration
                .MsSql2008
                .IsolationLevel(IsolationLevel.RepeatableRead)
                .DefaultSchema("bus");
        }

        static void BuildSchema(Configuration config)
        {
            new SchemaExport(config).Create(false, true);
        }
    }
}