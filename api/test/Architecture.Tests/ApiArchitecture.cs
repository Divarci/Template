using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Classes;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Interfaces;
using ArchUnitNET.Loader;
using Core.Library.Abstractions;
using Core.Library.Contracts.DomainEvents.Handle;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;
using ReflectionAssembly = System.Reflection.Assembly;

namespace Architecture.Tests;

internal static class ApiArchitecture
{
    public static readonly ImmutableArray<ReflectionAssembly> All =
        [.. AssemblyDiscovery.ApiAssemblyNames().Select(ReflectionAssembly.Load)];

    public static readonly ReflectionAssembly CoreLibrary = Named("Core.Library");
    public static readonly ReflectionAssembly CoreDomain = Named("Core.Domain");
    public static readonly ReflectionAssembly CoreServices = Named("Core.Services");
    public static readonly ReflectionAssembly AppUseCases = Named("App.UseCases");
    public static readonly ReflectionAssembly AppProcessor = Named("App.Processor");
    public static readonly ReflectionAssembly InfraPersistence = Named("Infra.Persistence");
    public static readonly ReflectionAssembly InfraCaching = Named("Infra.Caching");
    public static readonly ReflectionAssembly AdapterRestful = Named("Adapter.Restful");
    public static readonly ReflectionAssembly AdapterScheduler = Named("Adapter.Scheduler");

    public static readonly ImmutableArray<ReflectionAssembly> InfraAssemblies = WithPrefix("Infra.");

    public static readonly ImmutableArray<ReflectionAssembly> AdapterAssemblies = WithPrefix("Adapter.");

    public static TheoryData<string> InfraAssemblyNames =>
        new(InfraAssemblies.Select(assembly => assembly.GetName().Name!));

    public static readonly ArchUnitArchitecture Instance = new ArchLoader()
        .LoadAssemblies([.. All])
        .Build();

    public static GivenTypesConjunction TypesIn(params ReflectionAssembly[] assemblies) =>
        Types()
            .That()
            .ResideInAssembly(assemblies[0], assemblies[1..]);

    public static GivenTypesConjunction ApiTypesExcept(params ReflectionAssembly[] excluded) =>
        TypesIn([.. All.Except(excluded)]);

    public static GivenInterfacesConjunction ApiInterfaces =>
        Interfaces()
            .That()
            .ResideInAssembly(All[0], [.. All[1..]]);

    public static GivenClassesConjunction ApiClasses =>
        Classes()
            .That()
            .ResideInAssembly(All[0], [.. All[1..]]);

    public static GivenInterfacesConjunction ServiceInterfaces => ApiInterfacesEndingWith("Service");

    public static GivenInterfacesConjunction EntityServiceInterfaces =>
        ServiceInterfaces
            .And()
            .ResideInAssembly(CoreDomain);

    public static GivenInterfacesConjunction UseCaseInterfaces => ApiInterfacesEndingWith("UseCase");

    public static GivenInterfacesConjunction RepositoryInterfaces => ApiInterfacesEndingWith("Repository");

    public static GivenClassesConjunction ServiceImplementations => ApiImplementationsOf(ServiceInterfaces);

    public static GivenClassesConjunction UseCaseImplementations => ApiImplementationsOf(UseCaseInterfaces);

    public static GivenClassesConjunction RepositoryImplementations => ApiImplementationsOf(RepositoryInterfaces);

    public static GivenClassesConjunction DomainEventHandlers =>
        ApiClasses
            .And()
            .AreAssignableTo(typeof(IDomainEventHandler));

    public static GivenClassesConjunction Entities =>
        ApiClasses
            .And()
            .AreAssignableTo(typeof(BaseEntity))
            .And()
            .AreNot(typeof(BaseEntity));

    public static GivenClassesConjunction Controllers =>
        ApiClasses
            .And()
            .AreAssignableTo(typeof(ControllerBase));

    private static ReflectionAssembly Named(string name) =>
        All.SingleOrDefault(assembly => assembly.GetName().Name == name)
            ?? throw new InvalidOperationException($"Api assembly '{name}' was not discovered.");

    private static ImmutableArray<ReflectionAssembly> WithPrefix(string prefix) =>
        [.. All.Where(assembly => assembly.GetName().Name!.StartsWith(prefix, StringComparison.Ordinal))];

    private static GivenInterfacesConjunction ApiInterfacesEndingWith(string suffix) =>
        ApiInterfaces
            .And()
            .HaveNameMatching($@"^I\w*{suffix}(`\d+)?$");

    private static GivenClassesConjunction ApiImplementationsOf(GivenInterfacesConjunction interfaces) =>
        ApiClasses
            .And()
            .ImplementAnyInterfaces(interfaces);
}
