﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using TinyIoC;
using Nancy.Routing;
using Nancy.BootStrapper;
using Nancy.Tests.Fakes;
using Nancy.BootStrappers.Unity;
using Microsoft.Practices.Unity;
using Nancy.ViewEngines;

namespace Nancy.Tests.Unit
{
    public class FakeUnityNancyBootStrapper : UnityNancyBootStrapper
    {
        public bool RequestContainerConfigured { get; set; }

        public bool ApplicationContainerConfigured { get; set; }

        public IUnityContainer Container { get { return _UnityContainer; } }

        public override void ConfigureRequestContainer(IUnityContainer container)
        {
            base.ConfigureRequestContainer(container);

            RequestContainerConfigured = true;

            container.RegisterType<IFoo, Foo>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDependency, Dependency>(new ContainerControlledLifetimeManager());
        }

        protected override void ConfigureApplicationContainer(IUnityContainer container)
        {
            ApplicationContainerConfigured = true;
            base.ConfigureApplicationContainer(container);
        }
    }

    public class UnityNancyBootStrapperFixture
    {
        private FakeUnityNancyBootStrapper _BootStrapper;

        public UnityNancyBootStrapperFixture()
        {
            _BootStrapper = new FakeUnityNancyBootStrapper();
        }

        [Fact]
        public void GetEngine_ReturnsEngine()
        {
            var result = _BootStrapper.GetEngine();

            result.ShouldNotBeNull();
            result.ShouldBeOfType<INancyEngine>();
        }

        [Fact]
        public void GetAllModules_Returns_As_MultiInstance()
        {
            _BootStrapper.GetEngine();
            var output1 = _BootStrapper.GetAllModules().Where(nm => nm.GetType() == typeof(Fakes.FakeNancyModuleWithBasePath)).FirstOrDefault();
            var output2 = _BootStrapper.GetAllModules().Where(nm => nm.GetType() == typeof(Fakes.FakeNancyModuleWithBasePath)).FirstOrDefault();

            output1.ShouldNotBeNull();
            output2.ShouldNotBeNull();
            output1.ShouldNotBeSameAs(output2);
        }

        [Fact]
        public void GetModuleByKey_Returns_As_MultiInstance()
        {
            _BootStrapper.GetEngine();
            var output1 = _BootStrapper.GetModuleByKey(typeof(Fakes.FakeNancyModuleWithBasePath).FullName);
            var output2 = _BootStrapper.GetModuleByKey(typeof(Fakes.FakeNancyModuleWithBasePath).FullName);

            output1.ShouldNotBeNull();
            output2.ShouldNotBeNull();
            output1.ShouldNotBeSameAs(output2);
        }

        [Fact]
        public void GetAllModules_Configures_Child_Container()
        {
            _BootStrapper.GetEngine();
            _BootStrapper.RequestContainerConfigured = false;

            _BootStrapper.GetAllModules();

            _BootStrapper.RequestContainerConfigured.ShouldBeTrue();
        }

        [Fact]
        public void GetModuleByKey_Configures_Child_Container()
        {
            _BootStrapper.GetEngine();
            _BootStrapper.RequestContainerConfigured = false;

            _BootStrapper.GetModuleByKey(typeof(Fakes.FakeNancyModuleWithBasePath).FullName);

            _BootStrapper.RequestContainerConfigured.ShouldBeTrue();
        }

        [Fact]
        public void GetEngine_ConfigureApplicationContainer_Should_Be_Called()
        {
            _BootStrapper.GetEngine();

            _BootStrapper.ApplicationContainerConfigured.ShouldBeTrue();
        }

        [Fact]
        public void GetEngine_Defaults_Registered_In_Container()
        {
            _BootStrapper.GetEngine();

            _BootStrapper.Container.Resolve<INancyModuleCatalog>();
            _BootStrapper.Container.Resolve<IRouteResolver>();
            _BootStrapper.Container.Resolve<ITemplateEngineSelector>();
            _BootStrapper.Container.Resolve<INancyEngine>();
            _BootStrapper.Container.Resolve<IModuleKeyGenerator>();
            _BootStrapper.Container.Resolve<IRouteCache>();
            _BootStrapper.Container.Resolve<IRouteCacheProvider>();
        }

        [Fact]
        public void Get_Module_By_Key_Gives_Same_Request_Lifetime_Instance_To_Each_Dependency()
        {
            _BootStrapper.GetEngine();

            var result = _BootStrapper.GetModuleByKey(new Nancy.BootStrapper.DefaultModuleKeyGenerator().GetKeyForModuleType(typeof(Fakes.FakeNancyModuleWithDependency))) as Fakes.FakeNancyModuleWithDependency;

            result.FooDependency.ShouldNotBeNull();
            result.FooDependency.ShouldBeSameAs(result.Dependency.FooDependency);
        }

        [Fact]
        public void Get_Module_By_Key_Gives_Different_Request_Lifetime_Instance_To_Each_Call()
        {
            _BootStrapper.GetEngine();

            var result = _BootStrapper.GetModuleByKey(new Nancy.BootStrapper.DefaultModuleKeyGenerator().GetKeyForModuleType(typeof(Fakes.FakeNancyModuleWithDependency))) as Fakes.FakeNancyModuleWithDependency;
            var result2 = _BootStrapper.GetModuleByKey(new Nancy.BootStrapper.DefaultModuleKeyGenerator().GetKeyForModuleType(typeof(Fakes.FakeNancyModuleWithDependency))) as Fakes.FakeNancyModuleWithDependency;

            result.FooDependency.ShouldNotBeNull();
            result2.FooDependency.ShouldNotBeNull();
            result.FooDependency.ShouldNotBeSameAs(result2.FooDependency);
        }
    }
}
