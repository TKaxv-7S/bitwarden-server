﻿using Bit.Core.AdminConsole.Entities;
using Bit.Core.AdminConsole.OrganizationFeatures.OrganizationUsers;
using Bit.Core.Billing.Enums;
using Bit.Core.Entities;
using Bit.Core.Repositories;
using Bit.Core.Test.AutoFixture.OrganizationFixtures;
using Bit.Test.Common.AutoFixture;
using Bit.Test.Common.AutoFixture.Attributes;
using NSubstitute;
using Xunit;

namespace Bit.Core.Test.AdminConsole.OrganizationFeatures.OrganizationUsers;

[SutProviderCustomize]
public class GetOrganizationUsersManagementStatusQueryTests
{
    [Theory, BitAutoData]
    public async Task GetUsersOrganizationManagementStatusAsync_WithNoUsers_ReturnsEmpty(
        Organization organization,
        SutProvider<GetOrganizationUsersManagementStatusQuery> sutProvider)
    {
        var result = await sutProvider.Sut.GetUsersOrganizationManagementStatusAsync(organization.Id, new List<Guid>());

        Assert.Empty(result);
    }

    [Theory, BitAutoData, OrganizationCustomize(PlanType = PlanType.EnterpriseAnnually)]
    public async Task GetUsersOrganizationManagementStatusAsync_WithEnterprisePlan_Success(
        Organization organization,
        ICollection<OrganizationUser> usersWithClaimedDomain,
        SutProvider<GetOrganizationUsersManagementStatusQuery> sutProvider)
    {
        var userIdWithoutClaimedDomain = Guid.NewGuid();
        var userIdsToCheck = usersWithClaimedDomain.Select(u => u.Id).Concat(new List<Guid> { userIdWithoutClaimedDomain }).ToList();

        sutProvider.GetDependency<IOrganizationRepository>()
            .GetByIdAsync(organization.Id)
            .Returns(organization);

        sutProvider.GetDependency<IOrganizationUserRepository>()
            .GetManyByOrganizationWithClaimedDomainsAsync(organization.Id)
            .Returns(usersWithClaimedDomain);

        var result = await sutProvider.Sut.GetUsersOrganizationManagementStatusAsync(organization.Id, userIdsToCheck);

        Assert.All(usersWithClaimedDomain, ou => Assert.True(result[ou.Id]));
        Assert.False(result[userIdWithoutClaimedDomain]);
    }

    [Theory, BitAutoData, OrganizationCustomize(PlanType = PlanType.TeamsAnnually)]
    public async Task GetUsersOrganizationManagementStatusAsync_WithoutEnterprisePlan_ReturnsAllFalse(
        Organization organization,
        ICollection<OrganizationUser> usersWithClaimedDomain,
        SutProvider<GetOrganizationUsersManagementStatusQuery> sutProvider)
    {
        var userIdWithoutClaimedDomain = Guid.NewGuid();
        var userIdsToCheck = usersWithClaimedDomain.Select(u => u.Id).Concat(new List<Guid> { userIdWithoutClaimedDomain }).ToList();

        sutProvider.GetDependency<IOrganizationRepository>()
            .GetByIdAsync(organization.Id)
            .Returns(organization);

        sutProvider.GetDependency<IOrganizationUserRepository>()
            .GetManyByOrganizationWithClaimedDomainsAsync(organization.Id)
            .Returns(usersWithClaimedDomain);

        var result = await sutProvider.Sut.GetUsersOrganizationManagementStatusAsync(organization.Id, userIdsToCheck);

        Assert.All(result, r => Assert.False(r.Value));
    }

    [Theory, BitAutoData, OrganizationCustomize(PlanType = PlanType.EnterpriseAnnually)]
    public async Task GetUsersOrganizationManagementStatusAsync_WithDisabledOrganization_ReturnsAllFalse(
        Organization organization,
        ICollection<OrganizationUser> usersWithClaimedDomain,
        SutProvider<GetOrganizationUsersManagementStatusQuery> sutProvider)
    {
        organization.Enabled = false;

        var userIdWithoutClaimedDomain = Guid.NewGuid();
        var userIdsToCheck = usersWithClaimedDomain.Select(u => u.Id).Concat(new List<Guid> { userIdWithoutClaimedDomain }).ToList();

        sutProvider.GetDependency<IOrganizationRepository>()
            .GetByIdAsync(organization.Id)
            .Returns(organization);

        sutProvider.GetDependency<IOrganizationUserRepository>()
            .GetManyByOrganizationWithClaimedDomainsAsync(organization.Id)
            .Returns(usersWithClaimedDomain);

        var result = await sutProvider.Sut.GetUsersOrganizationManagementStatusAsync(organization.Id, userIdsToCheck);

        Assert.All(result, r => Assert.False(r.Value));
    }
}
