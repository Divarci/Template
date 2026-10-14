using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using AwesomeAssertions;
using Core.Library.Contracts.UnitOfWorks;
using Xunit;

namespace Architecture.Tests.Calls;

public sealed class CallRulesTests
{
    [Fact]
    public void Controllers_AnyController_DependOnNoServiceOrRepositoryInterface()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.Controllers
            .Should()
            .NotDependOnAny(ApiArchitecture.ServiceInterfaces)
            .AndShould()
            .NotDependOnAny(ApiArchitecture.RepositoryInterfaces);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UseCases_AnyUseCase_DependOnNoRepositoryInterface()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.UseCaseImplementations
            .Should()
            .NotDependOnAny(ApiArchitecture.RepositoryInterfaces);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UseCases_AnyUseCase_DependOnNoOtherUseCaseInterface()
    {
        // Arrange
        Interface[] useCaseInterfaces = [.. ApiArchitecture.UseCaseInterfaces.GetObjects(ApiArchitecture.Instance)];
        IArchRule rule = ArchitectureRules.EachMustNotDependOnPeers(
            ApiArchitecture.UseCaseImplementations.GetObjects(ApiArchitecture.Instance),
            useCase => useCaseInterfaces.Where(i => !useCase.ImplementedInterfaces.Any(own => own.FullName == i.FullName)));

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Services_AnyService_DependOnNoOtherService()
    {
        // Arrange
        Interface[] entityServiceInterfaces = [.. ApiArchitecture.EntityServiceInterfaces.GetObjects(ApiArchitecture.Instance)];
        Class[] services = [.. ApiArchitecture.ServiceImplementations.GetObjects(ApiArchitecture.Instance)];
        IArchRule rule = ArchitectureRules.EachMustNotDependOnPeers(
            services,
            service => entityServiceInterfaces
                .Where(i => !service.ImplementedInterfaces.Any(own => own.FullName == i.FullName))
                .Cast<IType>()
                .Concat(services.Where(other => other.FullName != service.FullName)));

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Services_AnyService_DependOnNoUseCase()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.ServiceImplementations
            .Should()
            .NotDependOnAny(ApiArchitecture.UseCaseInterfaces)
            .AndShould()
            .NotDependOnAny(ApiArchitecture.UseCaseImplementations);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Services_AnyService_DoNotDependOnTransactionalUnitOfWork()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.ServiceImplementations
            .Should()
            .NotDependOnAny(typeof(ITransactionalUnitOfWork));

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void DomainEventHandlers_AnyHandler_DependOnNoRepositoryInterface()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.DomainEventHandlers
            .Should()
            .NotDependOnAny(ApiArchitecture.RepositoryInterfaces);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }
}
